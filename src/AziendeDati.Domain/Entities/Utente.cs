namespace AziendeDati.Domain.Entities;

/// <summary>
/// Utente applicativo, appartiene a un'azienda e ha un ruolo.
/// </summary>
public class Utente
{
    public int Id { get; set; }

    // Obbligatorio, max 50, univoco: sarà lo "sub" (subject) del token JWT
    // nella Fase 8, la chiave con cui la claims transformation ricarica
    // l'utente dal DB.
    public required string Username { get; set; }

    public required string Email { get; set; }

    // Usato nella claims transformation (Fase 8): se l'utente viene disattivato
    // (Attivo=false), le richieste successive vengono negate anche se il suo
    // token JWT è ancora formalmente valido.
    public bool Attivo { get; set; }

    // FK esplicite + navigation obbligatorie (pattern spiegato in Dato.cs).
    public int AziendaId { get; set; }
    public int RuoloId { get; set; }

    public Azienda Azienda { get; set; } = null!;
    public Ruolo Ruolo { get; set; } = null!;
}
