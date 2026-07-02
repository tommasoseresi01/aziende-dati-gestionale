using AziendeDati.Domain.Entities;
using AziendeDati.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace AziendeDati.Infrastructure.Repositories;

/// <summary>Implementazione EF Core di ICategorieRepository (vedi AziendeRepository per i commenti).</summary>
public class CategorieRepository : ICategorieRepository
{
    private readonly AziendeDbContext _db;

    public CategorieRepository(AziendeDbContext db)
    {
        _db = db;
    }

    public async Task<List<Categoria>> GetAllAsync(CancellationToken ct = default)
    {
        return await _db.Categorie
            .AsNoTracking()
            .OrderBy(c => c.Nome)
            .ToListAsync(ct);
    }

    public async Task<Categoria?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        return await _db.Categorie.FindAsync([id], ct);
    }

    public async Task AddAsync(Categoria categoria, CancellationToken ct = default)
    {
        _db.Categorie.Add(categoria);
        await _db.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(Categoria categoria, CancellationToken ct = default)
    {
        await _db.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(Categoria categoria, CancellationToken ct = default)
    {
        _db.Categorie.Remove(categoria);
        await _db.SaveChangesAsync(ct);
    }
}
