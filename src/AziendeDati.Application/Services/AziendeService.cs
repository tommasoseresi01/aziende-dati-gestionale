using AziendeDati.Application.Dtos;
using AziendeDati.Application.Mappings;
using AziendeDati.Domain.Repositories;

namespace AziendeDati.Application.Services;

/// <summary>Implementazione dei casi d'uso sulle Aziende.</summary>
// Dipende SOLO dall'astrazione IAziendeRepository (constructor injection, Fase 1):
// non sa se sotto c'è SQL Server, SQLite o un finto repository di test.
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

        // LINQ to Objects (siamo già in memoria): entità → DTO, una per una.
        return aziende.Select(a => a.ToReadDto()).ToList();
    }

    public async Task<AziendaReadDto?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var azienda = await _repository.GetByIdAsync(id, ct);

        // Propaghiamo il "non trovato" come null: sarà il controller a decidere
        // il codice HTTP (404). Il servizio non conosce l'HTTP — separazione dei
        // ruoli. (Nella Fase 7 introdurremo la NotFoundException come alternativa.)
        return azienda?.ToReadDto();
    }

    public async Task<AziendaReadDto> CreateAsync(AziendaCreateDto dto, CancellationToken ct = default)
    {
        var azienda = dto.ToEntity();

        // La data di registrazione la decide il SERVER (regola di business):
        // se arrivasse dal client, ognuno potrebbe scriversi la data che vuole.
        // UtcNow e non Now: i timestamp si salvano in UTC (vedi IClockService).
        // TODO Fase 5+: iniettare un'astrazione del tempo invece di chiamare
        // l'orologio di sistema, per rendere testabile anche questa regola.
        azienda.DataRegistrazione = DateTime.UtcNow;

        await _repository.AddAsync(azienda, ct);

        // Dopo il salvataggio EF ha popolato azienda.Id col valore IDENTITY
        // generato dal DB: il DTO di ritorno è completo.
        return azienda.ToReadDto();
    }

    public async Task<bool> UpdateAsync(int id, AziendaUpdateDto dto, CancellationToken ct = default)
    {
        // Pattern update: carica (tracciata) → applica modifiche → salva.
        // Il change tracker genera l'UPDATE solo per le colonne cambiate.
        var azienda = await _repository.GetByIdAsync(id, ct);
        if (azienda is null)
        {
            return false;
        }

        dto.ApplyTo(azienda);
        await _repository.UpdateAsync(azienda, ct);
        return true;
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken ct = default)
    {
        var azienda = await _repository.GetByIdAsync(id, ct);
        if (azienda is null)
        {
            return false;
        }

        // ATTENZIONE: per come abbiamo configurato OnDelete (Fase 2), la
        // cancellazione di un'Azienda CASCATA su utenti, dati e ordini.
        await _repository.DeleteAsync(azienda, ct);
        return true;
    }
}
