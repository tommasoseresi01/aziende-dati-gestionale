using AziendeDati.Application.Dtos;
using AziendeDati.Application.Services;
using AziendeDati.Domain.Exceptions;
using AziendeDati.Infrastructure;
using AziendeDati.Infrastructure.Repositories;
using AziendeDati.Tests.TestSupport;

namespace AziendeDati.Tests.Unit;

/// <summary>
/// Test unitari del servizio applicativo CategorieService, montato sul suo
/// repository reale sopra un DbContext InMemory (Fase 11).
/// </summary>
// COSA STIAMO TESTANDO: la CATENA servizio → repository → EF, cioè i casi d'uso
// veri (mapping DTO↔entità compreso), non un mock. È il livello di test più utile
// per un servizio applicativo: verifica il comportamento osservabile, non i dettagli.
// STRUTTURA AAA (Arrange-Act-Assert): predispongo i dati, eseguo l'azione, verifico.
public class CategorieServiceTests
{
    // Piccolo helper: crea servizio + repository sullo stesso contesto InMemory.
    private static (CategorieService service, AziendeDbContext db) CreaServizio()
    {
        var db = InMemoryDb.Create();
        var service = new CategorieService(new CategorieRepository(db));
        return (service, db);
    }

    [Fact]
    public async Task GetByIdAsync_lancia_NotFoundException_se_la_categoria_non_esiste()
    {
        // Arrange
        var (service, db) = CreaServizio();
        await using var _ = db;

        // Act + Assert: il servizio traduce "non trovato" in un'eccezione di dominio
        // (che l'Api mapperà su 404 — vedi GlobalExceptionHandler, Fase 7).
        await Assert.ThrowsAsync<NotFoundException>(() => service.GetByIdAsync(999));
    }

    [Fact]
    public async Task CreateAsync_persiste_la_categoria_e_la_rende_leggibile()
    {
        // Arrange
        var (service, db) = CreaServizio();
        await using var _ = db;

        // Act
        var creata = await service.CreateAsync(new CategoriaCreateDto
        {
            Nome = "Vibrazione",
            Descrizione = "Vibrazioni meccaniche degli impianti"
        });

        // Assert: il DB ha assegnato un Id (> 0) e la rilettura restituisce i dati.
        Assert.True(creata.Id > 0);
        var riletta = await service.GetByIdAsync(creata.Id);
        Assert.Equal("Vibrazione", riletta.Nome);
        Assert.Equal("Vibrazioni meccaniche degli impianti", riletta.Descrizione);
    }

    [Fact]
    public async Task GetAllAsync_restituisce_le_categorie_ordinate_per_nome()
    {
        // Arrange: inserisco in ordine sparso; il repository ordina per Nome.
        var (service, db) = CreaServizio();
        await using var _ = db;
        await service.CreateAsync(new CategoriaCreateDto { Nome = "Zeta" });
        await service.CreateAsync(new CategoriaCreateDto { Nome = "Alfa" });
        await service.CreateAsync(new CategoriaCreateDto { Nome = "Mu" });

        // Act
        var tutte = await service.GetAllAsync();

        // Assert: l'ordinamento alfabetico è rispettato.
        Assert.Equal(new[] { "Alfa", "Mu", "Zeta" }, tutte.Select(c => c.Nome).ToArray());
    }
}
