using AziendeDati.Application.Dtos;
using AziendeDati.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace AziendeDati.Api.Controllers;

/// <summary>CRUD sulle Categorie. Route: /api/categorie (vedi AziendeController per i commenti).</summary>
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
        var categorie = await _service.GetAllAsync(ct);
        return Ok(categorie);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<CategoriaReadDto>> GetById(int id, CancellationToken ct)
    {
        var categoria = await _service.GetByIdAsync(id, ct);
        return categoria is null ? NotFound() : Ok(categoria);
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
        var aggiornata = await _service.UpdateAsync(id, dto, ct);
        return aggiornata ? NoContent() : NotFound();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var eliminata = await _service.DeleteAsync(id, ct);
        return eliminata ? NoContent() : NotFound();
    }
}
