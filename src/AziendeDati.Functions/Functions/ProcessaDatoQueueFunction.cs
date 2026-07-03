using System.Text.Json;
using AziendeDati.Domain.Entities;
using AziendeDati.Infrastructure;
using Microsoft.Azure.Functions.Worker;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AziendeDati.Functions.Functions;

/// <summary>
/// Queue trigger: consuma i messaggi della coda "dati-da-importare" e salva ogni
/// Dato nel database della soluzione (lo stesso della Web API).
/// </summary>
// PERCHÉ DUE FUNCTION (HTTP che accoda, Queue che salva) invece di salvare subito
// nell'HTTP trigger? È il pattern "producer/consumer" con una coda in mezzo:
//  - DISACCOPPIAMENTO: il client ottiene subito 202 e non aspetta il salvataggio.
//  - ASSORBIRE I PICCHI: se arrivano 10.000 dati in un secondo, si impilano in
//    coda e vengono processati al ritmo sostenibile dal DB (load leveling).
//  - RESILIENZA: se il salvataggio fallisce, il messaggio NON si perde. Il runtime
//    ci riprova (dequeueCount) e, dopo N tentativi, sposta il messaggio nella coda
//    "<nome>-poison" per analisi — senza bloccare gli altri.
//
// COME SI ATTIVA: il runtime fa polling della coda; appena c'è un messaggio,
// invoca questa function passandoci il testo del messaggio — è di nuovo BINDING
// AUTOMATICO (in ingresso). Qui lo prendiamo come `string` e lo deserializziamo a
// mano per rendere esplicito il passaggio; in alternativa si può dichiarare il
// parametro direttamente come `ImportDatoMessage` e lasciare che sia il runtime a
// deserializzare il JSON.
public class ProcessaDatoQueueFunction
{
    private readonly AziendeDbContext _db;
    private readonly ILogger<ProcessaDatoQueueFunction> _logger;

    // Dependency Injection dell'host Functions: riceviamo il DbContext (Scoped,
    // uno per invocazione) registrato in Program.cs. È lo STESSO tipo che usa la
    // Web API → stesse entità, stesse tabelle, stesso database.
    public ProcessaDatoQueueFunction(AziendeDbContext db, ILogger<ProcessaDatoQueueFunction> logger)
    {
        _db = db;
        _logger = logger;
    }

    // [QueueTrigger]: prima l'argomento è il NOME della coda, poi Connection indica
    // QUALE storage (la stessa impostazione "AzureWebJobsStorage" → Azurite in locale).
    [Function("ImportDatoQueue")]
    public async Task Run(
        [QueueTrigger(CodaDati.Nome, Connection = "AzureWebJobsStorage")] string messaggioJson,
        CancellationToken ct)
    {
        var messaggio = JsonSerializer.Deserialize<ImportDatoMessage>(messaggioJson, CodaDati.Json);
        if (messaggio is null)
        {
            // Messaggio illeggibile: logghiamo e usciamo "con successo" per non
            // rimetterlo in coda all'infinito (è irrecuperabile comunque).
            _logger.LogWarning("Messaggio non deserializzabile, ignorato: {Payload}", messaggioJson);
            return;
        }

        // Le FK devono esistere, altrimenti SaveChanges fallirebbe con errore di
        // vincolo e il messaggio finirebbe "poison". Controllarle prima ci permette
        // di gestire il caso in modo pulito (log + skip) invece di far esplodere EF.
        var aziendaEsiste = await _db.Aziende.AnyAsync(a => a.Id == messaggio.AziendaId, ct);
        var categoriaEsiste = await _db.Categorie.AnyAsync(c => c.Id == messaggio.CategoriaId, ct);
        if (!aziendaEsiste || !categoriaEsiste)
        {
            _logger.LogWarning(
                "Dato scartato: Azienda {AziendaId} o Categoria {CategoriaId} inesistente.",
                messaggio.AziendaId, messaggio.CategoriaId);
            return;
        }

        // Costruiamo l'ENTITÀ di dominio dal messaggio e la salviamo. Timestamp
        // assente → lo mette il consumer (UtcNow): la data "di ingresso" nel sistema.
        var dato = new Dato
        {
            Value = messaggio.Value,
            Timestamp = messaggio.Timestamp ?? DateTime.UtcNow,
            AziendaId = messaggio.AziendaId,
            CategoriaId = messaggio.CategoriaId
        };

        _db.Dati.Add(dato);          // solo tracking in memoria...
        await _db.SaveChangesAsync(ct); // ...qui parte l'INSERT nella transazione

        _logger.LogInformation(
            "Dato salvato dal Queue trigger: Id={Id}, Value={Value}, Azienda={AziendaId}, Categoria={CategoriaId}.",
            dato.Id, dato.Value, dato.AziendaId, dato.CategoriaId);
    }
}
