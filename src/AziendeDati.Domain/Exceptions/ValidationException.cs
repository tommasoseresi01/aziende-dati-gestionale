namespace AziendeDati.Domain.Exceptions;

/// <summary>Eccezione di dominio: la richiesta è formalmente valida ma viola una regola di business (→ HTTP 400).</summary>
// Da non confondere con la validazione "di forma" della Fase 6 (annotations /
// FluentValidation, che agiscono PRIMA di arrivare ai servizi): questa la
// lancia la LOGICA APPLICATIVA quando una regola che richiede i dati fallisce
// (es. "l'azienda indicata non esiste").
//
// ATTENZIONE all'omonimia: esistono anche System.ComponentModel.DataAnnotations
// .ValidationException e FluentValidation.ValidationException. Il namespace
// distingue; negli using dei file che ne vedono più d'una si usa un alias.
public class ValidationException : Exception
{
    public ValidationException(string message)
        : base(message)
    {
    }
}
