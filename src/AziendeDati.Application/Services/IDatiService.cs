using AziendeDati.Application.Dtos;

namespace AziendeDati.Application.Services;

/// <summary>Casi d'uso su Dati e report.</summary>
public interface IDatiService
{
    /// <summary>Report: somma dei Value per categoria, ordinata decrescente.</summary>
    Task<List<SommaPerCategoriaDto>> GetSommaPerCategoriaAsync(CancellationToken ct = default);

    /// <summary>Dati di un'azienda; null se l'azienda non esiste (→ 404).</summary>
    // Distinzione importante: azienda senza dati → lista VUOTA (200 []);
    // azienda inesistente → null (404). Sono due risposte diverse per il client.
    Task<List<DatoReadDto>?> GetByAziendaAsync(int aziendaId, CancellationToken ct = default);
}
