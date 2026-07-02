using AziendeDati.Application.Dtos;
using AziendeDati.Application.Mappings;
using AziendeDati.Domain.Repositories;

namespace AziendeDati.Application.Services;

/// <summary>Implementazione dei casi d'uso sulle Categorie (struttura identica ad AziendeService).</summary>
public class CategorieService : ICategorieService
{
    private readonly ICategorieRepository _repository;

    public CategorieService(ICategorieRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<CategoriaReadDto>> GetAllAsync(CancellationToken ct = default)
    {
        var categorie = await _repository.GetAllAsync(ct);
        return categorie.Select(c => c.ToReadDto()).ToList();
    }

    public async Task<CategoriaReadDto?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var categoria = await _repository.GetByIdAsync(id, ct);
        return categoria?.ToReadDto();
    }

    public async Task<CategoriaReadDto> CreateAsync(CategoriaCreateDto dto, CancellationToken ct = default)
    {
        var categoria = dto.ToEntity();
        await _repository.AddAsync(categoria, ct);
        return categoria.ToReadDto();
    }

    public async Task<bool> UpdateAsync(int id, CategoriaUpdateDto dto, CancellationToken ct = default)
    {
        var categoria = await _repository.GetByIdAsync(id, ct);
        if (categoria is null)
        {
            return false;
        }

        dto.ApplyTo(categoria);
        await _repository.UpdateAsync(categoria, ct);
        return true;
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken ct = default)
    {
        var categoria = await _repository.GetByIdAsync(id, ct);
        if (categoria is null)
        {
            return false;
        }

        // NOTA: se la categoria è usata da Dati/RigheOrdine, il DB blocca la
        // DELETE (OnDelete Restrict, Fase 2) → oggi eccezione/500; nella Fase 7
        // la trasformeremo in una risposta pulita.
        await _repository.DeleteAsync(categoria, ct);
        return true;
    }
}
