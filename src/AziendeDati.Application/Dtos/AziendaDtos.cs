namespace AziendeDati.Application.Dtos;

// ============================================================================
// PERCHÉ I DTO (Data Transfer Object) E NON LE ENTITÀ DIRETTAMENTE?
// Esporre le entità EF dai controller "funziona", ma è una trappola. Tre motivi:
//
// 1. OVER-POSTING (sicurezza): se il POST accettasse direttamente Azienda, un
//    client malizioso potrebbe inviare {"id": 999, "dataRegistrazione": ...} e
//    il model binding valorizzerebbe TUTTO, anche i campi che non deve decidere
//    lui. Il DTO di input elenca SOLO i campi che il client può scrivere:
//    tutto il resto semplicemente non esiste nel contratto.
//
// 2. CICLI DI SERIALIZZAZIONE: le entità hanno navigation bidirezionali
//    (Azienda.Utenti → Utente.Azienda → Azienda.Utenti → ...). Il serializzatore
//    JSON entrerebbe in loop infinito (JsonException). I DTO sono "piatti":
//    niente cicli per costruzione.
//
// 3. ACCOPPIAMENTO: l'entità È lo schema del DB; se la esponi, ogni migration
//    diventa un breaking change per i client. Il DTO è il CONTRATTO PUBBLICO
//    dell'API: i due possono evolvere indipendentemente.
//
// DTO SEPARATI per lettura/creazione/aggiornamento: ognuno espone solo ciò che
// serve a quel caso d'uso (es. l'Id c'è nel Read ma non nel Create: lo genera
// il DB; il Create e l'Update oggi coincidono, ma i contratti divergono col
// tempo — meglio tipi distinti da subito).
//
// PERCHÉ "record" e non "class": i DTO sono dati puri, senza comportamento.
// I record danno immutabilità ("init": si valorizza solo alla creazione),
// uguaglianza per valore e ToString leggibile — perfetti per contratti.
// ============================================================================

/// <summary>Rappresentazione di un'azienda restituita dall'API (output).</summary>
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
    public required string RagioneSociale { get; init; }
    public required string PartitaIva { get; init; }
}

/// <summary>Dati che il client invia per aggiornare un'azienda (input).</summary>
public sealed record AziendaUpdateDto
{
    public required string RagioneSociale { get; init; }
    public required string PartitaIva { get; init; }
}
