namespace AziendeDati.Domain.Entities;

/// <summary>
/// Un'azienda registrata nel gestionale: possiede utenti, dati misurati e ordini.
/// </summary>
// Questa è una POCO (Plain Old CLR Object): una classe C# "pura", senza attributi
// EF né classi base speciali. PERCHÉ: il dominio non deve sapere COME viene
// persistito (dependency rule, vedi ARCHITETTURA.md). Tutta la configurazione
// del mapping (nomi colonna, vincoli, relazioni) sta in Infrastructure, nelle
// classi IEntityTypeConfiguration — così qui restano solo dati e logica di business.
public class Azienda
{
    // Per convenzione EF Core una proprietà "Id" (o "AziendaId") diventa la
    // chiave primaria IDENTITY. Lo renderemo comunque ESPLICITO nella
    // configurazione Fluent (niente magia nascosta).
    public int Id { get; set; }

    // "required" (C# 11): il compilatore OBBLIGA a valorizzare la proprietà
    // quando si crea l'oggetto (new Azienda { RagioneSociale = ... }).
    // È il modo moderno di dire "questa stringa non è mai null" con i nullable
    // reference types attivi, senza ricorrere al trucco "= null!".
    // Nel DB diventerà la colonna RAG_SOC NOT NULL (vedi AziendaConfiguration).
    public required string RagioneSociale { get; set; }

    // Partita IVA italiana: 11 caratteri, univoca. È una stringa (non un numero!)
    // perché può iniziare per zero e non ci si fanno calcoli sopra.
    public required string PartitaIva { get; set; }

    public DateTime DataRegistrazione { get; set; }

    // NAVIGATION PROPERTY (lato "1" della relazione 1-N): da un'Azienda si
    // naviga ai suoi Utenti/Dati/Ordini. EF Core le popola solo se glielo
    // chiediamo (es. con Include, Fase 4): di default restano vuote.
    // Inizializzarle a lista vuota ("= []", collection expression C# 12) evita
    // NullReferenceException quando l'azienda è appena creata o non caricata.
    public ICollection<Utente> Utenti { get; set; } = [];
    public ICollection<Dato> Dati { get; set; } = [];
    public ICollection<Ordine> Ordini { get; set; } = [];
}
