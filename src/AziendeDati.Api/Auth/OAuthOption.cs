namespace AziendeDati.Api.Auth;

/// <summary>I client OAuth2 registrati (sezione "OAuth" di appsettings.json).</summary>
// Nel Client Credentials Flow i "client" sono APPLICAZIONI (non persone) con
// credenziali proprie. In un authorization server reale starebbero in una
// tabella con secret HASHATI; qui li teniamo in configurazione per semplicità
// didattica (e in produzione i secret andrebbero comunque in Key Vault).
public sealed class OAuthOption
{
    public const string SectionName = "OAuth";

    public List<OAuthClientOption> Clients { get; set; } = [];
}

/// <summary>Un client registrato: credenziali + l'utente applicativo che rappresenta.</summary>
public sealed class OAuthClientOption
{
    public string ClientId { get; set; } = string.Empty;

    public string ClientSecret { get; set; } = string.Empty;

    // Ponte tra il mondo OAuth (client) e il nostro dominio (Utente): il token
    // emesso per questo client avrà "sub" = questo username, e la claims
    // transformation caricherà da qui ruolo e stato Attivo.
    public string Username { get; set; } = string.Empty;
}
