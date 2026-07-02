using AziendeDati.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace AziendeDati.Api.Controllers;

/// <summary>
/// Controller di esempio: risponde a GET /api/ping con data/ora dal servizio iniettato.
/// </summary>
//
// ControllerBase vs Controller — quale classe base scegliere?
//   - ControllerBase: la base per le API "pure". Offre tutto ciò che serve per
//     l'HTTP: Ok(), NotFound(), BadRequest(), CreatedAtAction(), ModelState, User...
//   - Controller: ERIDITA da ControllerBase e AGGIUNGE il supporto alle VISTE
//     (View(), ViewBag, ViewData, PartialView...) per le app MVC con pagine HTML
//     renderizzate lato server.
// Qui costruiamo una Web API che restituisce JSON, non HTML: usare Controller
// porterebbe solo dipendenze inutili. Regola: API → ControllerBase, MVC → Controller.
//
// [ApiController] — attiva i comportamenti "da API" (opt-in, non automatici):
//   1. Validazione automatica del modello: se il binding produce un ModelState
//      invalido, il framework risponde 400 Bad Request DA SOLO (lo sfrutteremo
//      nella Fase 6, senza mai scrivere "if (!ModelState.IsValid)").
//   2. Inferenza del binding: capisce da solo che i tipi complessi arrivano dal
//      body JSON, i tipi semplici dalla route/querystring.
//   3. Richiede il routing per attributi ([Route]): niente route "convenzionali".
//
// [Route("api/[controller]")] — routing per attributi. Il token [controller] viene
// sostituito col nome della classe SENZA il suffisso "Controller": quindi
// PingController → "api/ping". PERCHÉ il token invece di scrivere "api/ping" a mano:
// se rinomini la classe, la route resta coerente senza doppie manutenzioni.
[ApiController]
[Route("api/[controller]")]
public class PingController : ControllerBase
{
    // Il campo è readonly: la dipendenza si riceve UNA volta nel costruttore e
    // non cambia mai durante la vita del controller (immutabilità = meno bug).
    private readonly IClockService _clock;

    // CONSTRUCTOR INJECTION — il pattern standard della DI in ASP.NET Core.
    // Non siamo noi a fare "new PingController(...)": a ogni richiesta HTTP è il
    // framework che crea il controller e, vedendo che il costruttore chiede un
    // IClockService, lo risolve dal container DI (dove l'abbiamo registrato in
    // Program.cs). Se il servizio non fosse registrato, otterremmo un errore
    // esplicito a runtime ("Unable to resolve service for type ...").
    // PERCHÉ via costruttore e non "new ClockService()" dentro l'action:
    // il controller resta disaccoppiato dall'implementazione ed è testabile
    // passando un finto IClockService.
    public PingController(IClockService clock)
    {
        _clock = clock;
    }

    /// <summary>Risponde con "pong" e la data/ora corrente.</summary>
    // [HttpGet] senza template: risponde alla route del controller → GET /api/ping.
    // NOTA su async: qui NON c'è I/O (si legge solo l'orologio di sistema), quindi
    // il metodo è sincrono. La regola "async ovunque" vale per il VERO I/O
    // (database, rete, file): rendere async un metodo CPU-only non porta benefici.
    [HttpGet]
    public IActionResult Get()
    {
        // Ok(...) produce una risposta 200 con il corpo serializzato in JSON
        // (System.Text.Json, con nomi in camelCase: Messaggio → "messaggio").
        // Usiamo un tipo anonimo per semplicità: dalla Fase 3 introdurremo
        // DTO espliciti per i contratti dell'API.
        return Ok(new
        {
            Messaggio = "pong",
            DataOra = _clock.Now,       // ora locale del server, con offset
            DataOraUtc = _clock.UtcNow  // ora UTC (quella "da database")
        });
    }
}
