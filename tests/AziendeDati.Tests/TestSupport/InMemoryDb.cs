using AziendeDati.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace AziendeDati.Tests.TestSupport;

/// <summary>Fabbrica di AziendeDbContext col provider InMemory di EF Core, per i test unitari.</summary>
// PERCHÉ InMemory nei test UNITARI: è velocissimo, non richiede SQL Server e ogni
// test parte da un database ISOLATO (nome univoco) → test indipendenti e ripetibili.
//
// LIMITE DA CONOSCERE (importante): InMemory NON è un vero database relazionale.
// Ignora i vincoli (chiavi esterne, indici univoci), i tipi di colonna e le
// transazioni reali. Va benissimo per testare la LOGICA (servizi, mapping, query
// LINQ che diventano LINQ-to-objects); per verificare vincoli/SQL veri servono
// SQLite in-memory o un database reale (es. con Testcontainers).
internal static class InMemoryDb
{
    /// <summary>Nuovo contesto su un database InMemory vuoto e isolato (nome casuale).</summary>
    public static AziendeDbContext Create()
    {
        var options = new DbContextOptionsBuilder<AziendeDbContext>()
            // Nome univoco per test → nessuna condivisione di stato fra test diversi.
            // Non chiamiamo EnsureCreated: così NON viene applicato il seed (HasData)
            // e ogni test controlla ESATTAMENTE i dati che inserisce.
            .UseInMemoryDatabase(databaseName: $"unit-{Guid.NewGuid()}")
            .Options;

        return new AziendeDbContext(options);
    }
}
