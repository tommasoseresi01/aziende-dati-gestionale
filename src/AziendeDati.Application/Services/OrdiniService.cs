using AziendeDati.Application.Dtos;
using AziendeDati.Application.Mappings;
using AziendeDati.Domain.Exceptions;
using AziendeDati.Domain.Repositories;

namespace AziendeDati.Application.Services;

/// <summary>Implementazione dei casi d'uso sugli Ordini.</summary>
public class OrdiniService : IOrdiniService
{
    private readonly IOrdiniRepository _ordiniRepository;
    private readonly IAziendeRepository _aziendeRepository;

    public OrdiniService(IOrdiniRepository ordiniRepository, IAziendeRepository aziendeRepository)
    {
        _ordiniRepository = ordiniRepository;
        _aziendeRepository = aziendeRepository;
    }

    public async Task<List<OrdineReadDto>> GetAllAsync(CancellationToken ct = default)
    {
        var ordini = await _ordiniRepository.GetAllAsync(ct);
        return ordini.Select(o => o.ToReadDto()).ToList();
    }

    public async Task<OrdineReadDto> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var ordine = await _ordiniRepository.GetByIdAsync(id, ct)
            ?? throw new NotFoundException("Ordine", id);

        return ordine.ToReadDto();
    }

    public async Task<OrdineReadDto> CreateAsync(OrdineCreateDto dto, CancellationToken ct = default)
    {
        // Validazione di COERENZA COI DATI (richiede il DB, non è compito del
        // validator FluentValidation): azienda inesistente → ValidationException
        // di dominio, che il gestore globale traduce in 400.
        var aziendaEsiste = await _aziendeRepository.ExistsAsync(dto.AziendaId, ct);
        if (!aziendaEsiste)
        {
            throw new ValidationException($"L'azienda {dto.AziendaId} non esiste.");
        }

        var ordine = dto.ToEntity();

        // Se una CategoriaId delle righe non esiste, la FK del DB rifiuta il
        // salvataggio con DbUpdateException → 409 dal gestore globale
        // (chiuso il TODO della Fase 6: niente più 500).
        await _ordiniRepository.AddAsync(ordine, ct);

        // Ricarichiamo per avere le Categorie delle righe (nomi nel DTO).
        var creato = await _ordiniRepository.GetByIdAsync(ordine.Id, ct);
        return creato!.ToReadDto();
    }
}
