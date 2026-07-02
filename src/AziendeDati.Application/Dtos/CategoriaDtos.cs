namespace AziendeDati.Application.Dtos;

// DTO per Categoria — stessi principi spiegati in AziendaDtos.cs
// (over-posting, cicli di serializzazione, disaccoppiamento contratto/schema).

/// <summary>Rappresentazione di una categoria restituita dall'API (output).</summary>
public sealed record CategoriaReadDto
{
    public int Id { get; init; }
    public required string Nome { get; init; }
    public string? Descrizione { get; init; }
}

/// <summary>Dati per creare una categoria (input).</summary>
public sealed record CategoriaCreateDto
{
    public required string Nome { get; init; }
    public string? Descrizione { get; init; }
}

/// <summary>Dati per aggiornare una categoria (input).</summary>
public sealed record CategoriaUpdateDto
{
    public required string Nome { get; init; }
    public string? Descrizione { get; init; }
}
