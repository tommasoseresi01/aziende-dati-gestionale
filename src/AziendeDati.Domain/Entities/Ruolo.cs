namespace AziendeDati.Domain.Entities;

/// <summary>
/// Ruolo applicativo. Il Nome viene emesso come claim "role" nel token JWT (Fase 8).
/// </summary>
// Valori previsti: "data.company.owner" (può scrivere) e "data.company.reader"
// (sola lettura). Sono in una TABELLA (non in un enum hardcoded) perché così i
// ruoli sono dati amministrabili: se ne possono aggiungere senza ricompilare.
public class Ruolo
{
    public int Id { get; set; }

    public required string Nome { get; set; }

    // Lato "1" della relazione Ruolo 1-N Utente.
    public ICollection<Utente> Utenti { get; set; } = [];
}
