using AziendeDati.Domain.Entities;

namespace AziendeDati.Domain.Repositories;

/// <summary>Contratto di accesso ai dati per gli Utenti (principi in IAziendeRepository).</summary>
public interface IUtentiRepository
{
    /// <summary>Utente per username (con Ruolo caricato), o null. Usato da token endpoint e claims transformation.</summary>
    Task<Utente?> GetByUsernameAsync(string username, CancellationToken ct = default);
}
