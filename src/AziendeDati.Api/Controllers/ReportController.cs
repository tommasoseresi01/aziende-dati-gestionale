using AziendeDati.Application.Dtos;
using AziendeDati.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace AziendeDati.Api.Controllers;

/// <summary>Endpoint di report/aggregazioni. Route: /api/report.</summary>
[ApiController]
[Route("api/[controller]")]
public class ReportController : ControllerBase
{
    private readonly IDatiService _service;

    public ReportController(IDatiService service)
    {
        _service = service;
    }

    /// <summary>Somma dei valori misurati per categoria, dalla più alta alla più bassa.</summary>
    // Route esplicita nel template: GET /api/report/somma-per-categoria.
    // I trattini (kebab-case) sono la convenzione più comune per le URL REST.
    [HttpGet("somma-per-categoria")]
    public async Task<ActionResult<List<SommaPerCategoriaDto>>> GetSommaPerCategoria(CancellationToken ct)
    {
        var report = await _service.GetSommaPerCategoriaAsync(ct);
        return Ok(report);
    }
}
