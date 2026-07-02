using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AziendeDati.Domain.Entities;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace AziendeDati.Api.Auth;

/// <summary>Emette JWT firmati con HMAC-SHA256, usando le impostazioni della sezione "Jwt".</summary>
public sealed class JwtTokenService : ITokenService
{
    private readonly JwtOption _jwt;

    public JwtTokenService(IOptions<JwtOption> options)
    {
        _jwt = options.Value;
    }

    public (string Token, int ExpiresInSeconds) GeneraToken(Utente utente)
    {
        // I CLAIM: le "affermazioni" su chi è il soggetto, dentro il payload
        // del token. Il token è FIRMATO ma NON cifrato: chiunque può leggerli
        // (base64), quindi mai metterci segreti — la firma garantisce solo che
        // nessuno li ha alterati.
        var claims = new List<Claim>
        {
            // "sub" (subject): l'identità — il nostro Username. È la chiave
            // con cui la claims transformation ricarica l'utente dal DB.
            new(JwtRegisteredClaimNames.Sub, utente.Username),

            // "jti": id univoco del token (utile per audit/blacklist).
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),

            // "role": il ruolo applicativo AL MOMENTO DELL'EMISSIONE.
            // Può invecchiare: se il ruolo cambia nel DB mentre il token è
            // ancora valido, questo claim mente — è il problema che la claims
            // transformation risolve (sostituendolo col ruolo REALE a ogni richiesta).
            new("role", utente.Ruolo.Nome),

            // Claim CUSTOM di dominio: l'azienda di appartenenza. I claim
            // custom si nominano liberamente (convenzione: snake_case).
            new("azienda_id", utente.AziendaId.ToString())
        };

        // La chiave di firma simmetrica + l'algoritmo HMAC-SHA256 ("HS256").
        var chiave = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwt.SigningKey));
        var credenziali = new SigningCredentials(chiave, SecurityAlgorithms.HmacSha256);

        var adesso = DateTime.UtcNow;
        var scadenza = adesso.AddMinutes(_jwt.ExpirationMinutes);

        // Il token: header (alg) + payload (claims, iss, aud, exp, nbf) + firma.
        var token = new JwtSecurityToken(
            issuer: _jwt.Issuer,
            audience: _jwt.Audience,
            claims: claims,
            notBefore: adesso,
            expires: scadenza,          // claim "exp": dopo, il token è spazzatura
            signingCredentials: credenziali);

        // WriteToken serializza nelle tre parti base64 separate da punti:
        // eyJhbGc...(header).eyJzdWI...(payload).firma
        var jwtSerializzato = new JwtSecurityTokenHandler().WriteToken(token);

        return (jwtSerializzato, _jwt.ExpirationMinutes * 60);
    }
}
