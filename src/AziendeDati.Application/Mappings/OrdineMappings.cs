using AziendeDati.Application.Dtos;
using AziendeDati.Domain.Entities;

namespace AziendeDati.Application.Mappings;

/// <summary>Mapping manuale Ordine ↔ DTO (principi in AziendaMappings.cs).</summary>
public static class OrdineMappings
{
    // Mapping di un GRAFO: l'ordine e le sue righe insieme.
    // Pre-condizione: Righe e Categoria caricate (Include/ThenInclude nel repository).
    public static OrdineReadDto ToReadDto(this Ordine entity) => new()
    {
        Id = entity.Id,
        Numero = entity.Numero,
        Data = entity.Data,
        AziendaId = entity.AziendaId,
        Righe = entity.Righe.Select(r => r.ToReadDto()).ToList(),
        Totale = entity.Righe.Sum(r => r.Quantita * r.PrezzoUnitario)
    };

    public static RigaOrdineReadDto ToReadDto(this RigaOrdine entity) => new()
    {
        Id = entity.Id,
        CategoriaId = entity.CategoriaId,
        CategoriaNome = entity.Categoria.Nome,
        Descrizione = entity.Descrizione,
        Quantita = entity.Quantita,
        PrezzoUnitario = entity.PrezzoUnitario,
        Totale = entity.Quantita * entity.PrezzoUnitario
    };

    public static Ordine ToEntity(this OrdineCreateDto dto) => new()
    {
        Numero = dto.Numero,
        Data = dto.Data,
        AziendaId = dto.AziendaId,
        // Le righe si mappano insieme al padre: EF salverà l'intero grafo.
        // OrdineId non va impostato: lo collega EF quando salva il padre.
        Righe = dto.Righe.Select(r => new RigaOrdine
        {
            CategoriaId = r.CategoriaId,
            Descrizione = r.Descrizione,
            Quantita = r.Quantita,
            PrezzoUnitario = r.PrezzoUnitario
        }).ToList()
    };
}
