using AziendeDati.Application.Dtos;
using AziendeDati.Domain.Entities;

namespace AziendeDati.Application.Mappings;

/// <summary>Mapping manuale Categoria ↔ DTO (vedi AziendaMappings per i principi).</summary>
public static class CategoriaMappings
{
    public static CategoriaReadDto ToReadDto(this Categoria entity) => new()
    {
        Id = entity.Id,
        Nome = entity.Nome,
        Descrizione = entity.Descrizione
    };

    public static Categoria ToEntity(this CategoriaCreateDto dto) => new()
    {
        Nome = dto.Nome,
        Descrizione = dto.Descrizione
    };

    public static void ApplyTo(this CategoriaUpdateDto dto, Categoria entity)
    {
        entity.Nome = dto.Nome;
        entity.Descrizione = dto.Descrizione;
    }
}
