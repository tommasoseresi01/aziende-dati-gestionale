using System.Security.Cryptography;
using System.Text;
using AziendeDati.Api.Auth;
using AziendeDati.Domain.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace AziendeDati.Api.Controllers;

/// <summary>
/// Endpoint OAuth2 di emissione token: POST /connect/token (Client Credentials Flow).
/// </summary>
//
// COS'È IL CLIENT CREDENTIALS FLOW (OAuth 2.0, RFC 6749 §4.4):
// è il flusso MACHINE-TO-MACHINE. Non c'è nessun utente umano davanti a un
// browser: un'APPLICAZIONE (un servizio batch, un altro backend, un job
// schedulato) si autentica DA SOLA presentando le PROPRIE credenziali
// (client_id + client_secret) e riceve un access token con cui chiamare l'API.
//
//   client ──POST /connect/token (client_id, client_secret)──► auth server
//   client ◄────────── { access_token, expires_in } ──────────┘
//   client ──GET /api/aziende (Authorization: Bearer <token>)──► API
//
// QUANDO SI USA: integrazioni server-to-server, demoni, pipeline — ovunque non
// esista un utente da far "loggare".
//
// COME SI DISTINGUE DAGLI ALTRI FLOW OAuth2:
//  - Authorization Code (+ PKCE): c'è un UTENTE che fa login nel browser e
//    CONSENTE l'accesso; il client riceve un code da scambiare col token.
//    È il flow delle web app e SPA (lo incontreremo idealmente in Fase 12).
//  - Device Code: per dispositivi senza browser/tastiera (TV, CLI).
//  - Implicit e Password (ROPC): flow storici ormai DEPRECATI per ragioni di
//    sicurezza — non vanno più usati.
// Client Credentials è l'unico SENZA utente: il "soggetto" è il client stesso.
//
// NOTA DIDATTICA (dalla doc Microsoft): un'API che emette token in proprio va
// bene per imparare/testare; in produzione l'emissione si delega a un
// authorization server dedicato (Microsoft Entra ID, Keycloak, Duende
// IdentityServer). La forma della richiesta qui è quella standard OAuth2:
// POST application/x-www-form-urlencoded con grant_type=client_credentials.
[ApiController]
[Route("connect/token")]
public class TokenController : ControllerBase
{
    private readonly OAuthOption _oauth;
    private readonly IUtentiRepository _utentiRepository;
    private readonly ITokenService _tokenService;

    public TokenController(
        IOptions<OAuthOption> oauthOptions,
        IUtentiRepository utentiRepository,
        ITokenService tokenService)
    {
        _oauth = oauthOptions.Value;
        _utentiRepository = utentiRepository;
        _tokenService = tokenService;
    }

    /// <summary>Emette un JWT dato un client_id/client_secret validi (grant_type=client_credentials).</summary>
    // [AllowAnonymous] è d'obbligo: chi chiede un token, per definizione, non
    // ne ha ancora uno. [FromForm]: OAuth2 usa il form-encoding, non JSON.
    [HttpPost]
    [AllowAnonymous]
    public async Task<IActionResult> Token([FromForm] TokenRequest request, CancellationToken ct)
    {
        // 1. Il flow richiesto è quello che supportiamo? Le risposte d'errore
        //    usano i codici STANDARD di RFC 6749 §5.2 ("error": "..."), così
        //    qualunque client OAuth2 le capisce.
        if (request.GrantType != "client_credentials")
        {
            return BadRequest(new { error = "unsupported_grant_type" });
        }

        // 2. Il client esiste e il secret è giusto?
        var client = _oauth.Clients.FirstOrDefault(c => c.ClientId == request.ClientId);

        // Confronto in TEMPO COSTANTE del secret: il classico "==" esce al
        // primo carattere diverso, e la differenza di tempo misurabile può
        // aiutare un attaccante a indovinare il secret (timing attack).
        if (client is null || !SecretUguali(client.ClientSecret, request.ClientSecret))
        {
            // Non diciamo SE è sbagliato l'id o il secret: meno indizi si
            // danno a un attaccante, meglio è.
            return Unauthorized(new { error = "invalid_client" });
        }

        // 3. Il client rappresenta un utente applicativo: da lui prendiamo
        //    ruolo e azienda per i claim. Utente sparito o disattivato →
        //    niente token (prima linea di difesa; la claims transformation è
        //    la seconda, per i token GIÀ emessi).
        var utente = await _utentiRepository.GetByUsernameAsync(client.Username, ct);
        if (utente is null || !utente.Attivo)
        {
            return Unauthorized(new { error = "invalid_client" });
        }

        // 4. Emissione del JWT firmato (claims: sub, role, azienda_id, exp...).
        var (token, expiresIn) = _tokenService.GeneraToken(utente);

        // Risposta nel formato standard OAuth2 (RFC 6749 §5.1).
        return Ok(new
        {
            access_token = token,
            token_type = "Bearer",
            expires_in = expiresIn
        });
    }

    private static bool SecretUguali(string atteso, string ricevuto)
    {
        return CryptographicOperations.FixedTimeEquals(
            Encoding.UTF8.GetBytes(atteso),
            Encoding.UTF8.GetBytes(ricevuto));
    }
}

/// <summary>Corpo (form-urlencoded) della richiesta token, coi nomi campo standard OAuth2.</summary>
// [FromForm(Name=...)] mappa i nomi snake_case del protocollo sulle proprietà C#.
public sealed class TokenRequest
{
    [FromForm(Name = "grant_type")]
    public string GrantType { get; set; } = string.Empty;

    [FromForm(Name = "client_id")]
    public string ClientId { get; set; } = string.Empty;

    [FromForm(Name = "client_secret")]
    public string ClientSecret { get; set; } = string.Empty;
}
