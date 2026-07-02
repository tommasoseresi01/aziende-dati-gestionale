using AziendeDati.Application.Dtos;

namespace AziendeDati.Application.Services;

/// <summary>Casi d'uso applicativi sulle Aziende. È ciò che i controller consumano.</summary>
// Il SERVIZIO parla il linguaggio dei DTO (contratti), il REPOSITORY quello
// delle entità: il servizio sta in mezzo e fa da traduttore + logica.
// I controller NON vedono mai entità né DbContext: solo questa interfaccia.
public interface IAziendeService
{
    Task<List<AziendaReadDto>> GetAllAsync(CancellationToken ct = default);

    /// <summary>null se l'azienda non esiste (il controller la traduce in 404).</summary>
    Task<AziendaReadDto?> GetByIdAsync(int id, CancellationToken ct = default);

    /// <summary>Crea e restituisce il DTO completo di Id generato.</summary>
    Task<AziendaReadDto> CreateAsync(AziendaCreateDto dto, CancellationToken ct = default);

    /// <summary>false se l'azienda non esiste (→ 404).</summary>
    Task<bool> UpdateAsync(int id, AziendaUpdateDto dto, CancellationToken ct = default);

    /// <summary>false se l'azienda non esiste (→ 404).</summary>
    Task<bool> DeleteAsync(int id, CancellationToken ct = default);
}
