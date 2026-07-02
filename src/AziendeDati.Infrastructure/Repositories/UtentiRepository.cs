using AziendeDati.Domain.Entities;
using AziendeDati.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace AziendeDati.Infrastructure.Repositories;

/// <summary>Implementazione EF Core di IUtentiRepository.</summary>
public class UtentiRepository : IUtentiRepository
{
    private readonly AziendeDbContext _db;

    public UtentiRepository(AziendeDbContext db)
    {
        _db = db;
    }

    public async Task<Utente?> GetByUsernameAsync(string username, CancellationToken ct = default)
    {
        // ATTENZIONE ALLE PRESTAZIONI: questa query gira A OGNI RICHIESTA
        // autenticata (la chiama la claims transformation). AsNoTracking +
        // indice univoco su Username (Fase 2) la rendono leggerissima; in
        // sistemi ad alto traffico si valuterebbe una cache breve (vedi il
        // commento in MyClaimsTransformation).
        return await _db.Utenti
            .AsNoTracking()
            .Include(u => u.Ruolo)
            .FirstOrDefaultAsync(u => u.Username == username, ct);
    }
}
