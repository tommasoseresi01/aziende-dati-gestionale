using System.ComponentModel.DataAnnotations;

namespace AziendeDati.Application.Dtos;

// ============================================================================
// PERCHÉ I DTO (Data Transfer Object) E NON LE ENTITÀ DIRETTAMENTE?
// Esporre le entità EF dai controller "funziona", ma è una trappola. Tre motivi:
//
// 1. OVER-POSTING (sicurezza): se il POST accettasse direttamente Azienda, un
//    client malizioso potrebbe inviare {"id": 999, "dataRegistrazione": ...} e
//    il model binding valorizzerebbe TUTTO, anche i campi che non deve decidere
//    lui. Il DTO di input elenca SOLO i campi che il client può scrivere.
//
// 2. CICLI DI SERIALIZZAZIONE: le entità hanno navigation bidirezionali
//    (Azienda.Utenti → Utente.Azienda → ...): il serializzatore JSON andrebbe
//    in loop. I DTO sono "piatti": niente cicli per costruzione.
//
// 3. ACCOPPIAMENTO: l'entità È lo schema del DB; il DTO è il CONTRATTO PUBBLICO
//    dell'API: i due evolvono indipendentemente.
//
// VALIDAZIONE CON DATA ANNOTATIONS (Fase 6): gli attributi qui sotto dichiarano
// le regole sul contratto. Grazie ad [ApiController] (vedi PingController) la
// validazione parte AUTOMATICAMENTE dopo il model binding: se una regola è
// violata, il framework risponde 400 Bad Request con un ValidationProblemDetails
// che elenca campo per campo gli errori — SENZA che nessuna action debba
// scrivere "if (!ModelState.IsValid) return BadRequest(...)". L'action non
// viene proprio eseguita.
//
// NOTA sul "required" C# (usato nei DTO di LETTURA più sotto): sui DTO di
// INPUT usiamo [Required] + default, NON la keyword "required". Con la keyword,
// un JSON senza il campo fallirebbe già nella DESERIALIZZAZIONE (errore del
// serializzatore, messaggio generico); con [Required] il binding riesce e la
// VALIDAZIONE produce l'errore puntuale e leggibile che vogliamo dare al client.
// ============================================================================

/// <summary>Rappresentazione di un'azienda restituita dall'API (output).</summary>
// DTO di output: niente validazione (i dati escono, non entrano);
// "required" C# qui va benissimo perché siamo NOI a costruirlo nel mapping.
public sealed record AziendaReadDto
{
    public int Id { get; init; }
    public required string RagioneSociale { get; init; }
    public required string PartitaIva { get; init; }
    public DateTime DataRegistrazione { get; init; }
}

/// <summary>Dati che il client invia per creare un'azienda (input).</summary>
// Niente Id (lo genera il DB) e niente DataRegistrazione (la decide il server):
// il client non DEVE poterli impostare — è l'antidoto all'over-posting.
public sealed record AziendaCreateDto
{
    // [Required] boccia null E stringa vuota (AllowEmptyStrings default: false).
    // ErrorMessage: il testo che il client vede nel dettaglio del 400.
    [Required(ErrorMessage = "La ragione sociale è obbligatoria.")]
    [MaxLength(100, ErrorMessage = "La ragione sociale non può superare i 100 caratteri.")]
    public string RagioneSociale { get; init; } = string.Empty;

    // [StringLength] con MinimumLength = lunghezza ESATTA di 11;
    // [RegularExpression] aggiunge il vincolo "solo cifre". Le annotations si
    // COMBINANO: vengono valutate tutte e il 400 elenca ogni violazione.
    [Required(ErrorMessage = "La partita IVA è obbligatoria.")]
    [StringLength(11, MinimumLength = 11, ErrorMessage = "La partita IVA deve essere di 11 caratteri.")]
    [RegularExpression("^[0-9]{11}$", ErrorMessage = "La partita IVA deve contenere solo cifre.")]
    public string PartitaIva { get; init; } = string.Empty;
}

/// <summary>Dati che il client invia per aggiornare un'azienda (input).</summary>
public sealed record AziendaUpdateDto
{
    [Required(ErrorMessage = "La ragione sociale è obbligatoria.")]
    [MaxLength(100, ErrorMessage = "La ragione sociale non può superare i 100 caratteri.")]
    public string RagioneSociale { get; init; } = string.Empty;

    [Required(ErrorMessage = "La partita IVA è obbligatoria.")]
    [StringLength(11, MinimumLength = 11, ErrorMessage = "La partita IVA deve essere di 11 caratteri.")]
    [RegularExpression("^[0-9]{11}$", ErrorMessage = "La partita IVA deve contenere solo cifre.")]
    public string PartitaIva { get; init; } = string.Empty;
}
