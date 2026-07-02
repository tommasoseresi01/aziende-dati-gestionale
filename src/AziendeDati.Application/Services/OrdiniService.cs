using AziendeDati.Application.Dtos;
using AziendeDati.Application.Mappings;
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

    public async Task<OrdineReadDto?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var ordine = await _ordiniRepository.GetByIdAsync(id, ct);
        return ordine?.ToReadDto();
    }

    public async Task<OrdineReadDto?> CreateAsync(OrdineCreateDto dto, CancellationToken ct = default)
    {
        // Validazione di ESISTENZA (richiede il DB): non è compito del
        // validator FluentValidation (regole di FORMA sul contratto), è logica
        // applicativa. Confine utile da ricordare: forma → validator,
        // coerenza coi dati → servizio.
        var aziendaEsiste = await _aziendeRepository.ExistsAsync(dto.AziendaId, ct);
        if (!aziendaEsiste)
        {
            return null;
        }

        var ordine = dto.ToEntity();

        // NOTA: se una CategoriaId delle righe non esiste, oggi il DB rifiuta
        // la FK e l'app risponde 500. Nella Fase 7 (gestione eccezioni) la
        // trasformeremo in una risposta pulita.
        await _ordiniRepository.AddAsync(ordine, ct);

        // Ricarichiamo dal repository per avere le Categorie delle righe
        // (servono i nomi nel DTO di risposta).
        var creato = await _ordiniRepository.GetByIdAsync(ordine.Id, ct);
        return creato!.ToReadDto();
    }
}
