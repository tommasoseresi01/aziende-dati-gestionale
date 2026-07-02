using System.Security.Claims;
using AziendeDati.Domain.Repositories;
using Microsoft.AspNetCore.Authentication;

namespace AziendeDati.Api.Auth;

/// <summary>
/// Claims transformation: dopo OGNI autenticazione riuscita, arricchisce (o
/// azzera) l'identità con i dati REALI dell'utente presi dal database.
/// </summary>
//
// PERCHÉ SERVE: il JWT è una FOTOGRAFIA scattata all'emissione. Se dopo
// l'emissione il ruolo dell'utente cambia, o l'utente viene DISATTIVATO, il
// token continua a dire il contrario finché non scade (un JWT non si revoca).
// IClaimsTransformation è il gancio di ASP.NET Core per integrare informazioni
// di dominio "fresche" con i claim del token: gira DOPO l'autenticazione e
// PRIMA dell'autorizzazione, quindi le policy vedono i claim già corretti.
//
// ATTENZIONE AL COSTO: TransformAsync gira A OGNI RICHIESTA autenticata →
// una query al DB per richiesta. Qui è accettabile (query leggera su indice
// univoco); ad alto traffico si valuta una cache breve (es. IMemoryCache di
// 30-60 secondi), accettando che la disattivazione faccia effetto con quel
// ritardo. Trade-off classico sicurezza/prestazioni: va DECISO, non ignorato.
//
// NOTA (dalla doc): TransformAsync "può essere chiamata più volte" — non va
// modificato il principal ricevuto, si restituisce un principal NUOVO e
// l'operazione deve essere idempotente.
// Fonte: https://learn.microsoft.com/aspnet/core/security/authentication/claims#extend-or-add-custom-claims-using-iclaimstransformation
public sealed class MyClaimsTransformation : IClaimsTransformation
{
    private readonly IUtentiRepository _utentiRepository;

    // Repository Scoped nel costruttore → anche questa classe è registrata
    // Scoped (regola dei lifetime, Fase 1).
    public MyClaimsTransformation(IUtentiRepository utentiRepository)
    {
        _utentiRepository = utentiRepository;
    }

    public async Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
    {
        // Richieste anonime o identità non autenticate: non c'è nulla da trasformare.
        if (principal.Identity is not ClaimsIdentity identita || !identita.IsAuthenticated)
        {
            return principal;
        }

        // Il "sub" del token è lo Username (MapInboundClaims=false: il claim
        // mantiene il nome originale "sub", nessuna rinomina automatica).
        var username = principal.FindFirst("sub")?.Value;
        if (username is null)
        {
            return principal;
        }

        // QUI il dominio incontra il token: si ricarica l'utente dal DB.
        var utente = await _utentiRepository.GetByUsernameAsync(username);

        // UTENTE INESISTENTE O DISATTIVATO → si NEGA L'ACCESSO restituendo un
        // principal ANONIMO (identità vuota, senza claim): nessuna policy può
        // passare. In pratica l'API risponde 403 Forbidden — l'autenticazione
        // JWT era riuscita (firma valida), è l'AUTORIZZAZIONE che fallisce
        // sull'identità svuotata. È così che "spegniamo" un token ancora
        // formalmente valido (verificato dal vivo: stesso token, Attivo=0 → 403).
        if (utente is null || !utente.Attivo)
        {
            return new ClaimsPrincipal(new ClaimsIdentity());
        }

        // Identità NUOVA (mai modificare quella ricevuta): copiamo tutti i
        // claim TRANNE i "role" del token (potrebbero essere invecchiati) e
        // aggiungiamo il ruolo REALE letto adesso dal DB.
        // Gli ultimi due parametri (nameType, roleType) dicono alla nuova
        // identità quali claim usare per Identity.Name e IsInRole/RequireRole.
        var nuovaIdentita = new ClaimsIdentity(
            identita.Claims.Where(c => c.Type != "role"),
            identita.AuthenticationType,
            nameType: "sub",
            roleType: "role");

        nuovaIdentita.AddClaim(new Claim("role", utente.Ruolo.Nome));

        return new ClaimsPrincipal(nuovaIdentita);
    }
}
