namespace AziendeDati.Domain.Entities;

/// <summary>
/// Ordine emesso da un'azienda; composto da righe (relazione padre-figlio 1-N).
/// </summary>
public class Ordine
{
    public int Id { get; set; }

    // Numero "umano" dell'ordine (es. ORD-2025-001), univoco: è diverso dall'Id
    // tecnico. Best practice: mai usare la PK come identificatore di business.
    public required string Numero { get; set; }

    public DateTime Data { get; set; }

    public int AziendaId { get; set; }
    public Azienda Azienda { get; set; } = null!;

    // Le righe "vivono" dentro l'ordine: se l'ordine viene eliminato, le righe
    // non hanno più senso → delete a cascata (configurato nella Fluent API).
    public ICollection<RigaOrdine> Righe { get; set; } = [];
}
