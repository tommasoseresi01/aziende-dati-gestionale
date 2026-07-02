using System.ComponentModel.DataAnnotations;

namespace AziendeDati.Api.Auth;

/// <summary>Impostazioni per emissione e validazione dei token JWT (sezione "Jwt" di appsettings.json).</summary>
// Stesso Options pattern della Fase 5, con validazione all'avvio (Fase 6):
// se manca la chiave di firma l'app NON parte — meglio che scoprirlo al primo login.
public sealed class JwtOption
{
    public const string SectionName = "Jwt";

    // Issuer ("iss"): CHI ha emesso il token. La nostra API fa anche da
    // authorization server, quindi è lei l'issuer.
    [Required]
    public string Issuer { get; set; } = string.Empty;

    // Audience ("aud"): PER CHI è il token. Un token emesso per un'altra API
    // non deve essere accettato qui, anche se firmato dalla stessa chiave.
    [Required]
    public string Audience { get; set; } = string.Empty;

    // Chiave SIMMETRICA di firma (HMAC-SHA256): la stessa chiave firma e
    // verifica. Richiede ALMENO 32 caratteri (256 bit per HS256).
    // ATTENZIONE (nota della doc Microsoft): la firma simmetrica + token emessi
    // in proprio vanno bene per un progetto didattico; in produzione si usa un
    // authorization server (Entra ID, Keycloak, Duende) con chiavi ASIMMETRICHE
    // (privata per firmare, pubblica per verificare) e la chiave NON sta in
    // appsettings ma in User Secrets / Azure Key Vault.
    // Fonte: https://learn.microsoft.com/aspnet/core/security/authentication/configure-jwt-bearer-authentication
    [Required]
    [MinLength(32, ErrorMessage = "Jwt:SigningKey deve avere almeno 32 caratteri (256 bit per HS256).")]
    public string SigningKey { get; set; } = string.Empty;

    // Durata del token: breve per limitare i danni di un token rubato
    // (un access token non si può "revocare": scade e basta).
    [Range(1, 1440)]
    public int ExpirationMinutes { get; set; } = 60;
}
