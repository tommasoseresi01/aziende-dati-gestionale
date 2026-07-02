using AziendeDati.Domain.Entities;

namespace AziendeDati.Domain.Repositories;

/// <summary>Contratto di accesso ai dati per le Categorie (vedi IAziendeRepository per i principi).</summary>
public interface ICategorieRepository
{
    Task<List<Categoria>> GetAllAsync(CancellationToken ct = default);
    Task<Categoria?> GetByIdAsync(int id, CancellationToken ct = default);
    Task AddAsync(Categoria categoria, CancellationToken ct = default);
    Task UpdateAsync(Categoria categoria, CancellationToken ct = default);
    Task DeleteAsync(Categoria categoria, CancellationToken ct = default);
}
