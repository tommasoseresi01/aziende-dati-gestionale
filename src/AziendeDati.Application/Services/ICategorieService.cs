using AziendeDati.Application.Dtos;

namespace AziendeDati.Application.Services;

/// <summary>Casi d'uso applicativi sulle Categorie (vedi IAziendeService per i principi).</summary>
public interface ICategorieService
{
    Task<List<CategoriaReadDto>> GetAllAsync(CancellationToken ct = default);
    Task<CategoriaReadDto?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<CategoriaReadDto> CreateAsync(CategoriaCreateDto dto, CancellationToken ct = default);
    Task<bool> UpdateAsync(int id, CategoriaUpdateDto dto, CancellationToken ct = default);
    Task<bool> DeleteAsync(int id, CancellationToken ct = default);
}
