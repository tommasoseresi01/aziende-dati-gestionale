namespace AziendeDati.Domain.Entities;

/// <summary>
/// Una misurazione/valore registrato da un'azienda, classificato per categoria.
/// </summary>
public class Dato
{
    public int Id { get; set; }

    // decimal per i valori numerici "di business" (misure, importi): a differenza
    // di double/float è in base 10 e NON introduce errori di arrotondamento
    // binario (0.1 + 0.2 == 0.3 con decimal, non con double!).
    // Precisione decimal(18,2) fissata nella configurazione Fluent.
    public decimal Value { get; set; }

    public DateTime Timestamp { get; set; }

    // FOREIGN KEY ESPLICITE: per convenzione EF Core riconosce "<Navigation>Id"
    // come FK della navigation omonima. Averle come proprietà (invece che solo
    // implicite/"shadow") permette di leggerle o impostarle senza caricare
    // l'intera entità correlata (es. dato.AziendaId = 3, niente query extra).
    public int AziendaId { get; set; }
    public int CategoriaId { get; set; }

    // NAVIGATION lato "N" (reference navigation): dal Dato si risale all'Azienda
    // e alla Categoria. "= null!" dice al compilatore: "so che sembra non
    // inizializzata, fidati" — è il pattern consigliato dalla doc Microsoft per
    // le navigation OBBLIGATORIE: null solo se non caricata via Include, e in
    // quel caso non va toccata. (Il "required" usato per le stringhe qui sarebbe
    // scomodo: costringerebbe a valorizzare la navigation a ogni new, mentre di
    // solito si imposta solo la FK.)
    // Fonte: https://learn.microsoft.com/ef/core/miscellaneous/nullable-reference-types
    public Azienda Azienda { get; set; } = null!;
    public Categoria Categoria { get; set; } = null!;
}
