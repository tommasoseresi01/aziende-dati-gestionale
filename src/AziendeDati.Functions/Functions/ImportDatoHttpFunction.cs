using System.Net;
using System.Text.Json;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

namespace AziendeDati.Functions.Functions;

// ============================================================================
// FASE 10 — AZURE FUNCTIONS: perché esistono e quando convengono.
//
// Una "function" è un pezzo di codice che NON scegli tu quando eseguire: lo
// esegue il runtime quando arriva un EVENTO. La coppia evento→codice è il
// TRIGGER. I trigger principali di Azure Functions:
//
//  • HTTP     → una richiesta HTTP (come un endpoint web). Lo usiamo qui sotto.
//  • Timer    → una pianificazione cron (es. "ogni notte alle 2"): report,
//               pulizie, sincronizzazioni ricorrenti.
//  • Queue    → un messaggio compare in una coda (Azure Storage Queue): lavoro
//               ASINCRONO, elaborato quando c'è tempo. È l'altra function di questa fase.
//  • Blob     → un file viene caricato in un blob storage (es. elaborare un CSV/immagine).
//  • Service Bus → un messaggio su Service Bus (bus enterprise: topic/subscription,
//               transazioni, ordinamento) — la versione "robusta" delle code.
//  (Esistono anche Event Hub, Cosmos DB change feed, Event Grid, ecc.)
//
// QUANDO una Function invece di un endpoint della Web API?
//  - Lavoro DIFFERIBILE/lungo che non deve far aspettare il chiamante (invio email,
//    generazione PDF, import massivi): l'API risponde subito "preso in carico" e
//    una Function lo svolge dietro le quinte.
//  - Lavoro PIANIFICATO (Timer) o REATTIVO a eventi di storage/bus (Queue/Blob/Service Bus).
//  - Scalabilità "a consumo": la piattaforma crea/distrugge istanze da sola e si
//    paga solo l'esecuzione (serverless), utile per carichi a picchi.
// Quando invece serve una risposta sincrona con il dato pronto (CRUD, query,
// login), resta più adatto un endpoint della Web API.
//
// COS'È IL "BINDING": oltre al trigger (input che avvia), una function dichiara
// via ATTRIBUTI le sue connessioni a servizi esterni; il runtime le collega da
// solo, senza che noi apriamo connessioni. Un OUTPUT BINDING scrive il valore
// restituito su un servizio (qui: una coda). È il "binding automatico": niente
// SDK a mano per mettere il messaggio in coda, basta l'attributo [QueueOutput].
// Fonte: https://learn.microsoft.com/azure/azure-functions/functions-triggers-bindings
// ============================================================================

/// <summary>Impostazioni condivise della coda dei Dati da importare.</summary>
internal static class CodaDati
{
    // Nome della coda su Azure Storage (minuscolo, trattini: regole dei nomi coda).
    // È const perché serve come argomento degli attributi [QueueOutput]/[QueueTrigger],
    // che accettano solo costanti di compilazione.
    public const string Nome = "dati-da-importare";

    // Stesse opzioni JSON in scrittura (HTTP) e lettura (coda): "Web" = camelCase
    // e matching case-insensitive, così il round-trip del messaggio non si rompe
    // per una maiuscola.
    public static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);
}

/// <summary>Messaggio scambiato via coda: i dati grezzi di UN dato da importare.</summary>
// DTO "di trasporto": volutamente separato dall'entità Dato del dominio (niente
// Id, niente navigation). Viaggia come testo JSON nella coda.
public sealed class ImportDatoMessage
{
    public decimal Value { get; set; }
    public DateTime? Timestamp { get; set; } // opzionale: se assente, lo mette il consumer
    public int AziendaId { get; set; }
    public int CategoriaId { get; set; }
}

/// <summary>
/// Contenitore per il pattern "OUTPUT MULTIPLI": una function HTTP che, oltre a
/// rispondere al client, scrive anche su una coda.
/// </summary>
public sealed class ImportDatoOutput
{
    // OUTPUT BINDING: il runtime scrive AUTOMATICAMENTE il valore di questa
    // proprietà come messaggio nella coda indicata. Se è null, non accoda nulla
    // (così sul percorso d'errore 400 non finisce spazzatura in coda).
    [QueueOutput(CodaDati.Nome, Connection = "AzureWebJobsStorage")]
    public string? MessaggioInCoda { get; set; }

    // La risposta HTTP restituita al chiamante (riconosciuta perché è di tipo
    // HttpResponseData). Il runtime la manda al client.
    public HttpResponseData Risposta { get; set; } = default!;
}

/// <summary>HTTP trigger: POST /api/import-dato — valida il dato e lo ACCODA.</summary>
public class ImportDatoHttpFunction
{
    private readonly ILogger<ImportDatoHttpFunction> _logger;

    // Le classi delle function sono istanziate dalla Dependency Injection del
    // worker: qui ci basta il logger (l'HTTP trigger NON tocca il database —
    // il suo compito è solo accettare e mettere in coda, in fretta).
    public ImportDatoHttpFunction(ILogger<ImportDatoHttpFunction> logger)
    {
        _logger = logger;
    }

    // [Function("...")] dà il nome logico alla function (compare nei log e nel portale).
    // [HttpTrigger]: AuthorizationLevel.Anonymous = nessuna function key (comodo in
    // locale; in produzione si alza il livello o si mette dietro API Management).
    // Route = "import-dato" + prefisso di default "api" → POST /api/import-dato.
    // Il tipo di ritorno ImportDatoOutput attiva il binding di output verso la coda.
    [Function("ImportDatoHttp")]
    public async Task<ImportDatoOutput> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "import-dato")] HttpRequestData req)
    {
        // BINDING AUTOMATICO IN INGRESSO: ReadFromJsonAsync deserializza il corpo
        // della richiesta nel nostro tipo, usando il serializzatore del worker.
        var messaggio = await req.ReadFromJsonAsync<ImportDatoMessage>();

        // Validazione minima PRIMA di accodare: meglio dire subito 400 al client
        // che accettare un messaggio che il consumer non potrà mai salvare.
        if (messaggio is null || messaggio.AziendaId <= 0 || messaggio.CategoriaId <= 0)
        {
            var errore = req.CreateResponse();
            await errore.WriteAsJsonAsync(
                new { errore = "Corpo non valido: servono Value, AziendaId (>0) e CategoriaId (>0)." });
            // ATTENZIONE (footgun del modello isolated): WriteAsJsonAsync scrive il
            // corpo JSON MA rimette lo status a 200. Quindi lo status "vero" va
            // impostato DOPO la scrittura, altrimenti il client vedrebbe 200.
            errore.StatusCode = HttpStatusCode.BadRequest;
            // MessaggioInCoda = null → nessun accodamento su questo percorso.
            return new ImportDatoOutput { Risposta = errore, MessaggioInCoda = null };
        }

        // Serializziamo il messaggio a JSON: sarà il corpo del messaggio in coda.
        var payload = JsonSerializer.Serialize(messaggio, CodaDati.Json);

        _logger.LogInformation(
            "Ricevuto dato da importare (Azienda={AziendaId}, Categoria={CategoriaId}): accodo su '{Coda}'.",
            messaggio.AziendaId, messaggio.CategoriaId, CodaDati.Nome);

        // 202 Accepted è lo status giusto: "richiesta presa in carico, NON ancora
        // completata". Il salvataggio vero avverrà in modo asincrono nel consumer.
        var risposta = req.CreateResponse();
        await risposta.WriteAsJsonAsync(new { stato = "accodato", coda = CodaDati.Nome });
        risposta.StatusCode = HttpStatusCode.Accepted; // vedi nota sopra: status DOPO il corpo

        return new ImportDatoOutput { Risposta = risposta, MessaggioInCoda = payload };
    }
}
