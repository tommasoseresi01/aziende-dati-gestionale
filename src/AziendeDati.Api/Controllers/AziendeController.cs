using AziendeDati.Application.Dtos;
using AziendeDati.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace AziendeDati.Api.Controllers;

/// <summary>CRUD sulle Aziende. Route: /api/aziende.</summary>
// PRINCIPIO "CONTROLLER SOTTILE": il controller fa SOLO da adattatore HTTP —
// riceve la richiesta, chiama il servizio, traduce il risultato in status code.
// NIENTE logica di business, NIENTE DbContext qui dentro: se un domani l'API
// diventasse gRPC o un worker, i servizi si riuserebbero pari pari.
[ApiController]
[Route("api/[controller]")]
public class AziendeController : ControllerBase
{
    private readonly IAziendeService _service;

    public AziendeController(IAziendeService service)
    {
        _service = service;
    }

    /// <summary>Elenco di tutte le aziende.</summary>
    // ActionResult<T> unisce due mondi: si può restituire direttamente il DTO
    // (→ 200) OPPURE un risultato HTTP (NotFound() ecc.). In più documenta il
    // tipo di ritorno per Swagger (Fase 9).
    //
    // Il CancellationToken come parametro dell'action viene collegato DA SOLO
    // ([ApiController]) a HttpContext.RequestAborted: se il client chiude la
    // connessione, il token si "accende" e la query EF si interrompe.
    [HttpGet]
    public async Task<ActionResult<List<AziendaReadDto>>> GetAll(CancellationToken ct)
    {
        var aziende = await _service.GetAllAsync(ct);
        return Ok(aziende);
    }

    /// <summary>Singola azienda per Id.</summary>
    // "{id:int}" è un ROUTE CONSTRAINT: se l'URL non ha un intero (es.
    // /api/aziende/abc) il routing risponde 404 senza nemmeno entrare qui.
    [HttpGet("{id:int}")]
    public async Task<ActionResult<AziendaReadDto>> GetById(int id, CancellationToken ct)
    {
        var azienda = await _service.GetByIdAsync(id, ct);

        // Pattern null → 404: il servizio parla "dominio" (null = non esiste),
        // il controller traduce in HTTP.
        return azienda is null ? NotFound() : Ok(azienda);
    }

    /// <summary>Crea una nuova azienda.</summary>
    // REST: la creazione risponde 201 Created con: (a) l'header Location che
    // punta alla risorsa appena creata (GET /api/aziende/{id}) e (b) la risorsa
    // nel body. CreatedAtAction costruisce tutto questo indicando l'action di
    // lettura e i suoi parametri di route.
    [HttpPost]
    public async Task<ActionResult<AziendaReadDto>> Create(AziendaCreateDto dto, CancellationToken ct)
    {
        var creata = await _service.CreateAsync(dto, ct);
        return CreatedAtAction(nameof(GetById), new { id = creata.Id }, creata);
    }

    /// <summary>Aggiorna un'azienda esistente.</summary>
    // PUT = sostituzione completa della risorsa → risposta 204 No Content
    // (il client ha già tutti i dati, non serve rimandarglieli).
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, AziendaUpdateDto dto, CancellationToken ct)
    {
        var aggiornata = await _service.UpdateAsync(id, dto, ct);
        return aggiornata ? NoContent() : NotFound();
    }

    /// <summary>Elimina un'azienda (a cascata: utenti, dati e ordini collegati).</summary>
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var eliminata = await _service.DeleteAsync(id, ct);
        return eliminata ? NoContent() : NotFound();
    }
}
