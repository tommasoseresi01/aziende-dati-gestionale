namespace AziendeDati.Domain.Entities;

/// <summary>
/// Categoria di classificazione dei dati misurati (es. Temperatura, Pressione).
/// </summary>
public class Categoria
{
    public int Id { get; set; }

    // Obbligatorio, max 50, univoco (vincoli nella configurazione Fluent).
    public required string Nome { get; set; }

    // "string?" = può essere null: con i nullable reference types attivi il
    // punto di domanda È la dichiarazione di opzionalità. Nel DB la colonna
    // sarà NULL-abile (EF lo deduce proprio dal tipo C#).
    public string? Descrizione { get; set; }

    // Lato "1" della relazione Categoria 1-N Dato.
    public ICollection<Dato> Dati { get; set; } = [];
}
