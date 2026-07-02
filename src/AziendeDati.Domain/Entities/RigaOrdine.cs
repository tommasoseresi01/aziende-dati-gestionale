namespace AziendeDati.Domain.Entities;

/// <summary>
/// Riga di dettaglio di un ordine: cosa, quanto, a che prezzo.
/// </summary>
public class RigaOrdine
{
    public int Id { get; set; }

    public int OrdineId { get; set; }
    public int CategoriaId { get; set; }

    public required string Descrizione { get; set; }

    public int Quantita { get; set; }

    // decimal per gli importi monetari (vedi il commento in Dato.cs):
    // precisione decimal(18,2) fissata nella configurazione Fluent.
    public decimal PrezzoUnitario { get; set; }

    public Ordine Ordine { get; set; } = null!;
    public Categoria Categoria { get; set; } = null!;
}
