using AziendeDati.Application.Dtos;
using AziendeDati.Application.Mappings;
using AziendeDati.Domain.Exceptions;
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

    public async Task<CategoriaReadDto> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var categoria = await _repository.GetByIdAsync(id, ct)
            ?? throw new NotFoundException("Categoria", id);

        return categoria.ToReadDto();
    }

    public async Task<CategoriaReadDto> CreateAsync(CategoriaCreateDto dto, CancellationToken ct = default)
    {
        var categoria = dto.ToEntity();
        await _repository.AddAsync(categoria, ct);
        return categoria.ToReadDto();
    }

    public async Task UpdateAsync(int id, CategoriaUpdateDto dto, CancellationToken ct = default)
    {
        var categoria = await _repository.GetByIdAsync(id, ct)
            ?? throw new NotFoundException("Categoria", id);

        dto.ApplyTo(categoria);
        await _repository.UpdateAsync(categoria, ct);
    }

    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        var categoria = await _repository.GetByIdAsync(id, ct)
            ?? throw new NotFoundException("Categoria", id);

        // Se la categoria è referenziata da Dati/RigheOrdine, l'OnDelete
        // Restrict (Fase 2) fa fallire la DELETE con DbUpdateException:
        // il gestore globale la traduce in 409 Conflict con messaggio pulito
        // (chiuso il TODO della Fase 3: niente più 500).
        await _repository.DeleteAsync(categoria, ct);
    }
}
