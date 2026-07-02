using AziendeDati.Domain.Entities;
using AziendeDati.Domain.ReadModels;
using AziendeDati.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace AziendeDati.Infrastructure.Repositories;

/// <summary>Query LINQ sui Dati: report aggregato e dati per azienda.</summary>
//
// ============================================================================
// LINQ TO ENTITIES vs LINQ TO OBJECTS — la distinzione più importante di EF.
//
// Una query su un DbSet è un ALBERO DI ESPRESSIONI (IQueryable<T>): NON viene
// eseguita riga per riga in C#, ma TRADOTTA IN SQL da EF Core ed eseguita DAL
// DATABASE. Questo è "LINQ to Entities". Gli stessi operatori (Where, GroupBy,
// Sum...) applicati a una List<T> in memoria sono "LINQ to Objects": delegate
// eseguiti dal processo .NET, riga per riga.
//
// DEFERRED EXECUTION (esecuzione differita): comporre la query NON la esegue.
//   var q = _db.Dati.Where(...).GroupBy(...);   // nessun SQL: solo un "piano"
// L'SQL parte SOLO quando si materializza il risultato:
//   await q.ToListAsync();   // ← QUI parte la query (o con First/Any/Sum/foreach)
// Vantaggio: si può costruire la query a pezzi (filtri condizionali) e il DB
// riceve UNA sola query ottimizzata, con solo le colonne/righe che servono.
//
// L'ERRORE CLASSICO — ToList() troppo presto:
//   (await _db.Dati.ToListAsync())               // scarica TUTTA la tabella!
//       .GroupBy(d => d.CategoriaId)             // ...e aggrega IN MEMORIA
// Da lì in poi è LINQ to Objects: il DB fa da "dispensa" e tutto il lavoro
// (raggruppare, sommare, ordinare) lo fa il processo web. Con 10 righe non si
// nota; con 10 milioni è la differenza tra 5 ms e un server in ginocchio.
// Regola: materializzare (ToListAsync) IL PIÙ TARDI possibile.
//
// COME VEDERE L'SQL GENERATO: vedi il commento su EnableSensitiveDataLogging
// in Program.cs — in Development i log mostrano ogni DbCommand eseguito.
// ============================================================================
public class DatiRepository : IDatiRepository
{
    private readonly AziendeDbContext _db;

    public DatiRepository(AziendeDbContext db)
    {
        _db = db;
    }

    public async Task<List<SommaPerCategoria>> GetSommaPerCategoriaAsync(CancellationToken ct = default)
    {
        // Tutta questa catena è UNA espressione IQueryable: EF la traduce in una
        // sola query con GROUP BY / SUM / ORDER BY eseguita dal DB.
        return await _db.Dati
            // GroupBy sulla chiave composta (Id + Nome della categoria):
            // d.Categoria.Nome NON carica l'entità Categoria — dentro una query
            // LINQ to Entities una navigation diventa semplicemente un JOIN.
            .GroupBy(d => new { d.CategoriaId, d.Categoria.Nome })
            // PROIEZIONE: da ogni gruppo produciamo il read model. g.Sum(...) è
            // un'aggregazione SQL (SUM), non un ciclo in memoria.
            .Select(g => new SommaPerCategoria
            {
                CategoriaId = g.Key.CategoriaId,
                Categoria = g.Key.Nome,
                Somma = g.Sum(d => d.Value)
            })
            .OrderByDescending(x => x.Somma)
            // Fin qui NESSUN SQL è partito (deferred execution):
            // è ToListAsync che esegue la query e materializza i risultati.
            .ToListAsync(ct);
    }

    public async Task<List<Dato>> GetByAziendaAsync(int aziendaId, CancellationToken ct = default)
    {
        // ---------------------------------------------------------------------
        // INCLUDE (eager loading): "quando carichi i Dati, portati dietro anche
        // la Categoria di ciascuno". EF genera UNA query con JOIN:
        //   SELECT d.*, c.* FROM Dati d INNER JOIN Categorie c ON ...
        //
        // ALTERNATIVE e quando convengono:
        //  - Query SEPARATE (una per i Dati + una per le Categorie): con
        //    collection navigation grandi evita la "esplosione cartesiana" del
        //    JOIN (righe padre duplicate per ogni figlio); EF lo fa con
        //    .AsSplitQuery(). Per una reference navigation come questa
        //    (Dato→Categoria, 1 riga a 1 riga) il JOIN singolo è perfetto.
        //  - NIENTE Include e proiezione con Select di soli campi che servono:
        //    spesso la scelta migliore in assoluto (meno dati trasferiti);
        //    qui usiamo Include per mostrare il meccanismo.
        // ERRORE da evitare: dimenticare l'Include e toccare d.Categoria.Nome
        // DOPO la query → null (o lazy loading nascosto, se abilitato: N+1 query).
        // Fonte: https://learn.microsoft.com/ef/core/querying/related-data/eager
        // ---------------------------------------------------------------------
        return await _db.Dati
            .AsNoTracking()
            .Where(d => d.AziendaId == aziendaId)
            .Include(d => d.Categoria)
            .OrderBy(d => d.Timestamp)
            .ToListAsync(ct);

        // NOTA su ThenInclude: serve per il "secondo salto" — es. dall'Ordine:
        //   _db.Ordini.Include(o => o.Righe).ThenInclude(r => r.Categoria)
        // (Ordine → Righe → Categoria). Qui il salto è uno solo, basta Include.
    }
}
