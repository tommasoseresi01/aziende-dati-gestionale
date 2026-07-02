using AziendeDati.Domain.Entities;

namespace AziendeDati.Domain.Repositories;

/// <summary>Contratto di accesso ai dati per gli Ordini (principi in IAziendeRepository).</summary>
public interface IOrdiniRepository
{
    /// <summary>Tutti gli ordini con righe e categorie caricate.</summary>
    Task<List<Ordine>> GetAllAsync(CancellationToken ct = default);

    /// <summary>Ordine per Id con righe e categorie caricate, o null.</summary>
    Task<Ordine?> GetByIdAsync(int id, CancellationToken ct = default);

    /// <summary>Inserisce ordine E righe in un colpo solo (SaveChanges atomico).</summary>
    Task AddAsync(Ordine ordine, CancellationToken ct = default);
}
