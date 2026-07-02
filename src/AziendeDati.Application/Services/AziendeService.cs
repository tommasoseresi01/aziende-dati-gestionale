using AziendeDati.Application.Dtos;
using AziendeDati.Application.Mappings;
using AziendeDati.Domain.Exceptions;
using AziendeDati.Domain.Repositories;

namespace AziendeDati.Application.Services;

/// <summary>Implementazione dei casi d'uso sulle Aziende.</summary>
public class AziendeService : IAziendeService
{
    private readonly IAziendeRepository _repository;

    public AziendeService(IAziendeRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<AziendaReadDto>> GetAllAsync(CancellationToken ct = default)
    {
        var aziende = await _repository.GetAllAsync(ct);
        return aziende.Select(a => a.ToReadDto()).ToList();
    }

    public async Task<AziendaReadDto> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var azienda = await _repository.GetByIdAsync(id, ct);

        // Il servizio LANCIA l'eccezione di dominio; nessuno qui parla di HTTP.
        // Sarà il GlobalExceptionHandler (in Api) a tradurla in 404 — il
        // servizio resta riusabile anche fuori dal web.
        if (azienda is null)
        {
            throw new NotFoundException("Azienda", id);
        }

        return azienda.ToReadDto();
    }

    public async Task<AziendaReadDto> CreateAsync(AziendaCreateDto dto, CancellationToken ct = default)
    {
        var azienda = dto.ToEntity();

        // La data di registrazione la decide il SERVER (regola di business).
        azienda.DataRegistrazione = DateTime.UtcNow;

        // Se la P.IVA è duplicata, l'indice univoco del DB fa fallire il
        // salvataggio con DbUpdateException → il gestore globale la mappa in
        // 409 Conflict (non più 500 come nelle fasi precedenti).
        await _repository.AddAsync(azienda, ct);
        return azienda.ToReadDto();
    }

    public async Task UpdateAsync(int id, AziendaUpdateDto dto, CancellationToken ct = default)
    {
        var azienda = await _repository.GetByIdAsync(id, ct)
            ?? throw new NotFoundException("Azienda", id);

        dto.ApplyTo(azienda);
        await _repository.UpdateAsync(azienda, ct);
    }

    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        var azienda = await _repository.GetByIdAsync(id, ct)
            ?? throw new NotFoundException("Azienda", id);

        // La cancellazione CASCATA su utenti, dati e ordini (OnDelete, Fase 2).
        await _repository.DeleteAsync(azienda, ct);
    }
}
