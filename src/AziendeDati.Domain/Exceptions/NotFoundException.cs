namespace AziendeDati.Domain.Exceptions;

/// <summary>Eccezione di dominio: la risorsa richiesta non esiste (→ HTTP 404).</summary>
// ECCEZIONI DI DOMINIO — perché crearle:
// le eccezioni .NET standard (InvalidOperationException...) dicono COSA è
// andato storto tecnicamente, non COSA SIGNIFICA per il business. Un'eccezione
// con un nome di dominio ("non trovato") permette al gestore globale (Api) di
// mapparla su una risposta HTTP precisa SENZA che il dominio sappia nulla di
// HTTP: il dominio dice "non esiste", l'Api traduce in 404.
//
// Vivono in Domain (nessuna dipendenza: derivano da System.Exception) così
// OGNI strato può lanciarle e l'Api può catturarle.
public class NotFoundException : Exception
{
    public NotFoundException(string message)
        : base(message)
    {
    }

    // Costruttore di comodo per il caso tipico "entità X con id Y non trovata":
    // messaggi uniformi in tutta l'applicazione senza ripetere la formattazione.
    public NotFoundException(string risorsa, object chiave)
        : base($"{risorsa} con id '{chiave}' non trovata.")
    {
    }
}
