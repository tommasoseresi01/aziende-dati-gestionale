using AziendeDati.Application.Dtos;

namespace AziendeDati.Application.Services;

/// <summary>Casi d'uso applicativi sulle Categorie (pattern a eccezioni, vedi IAziendeService).</summary>
public interface ICategorieService
{
    Task<List<CategoriaReadDto>> GetAllAsync(CancellationToken ct = default);

    /// <summary>Lancia NotFoundException se la categoria non esiste.</summary>
    Task<CategoriaReadDto> GetByIdAsync(int id, CancellationToken ct = default);

    Task<CategoriaReadDto> CreateAsync(CategoriaCreateDto dto, CancellationToken ct = default);

    /// <summary>Lancia NotFoundException se la categoria non esiste.</summary>
    Task UpdateAsync(int id, CategoriaUpdateDto dto, CancellationToken ct = default);

    /// <summary>Lancia NotFoundException se non esiste; se è referenziata, il DB blocca (→ 409).</summary>
    Task DeleteAsync(int id, CancellationToken ct = default);
}
