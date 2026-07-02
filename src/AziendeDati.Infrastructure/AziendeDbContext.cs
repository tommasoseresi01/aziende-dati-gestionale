using AziendeDati.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AziendeDati.Infrastructure;

/// <summary>
/// Il DbContext della soluzione: la "porta" verso il database AziendeDati.
/// </summary>
// COSA RAPPRESENTA UN DbContext — due pattern in una classe:
//
//  1. UNIT OF WORK (unità di lavoro): raccoglie TUTTE le modifiche fatte alle
//     entità durante una richiesta (insert, update, delete) e le applica al DB
//     in un colpo solo, dentro UN'UNICA TRANSAZIONE, quando chiamiamo
//     SaveChangesAsync(). O tutto o niente: mai dati a metà.
//
//  2. CHANGE TRACKER: ogni entità letta dal contesto viene "tracciata" — EF si
//     salva una fotografia (snapshot) dei valori originali. Quando modifichiamo
//     una proprietà (azienda.RagioneSociale = "..."), NON parte nessun SQL:
//     stiamo solo cambiando un oggetto in memoria.
//
// QUANDO PARTE L'SQL? Solo in due momenti:
//  - all'ESECUZIONE di una query (es. await ...ToListAsync() — Fase 4);
//  - a SaveChangesAsync(): EF confronta lo stato attuale con lo snapshot,
//    calcola il diff e genera SOLO gli INSERT/UPDATE/DELETE necessari
//    (negli UPDATE aggiorna solo le colonne davvero cambiate).
//
// Usiamo SEMPRE SaveChangesAsync (non SaveChanges): parla col DB, quindi è I/O,
// e il thread non deve restare bloccato ad aspettare la rete (regola CLAUDE.md).
// Fonte: https://learn.microsoft.com/ef/core/saving/basic
public class AziendeDbContext : DbContext
{
    // Le opzioni (stringa di connessione, provider SQL Server, logging...)
    // NON si configurano qui dentro ma arrivano DALL'ESTERNO via costruttore:
    // è la stessa Dependency Injection vista nella Fase 1. Così l'app usa
    // SQL Server e i test potranno passare opzioni diverse (es. SQLite in-memory)
    // senza toccare questa classe.
    public AziendeDbContext(DbContextOptions<AziendeDbContext> options)
        : base(options)
    {
    }

    // Un DbSet<T> per entità: rappresenta "la tabella" su cui fare query e
    // Add/Remove. La forma "=> Set<T>()" (expression body) evita di dichiarare
    // proprietà auto con "= null!": Set<T>() è il metodo ufficiale del DbContext
    // che restituisce il set, sempre non-null.
    public DbSet<Azienda> Aziende => Set<Azienda>();
    public DbSet<Categoria> Categorie => Set<Categoria>();
    public DbSet<Dato> Dati => Set<Dato>();
    public DbSet<Utente> Utenti => Set<Utente>();
    public DbSet<Ruolo> Ruoli => Set<Ruolo>();
    public DbSet<Ordine> Ordini => Set<Ordine>();
    public DbSet<RigaOrdine> RigheOrdine => Set<RigaOrdine>();

    // OnModelCreating: qui EF costruisce il "modello" (il mapping classi→tabelle).
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ApplyConfigurationsFromAssembly cerca via reflection TUTTE le classi
        // di questo assembly che implementano IEntityTypeConfiguration<T>
        // (cartella Configurations/) e le applica.
        // PERCHÉ è meglio che scrivere tutto inline qui dentro:
        //  - con 7+ entità questo metodo diventerebbe un muro di centinaia di
        //    righe illeggibile;
        //  - una classe di configurazione per entità = responsabilità singola,
        //    facile da trovare (AziendaConfiguration configura Azienda);
        //  - aggiungendo una nuova entità non si tocca il DbContext.
        // Fonte: https://learn.microsoft.com/ef/core/modeling/#grouping-configuration
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AziendeDbContext).Assembly);
    }
}
