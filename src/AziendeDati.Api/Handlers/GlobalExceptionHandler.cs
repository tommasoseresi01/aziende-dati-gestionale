using System.Diagnostics;
using AziendeDati.Domain.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AziendeDati.Api.Handlers;

/// <summary>
/// Gestore GLOBALE delle eccezioni: qualunque eccezione non gestita, ovunque
/// nasca (controller, servizi, repository), finisce qui e diventa una risposta
/// JSON ProblemDetails coerente.
/// </summary>
//
// GESTIONE LOCALE vs GLOBALE:
//  - LOCALE (try/catch dentro l'action): serve quando si può RIMEDIARE lì
//    (retry, valore di fallback). Come strategia generale è pessima: catch
//    fotocopiati in ogni action, formati di errore diversi, e basta
//    dimenticarne uno per esporre lo stack trace al client.
//  - GLOBALE (questo handler, agganciato al middleware UseExceptionHandler):
//    UN punto unico che decide formato, status code e logging per TUTTI gli
//    errori. Le action restano pulite: niente try/catch, lanciano e basta.
//
// PERCHÉ IL MIDDLEWARE VA REGISTRATO PER PRIMO nella pipeline: i middleware
// avvolgono quelli successivi come strati di cipolla; l'exception handler può
// catturare SOLO ciò che accade nei middleware a valle di lui. Registrato per
// primo, avvolge tutto (routing, auth, endpoint) e nessuna eccezione gli sfugge.
//
// IExceptionHandler è il meccanismo moderno (.NET 8+) per dare una strategia a
// UseExceptionHandler, al posto della vecchia lambda con ExceptionHandlerApp.
// Fonte: https://learn.microsoft.com/aspnet/core/fundamentals/error-handling
//
// ProblemDetails (RFC 9457) è il formato JSON STANDARD per gli errori HTTP:
// { type, title, status, detail, instance, ... } — lo stesso che [ApiController]
// usa per i 400 di validazione (Fase 6): il client gestisce un formato solo.
public sealed class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;
    private readonly IHostEnvironment _environment;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger, IHostEnvironment environment)
    {
        _logger = logger;
        _environment = environment;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        // MAPPATURA eccezione → risposta HTTP, in un punto solo.
        // Il pattern matching con switch rende la tabella leggibile: aggiungere
        // una nuova eccezione di dominio = aggiungere una riga.
        var (status, title, detail) = exception switch
        {
            // Eccezioni di dominio: il messaggio è pensato per l'utente, si può esporre.
            NotFoundException ex => (StatusCodes.Status404NotFound,
                "Risorsa non trovata", ex.Message),

            ValidationException ex => (StatusCodes.Status400BadRequest,
                "Richiesta non valida", ex.Message),

            // Violazioni di vincoli DB (FK, indici univoci): il client ha
            // chiesto qualcosa che i dati attuali non permettono → 409 Conflict.
            // Il messaggio interno di SQL Server NON si espone (rivela lo schema).
            DbUpdateException => (StatusCodes.Status409Conflict,
                "Conflitto con i dati esistenti",
                "L'operazione viola un vincolo sui dati (es. valore duplicato o risorsa ancora referenziata)."),

            // TUTTO IL RESTO è un bug o un guasto: 500 con messaggio GENERICO.
            // Mai esporre exception.Message qui: potrebbe contenere dettagli
            // interni (connection string, percorsi, schema DB).
            _ => (StatusCodes.Status500InternalServerError,
                "Errore interno del server",
                "Si è verificato un errore imprevisto. Riprovare più tardi.")
        };

        // Gli errori 5xx si LOGGANO SEMPRE con l'eccezione completa (lo stack
        // trace va nei log del server, MAI nella risposta). I 4xx sono
        // comportamento normale dell'API: basta un livello informativo.
        if (status >= 500)
        {
            _logger.LogError(exception, "Eccezione non gestita su {Percorso}", httpContext.Request.Path);
        }
        else
        {
            _logger.LogInformation("Richiesta rifiutata ({Status}) su {Percorso}: {Motivo}",
                status, httpContext.Request.Path, detail);
        }

        var problem = new ProblemDetails
        {
            Title = title,
            Status = status,
            Detail = detail,
            Instance = httpContext.Request.Path
        };

        // traceId: correla la risposta vista dal client con le righe di log del
        // server — l'utente segnala il traceId, lo sviluppatore lo cerca nei log.
        problem.Extensions["traceId"] = Activity.Current?.Id ?? httpContext.TraceIdentifier;

        // SOLO in Development aggiungiamo i dettagli tecnici del 500: in
        // Production lo stack trace non deve MAI uscire (criterio della fase).
        if (_environment.IsDevelopment() && status >= 500)
        {
            problem.Extensions["exceptionType"] = exception.GetType().FullName;
            problem.Extensions["stackTrace"] = exception.StackTrace;
        }

        httpContext.Response.StatusCode = status;
        await httpContext.Response.WriteAsJsonAsync(problem, cancellationToken);

        // true = "eccezione gestita, risposta scritta": il framework non deve
        // fare altro. Con false passerebbe la mano a un eventuale handler successivo.
        return true;
    }
}
