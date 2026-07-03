using AziendeDati.Domain.Entities;
using AziendeDati.Infrastructure.Repositories;
using AziendeDati.Tests.TestSupport;

namespace AziendeDati.Tests.Unit;

/// <summary>Test della query LINQ di report "somma per categoria" (Fase 11, verifica la Fase 4).</summary>
// COSA VERIFICHIAMO: che GroupBy + Sum + OrderByDescending producano i totali
// GIUSTI e nell'ORDINE giusto (decrescente per somma). Su InMemory la query
// diventa LINQ-to-objects, ma la LOGICA è identica a quella tradotta in SQL:
// è esattamente ciò che vogliamo testare (non il dialetto SQL, ma il risultato).
public class ReportQueryTests
{
    [Fact]
    public async Task GetSommaPerCategoria_somma_per_categoria_e_ordina_per_somma_decrescente()
    {
        // Arrange: due categorie con dati noti.
        //   Temperatura: 10 + 5   = 15
        //   Pressione:   100       = 100   (deve uscire PRIMA: somma più alta)
        await using var db = InMemoryDb.Create();

        var temperatura = new Categoria { Nome = "Temperatura" };
        var pressione = new Categoria { Nome = "Pressione" };
        db.Categorie.AddRange(temperatura, pressione);

        db.Dati.AddRange(
            // Impostando la navigation Categoria, EF valorizza da sé la FK CategoriaId.
            new Dato { Categoria = temperatura, Value = 10m, Timestamp = DateTime.UtcNow, AziendaId = 1 },
            new Dato { Categoria = temperatura, Value = 5m, Timestamp = DateTime.UtcNow, AziendaId = 1 },
            new Dato { Categoria = pressione, Value = 100m, Timestamp = DateTime.UtcNow, AziendaId = 1 });
        await db.SaveChangesAsync();

        var repository = new DatiRepository(db);

        // Act
        var report = await repository.GetSommaPerCategoriaAsync();

        // Assert: due gruppi, ordinati per somma DECRESCENTE, con i totali corretti.
        Assert.Equal(2, report.Count);

        Assert.Equal("Pressione", report[0].Categoria);
        Assert.Equal(100m, report[0].Somma);

        Assert.Equal("Temperatura", report[1].Categoria);
        Assert.Equal(15m, report[1].Somma);
    }
}
