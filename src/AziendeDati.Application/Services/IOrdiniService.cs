using AziendeDati.Application.Dtos;

namespace AziendeDati.Application.Services;

/// <summary>Casi d'uso applicativi sugli Ordini (pattern a eccezioni, vedi IAziendeService).</summary>
public interface IOrdiniService
{
    Task<List<OrdineReadDto>> GetAllAsync(CancellationToken ct = default);

    /// <summary>Lancia NotFoundException se l'ordine non esiste.</summary>
    Task<OrdineReadDto> GetByIdAsync(int id, CancellationToken ct = default);

    /// <summary>Crea ordine + righe. Lancia ValidationException se l'azienda indicata non esiste.</summary>
    Task<OrdineReadDto> CreateAsync(OrdineCreateDto dto, CancellationToken ct = default);
}
