using AziendeDati.Application.Dtos;
using AziendeDati.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace AziendeDati.Api.Controllers;

/// <summary>CRUD sulle Categorie. Route: /api/categorie (vedi AziendeController per i commenti).</summary>
// Dalla Fase 7 anche qui vale il pattern a eccezioni: il "non trovato" e i
// conflitti sui vincoli non si gestiscono nel controller — ci pensa il
// GlobalExceptionHandler (404 / 409 ProblemDetails).
[ApiController]
[Route("api/[controller]")]
public class CategorieController : ControllerBase
{
    private readonly ICategorieService _service;

    public CategorieController(ICategorieService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<List<CategoriaReadDto>>> GetAll(CancellationToken ct)
    {
        return Ok(await _service.GetAllAsync(ct));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<CategoriaReadDto>> GetById(int id, CancellationToken ct)
    {
        return Ok(await _service.GetByIdAsync(id, ct));
    }

    [HttpPost]
    public async Task<ActionResult<CategoriaReadDto>> Create(CategoriaCreateDto dto, CancellationToken ct)
    {
        var creata = await _service.CreateAsync(dto, ct);
        return CreatedAtAction(nameof(GetById), new { id = creata.Id }, creata);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, CategoriaUpdateDto dto, CancellationToken ct)
    {
        await _service.UpdateAsync(id, dto, ct);
        return NoContent();
    }

    /// <summary>Elimina una categoria (409 se ancora referenziata da dati o righe d'ordine).</summary>
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        await _service.DeleteAsync(id, ct);
        return NoContent();
    }
}
