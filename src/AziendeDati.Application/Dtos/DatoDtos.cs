namespace AziendeDati.Application.Dtos;

// DTO per i Dati misurati e per il report (principi in AziendaDtos.cs).

/// <summary>Un dato misurato, arricchito col nome della categoria (output).</summary>
// CategoriaNome è "denormalizzato" nel DTO: il client riceve subito il nome
// leggibile senza dover fare una seconda chiamata a /api/categorie — il DTO si
// modella sui BISOGNI del client, non sulla struttura delle tabelle.
public sealed record DatoReadDto
{
    public int Id { get; init; }
    public decimal Value { get; init; }
    public DateTime Timestamp { get; init; }
    public int CategoriaId { get; init; }
    public required string CategoriaNome { get; init; }
}

/// <summary>Riga del report "somma dei valori per categoria" (output).</summary>
public sealed record SommaPerCategoriaDto
{
    public int CategoriaId { get; init; }
    public required string Categoria { get; init; }
    public decimal Somma { get; init; }
}
