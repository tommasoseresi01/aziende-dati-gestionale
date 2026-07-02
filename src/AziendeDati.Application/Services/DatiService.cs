using AziendeDati.Application.Dtos;
using AziendeDati.Application.Mappings;
using AziendeDati.Domain.Repositories;

namespace AziendeDati.Application.Services;

/// <summary>Implementazione dei casi d'uso su Dati e report.</summary>
public class DatiService : IDatiService
{
    private readonly IDatiRepository _datiRepository;
    private readonly IAziendeRepository _aziendeRepository;

    // Un servizio può dipendere da PIÙ repository: è il posto giusto per
    // orchestrare (qui: verifica esistenza azienda + lettura dati).
    public DatiService(IDatiRepository datiRepository, IAziendeRepository aziendeRepository)
    {
        _datiRepository = datiRepository;
        _aziendeRepository = aziendeRepository;
    }

    public async Task<List<SommaPerCategoriaDto>> GetSommaPerCategoriaAsync(CancellationToken ct = default)
    {
        var righe = await _datiRepository.GetSommaPerCategoriaAsync(ct);

        // Select su una List già in memoria: questo è LINQ to OBJECTS (delegati
        // C#), da non confondere con la query LINQ to Entities del repository
        // che è stata tradotta in SQL. L'ordinamento è già stato fatto dal DB.
        return righe.Select(r => r.ToDto()).ToList();
    }

    public async Task<List<DatoReadDto>?> GetByAziendaAsync(int aziendaId, CancellationToken ct = default)
    {
        // Prima si verifica che l'azienda ESISTA: senza questo controllo non
        // sapremmo distinguere "azienda senza dati" (200, lista vuota) da
        // "azienda inesistente" (404).
        var esiste = await _aziendeRepository.ExistsAsync(aziendaId, ct);
        if (!esiste)
        {
            return null;
        }

        var dati = await _datiRepository.GetByAziendaAsync(aziendaId, ct);
        return dati.Select(d => d.ToReadDto()).ToList();
    }
}
