using AziendeDati.Domain.Entities;
using AziendeDati.Domain.ReadModels;

namespace AziendeDati.Domain.Repositories;

/// <summary>Contratto di accesso ai dati per i Dati misurati e i report.</summary>
public interface IDatiRepository
{
    /// <summary>Somma dei Value per categoria, ordinata per somma decrescente.</summary>
    Task<List<SommaPerCategoria>> GetSommaPerCategoriaAsync(CancellationToken ct = default);

    /// <summary>Tutti i dati di un'azienda, con la Categoria caricata (Include).</summary>
    Task<List<Dato>> GetByAziendaAsync(int aziendaId, CancellationToken ct = default);
}
