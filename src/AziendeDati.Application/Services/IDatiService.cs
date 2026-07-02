using AziendeDati.Application.Dtos;

namespace AziendeDati.Application.Services;

/// <summary>Casi d'uso su Dati e report (pattern a eccezioni, vedi IAziendeService).</summary>
public interface IDatiService
{
    /// <summary>Report: somma dei Value per categoria, ordinata decrescente.</summary>
    Task<List<SommaPerCategoriaDto>> GetSommaPerCategoriaAsync(CancellationToken ct = default);

    /// <summary>Dati di un'azienda. Lancia NotFoundException se l'azienda non esiste.</summary>
    // Azienda senza dati → lista VUOTA (200 []); azienda inesistente → eccezione (404).
    Task<List<DatoReadDto>> GetByAziendaAsync(int aziendaId, CancellationToken ct = default);
}
