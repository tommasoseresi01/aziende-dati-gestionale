using AziendeDati.Domain.Entities;

namespace AziendeDati.Api.Auth;

/// <summary>Emette token JWT firmati per un utente applicativo.</summary>
public interface ITokenService
{
    /// <summary>Genera il JWT e ne restituisce anche la durata in secondi (per "expires_in").</summary>
    (string Token, int ExpiresInSeconds) GeneraToken(Utente utente);
}
