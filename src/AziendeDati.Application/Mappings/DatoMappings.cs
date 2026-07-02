using AziendeDati.Application.Dtos;
using AziendeDati.Domain.Entities;
using AziendeDati.Domain.ReadModels;

namespace AziendeDati.Application.Mappings;

/// <summary>Mapping manuale per Dati e report (principi in AziendaMappings.cs).</summary>
public static class DatoMappings
{
    // PRE-CONDIZIONE: l'entità deve avere la Categoria caricata (Include nel
    // repository), altrimenti d.Categoria è null → NullReferenceException.
    // È il motivo per cui repository e mapping vanno letti in coppia.
    public static DatoReadDto ToReadDto(this Dato entity) => new()
    {
        Id = entity.Id,
        Value = entity.Value,
        Timestamp = entity.Timestamp,
        CategoriaId = entity.CategoriaId,
        CategoriaNome = entity.Categoria.Nome
    };

    public static SommaPerCategoriaDto ToDto(this SommaPerCategoria readModel) => new()
    {
        CategoriaId = readModel.CategoriaId,
        Categoria = readModel.Categoria,
        Somma = readModel.Somma
    };
}
