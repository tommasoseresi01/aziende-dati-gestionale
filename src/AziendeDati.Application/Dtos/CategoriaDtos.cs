using System.ComponentModel.DataAnnotations;

namespace AziendeDati.Application.Dtos;

// DTO per Categoria — principi in AziendaDtos.cs (over-posting, cicli,
// accoppiamento; data annotations + [ApiController] → 400 automatico).

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
    [Required(ErrorMessage = "Il nome della categoria è obbligatorio.")]
    [MaxLength(50, ErrorMessage = "Il nome non può superare i 50 caratteri.")]
    public string Nome { get; init; } = string.Empty;

    // Campo OPZIONALE: niente [Required], solo il limite di lunghezza
    // (che rispecchia il HasMaxLength(250) della configurazione EF — il
    // contratto valida PRIMA quello che il DB rifiuterebbe DOPO).
    [MaxLength(250, ErrorMessage = "La descrizione non può superare i 250 caratteri.")]
    public string? Descrizione { get; init; }
}

/// <summary>Dati per aggiornare una categoria (input).</summary>
public sealed record CategoriaUpdateDto
{
    [Required(ErrorMessage = "Il nome della categoria è obbligatorio.")]
    [MaxLength(50, ErrorMessage = "Il nome non può superare i 50 caratteri.")]
    public string Nome { get; init; } = string.Empty;

    [MaxLength(250, ErrorMessage = "La descrizione non può superare i 250 caratteri.")]
    public string? Descrizione { get; init; }
}
