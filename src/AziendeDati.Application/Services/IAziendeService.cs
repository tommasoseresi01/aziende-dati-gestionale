using AziendeDati.Application.Dtos;

namespace AziendeDati.Application.Services;

/// <summary>Casi d'uso applicativi sulle Aziende. È ciò che i controller consumano.</summary>
// Il SERVIZIO parla il linguaggio dei DTO (contratti), il REPOSITORY quello
// delle entità: il servizio sta in mezzo e fa da traduttore + logica.
// I controller NON vedono mai entità né DbContext: solo questa interfaccia.
//
// PATTERN A ECCEZIONI (dalla Fase 7): i metodi non restituiscono più
// null/false per il "non trovato" — lanciano NotFoundException, che il
// gestore globale traduce in 404. Vantaggi rispetto al null: la firma dice la
// verità (GetByIdAsync restituisce SEMPRE un DTO), il chiamante non può
// dimenticarsi il controllo, e il caso d'errore è gestito in un punto solo.
// Rovescio della medaglia (onestà): le eccezioni costano più di un return e
// non vanno usate per flussi "normali" — qui il non-trovato È eccezionale.
public interface IAziendeService
{
    Task<List<AziendaReadDto>> GetAllAsync(CancellationToken ct = default);

    /// <summary>Lancia NotFoundException se l'azienda non esiste.</summary>
    Task<AziendaReadDto> GetByIdAsync(int id, CancellationToken ct = default);

    /// <summary>Crea e restituisce il DTO completo di Id generato.</summary>
    Task<AziendaReadDto> CreateAsync(AziendaCreateDto dto, CancellationToken ct = default);

    /// <summary>Lancia NotFoundException se l'azienda non esiste.</summary>
    Task UpdateAsync(int id, AziendaUpdateDto dto, CancellationToken ct = default);

    /// <summary>Lancia NotFoundException se l'azienda non esiste.</summary>
    Task DeleteAsync(int id, CancellationToken ct = default);
}
