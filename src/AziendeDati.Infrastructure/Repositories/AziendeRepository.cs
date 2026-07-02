using AziendeDati.Domain.Entities;
using AziendeDati.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace AziendeDati.Infrastructure.Repositories;

/// <summary>Implementazione EF Core di IAziendeRepository.</summary>
// Questa classe è l'UNICO posto (insieme agli altri repository) dove si tocca
// il DbContext per le Aziende: i dettagli EF restano confinati in Infrastructure.
public class AziendeRepository : IAziendeRepository
{
    private readonly AziendeDbContext _db;

    public AziendeRepository(AziendeDbContext db)
    {
        _db = db;
    }

    public async Task<List<Azienda>> GetAllAsync(CancellationToken ct = default)
    {
        // AsNoTracking: per le query di SOLA LETTURA il change tracker è lavoro
        // inutile (snapshot mai confrontati) → meno memoria e più velocità.
        // Regola pratica: NoTracking di default nelle letture; tracking solo
        // quando l'entità va modificata e risalvata.
        // Fonte: https://learn.microsoft.com/ef/core/querying/tracking
        return await _db.Aziende
            .AsNoTracking()
            .OrderBy(a => a.RagioneSociale) // ordine stabile: mai affidarsi all'ordine "naturale" del DB
            .ToListAsync(ct);
    }

    public async Task<Azienda?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        // QUI niente AsNoTracking: chi chiama potrebbe modificare l'entità e
        // l'UPDATE funziona proprio grazie al tracking.
        // FindAsync: cerca PRIMA nel change tracker (se già caricata evita la
        // query) e poi nel DB — perfetto per il lookup per chiave primaria.
        return await _db.Aziende.FindAsync([id], ct);
    }

    public async Task AddAsync(Azienda azienda, CancellationToken ct = default)
    {
        // Add segna l'entità come "Added" nel change tracker: NESSUN SQL parte
        // qui. L'INSERT viene generato da SaveChangesAsync, che al ritorno ha
        // anche popolato azienda.Id col valore IDENTITY del DB.
        _db.Aziende.Add(azienda);
        await _db.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(Azienda azienda, CancellationToken ct = default)
    {
        // L'entità è già tracciata (arriva da GetByIdAsync) e già modificata dal
        // servizio: basta salvare. EF confronta con lo snapshot e genera un
        // UPDATE con le SOLE colonne cambiate.
        await _db.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(Azienda azienda, CancellationToken ct = default)
    {
        _db.Aziende.Remove(azienda);
        await _db.SaveChangesAsync(ct);
    }
}
