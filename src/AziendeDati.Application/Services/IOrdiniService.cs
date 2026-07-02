using AziendeDati.Application.Dtos;

namespace AziendeDati.Application.Services;

/// <summary>Casi d'uso applicativi sugli Ordini.</summary>
public interface IOrdiniService
{
    Task<List<OrdineReadDto>> GetAllAsync(CancellationToken ct = default);
    Task<OrdineReadDto?> GetByIdAsync(int id, CancellationToken ct = default);

    /// <summary>Crea ordine + righe; null se l'azienda indicata non esiste.</summary>
    Task<OrdineReadDto?> CreateAsync(OrdineCreateDto dto, CancellationToken ct = default);
}
