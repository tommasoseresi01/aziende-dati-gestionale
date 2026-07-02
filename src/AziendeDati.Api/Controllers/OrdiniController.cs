using AziendeDati.Application.Dtos;
using AziendeDati.Application.Services;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace AziendeDati.Api.Controllers;

/// <summary>CRUD (parziale) sugli Ordini. Route: /api/ordini.</summary>
[ApiController]
[Route("api/[controller]")]
public class OrdiniController : ControllerBase
{
    private readonly IOrdiniService _service;

    // Il validator FluentValidation arriva dalla DI (registrato con
    // AddValidatorsFromAssemblyContaining in Program.cs): il controller chiede
    // l'astrazione IValidator<T>, non la classe concreta.
    private readonly IValidator<OrdineCreateDto> _validator;

    public OrdiniController(IOrdiniService service, IValidator<OrdineCreateDto> validator)
    {
        _service = service;
        _validator = validator;
    }

    [HttpGet]
    public async Task<ActionResult<List<OrdineReadDto>>> GetAll(CancellationToken ct)
    {
        return Ok(await _service.GetAllAsync(ct));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<OrdineReadDto>> GetById(int id, CancellationToken ct)
    {
        var ordine = await _service.GetByIdAsync(id, ct);
        return ordine is null ? NotFound() : Ok(ordine);
    }

    /// <summary>Crea un ordine completo di righe (validato con FluentValidation).</summary>
    [HttpPost]
    public async Task<ActionResult<OrdineReadDto>> Create(OrdineCreateDto dto, CancellationToken ct)
    {
        // VALIDAZIONE ESPLICITA con FluentValidation (approccio raccomandato:
        // l'auto-validazione MVC del vecchio pacchetto è deprecata, vedi
        // OrdineCreateDtoValidator). Tre passi:
        //  1. ValidateAsync esegue TUTTE le regole e raccoglie gli errori;
        //  2. AddToModelState li riversa nel ModelState di MVC;
        //  3. ValidationProblem produce lo STESSO 400 ValidationProblemDetails
        //     che [ApiController] genera per le data annotations → il client
        //     riceve errori nello stesso identico formato, qualunque sia il
        //     motore di validazione usato dietro.
        var risultato = await _validator.ValidateAsync(dto, ct);
        if (!risultato.IsValid)
        {
            foreach (var errore in risultato.Errors)
            {
                ModelState.AddModelError(errore.PropertyName, errore.ErrorMessage);
            }

            return ValidationProblem(ModelState);
        }

        var creato = await _service.CreateAsync(dto, ct);

        // null dal servizio = AziendaId inesistente: non è "forma" ma coerenza
        // coi dati → la segnaliamo comunque come errore di validazione (400).
        if (creato is null)
        {
            ModelState.AddModelError(nameof(dto.AziendaId), $"L'azienda {dto.AziendaId} non esiste.");
            return ValidationProblem(ModelState);
        }

        return CreatedAtAction(nameof(GetById), new { id = creato.Id }, creato);
    }
}
