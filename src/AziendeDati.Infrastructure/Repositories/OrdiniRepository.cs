using AziendeDati.Domain.Entities;
using AziendeDati.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace AziendeDati.Infrastructure.Repositories;

/// <summary>Implementazione EF Core di IOrdiniRepository.</summary>
public class OrdiniRepository : IOrdiniRepository
{
    private readonly AziendeDbContext _db;

    public OrdiniRepository(AziendeDbContext db)
    {
        _db = db;
    }

    public async Task<List<Ordine>> GetAllAsync(CancellationToken ct = default)
    {
        // Ecco ThenInclude in azione (anticipato nella Fase 4): DUE salti di
        // navigazione — Ordine → Righe (collection) → Categoria (reference).
        // EF genera i JOIN necessari per caricare tutto in una query.
        return await _db.Ordini
            .AsNoTracking()
            .Include(o => o.Righe)
                .ThenInclude(r => r.Categoria)
            .OrderBy(o => o.Numero)
            .ToListAsync(ct);
    }

    public async Task<Ordine?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        // Niente FindAsync qui: serve l'Include delle righe, quindi query
        // completa con FirstOrDefaultAsync (null se non esiste).
        return await _db.Ordini
            .AsNoTracking()
            .Include(o => o.Righe)
                .ThenInclude(r => r.Categoria)
            .FirstOrDefaultAsync(o => o.Id == id, ct);
    }

    public async Task AddAsync(Ordine ordine, CancellationToken ct = default)
    {
        // Add sull'ordine traccia ANCHE le righe collegate (grafo di oggetti):
        // un solo SaveChangesAsync = una sola transazione → ordine e righe
        // entrano nel DB atomicamente (o tutto o niente).
        _db.Ordini.Add(ordine);
        await _db.SaveChangesAsync(ct);
    }
}
