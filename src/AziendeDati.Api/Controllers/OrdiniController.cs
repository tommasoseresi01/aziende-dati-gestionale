using AziendeDati.Api.Auth;
using AziendeDati.Application.Dtos;
using AziendeDati.Application.Services;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AziendeDati.Api.Controllers;

/// <summary>CRUD (parziale) sugli Ordini. Route: /api/ordini (letture reader, scritture owner).</summary>
[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = Policies.CompanyReader)]
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
        // NotFoundException → 404 dal gestore globale (Fase 7).
        return Ok(await _service.GetByIdAsync(id, ct));
    }

    /// <summary>Crea un ordine completo di righe (validato con FluentValidation).</summary>
    [HttpPost]
    [Authorize(Policy = Policies.CompanyOwner)]
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

        // Coerenza coi dati (azienda inesistente): dalla Fase 7 il servizio
        // lancia ValidationException → 400 ProblemDetails dal gestore globale.
        var creato = await _service.CreateAsync(dto, ct);
        return CreatedAtAction(nameof(GetById), new { id = creato.Id }, creato);
    }
}
