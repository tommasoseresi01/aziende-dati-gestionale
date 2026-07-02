using AziendeDati.Application.Dtos;
using FluentValidation;

namespace AziendeDati.Application.Validators;

/// <summary>Regole di validazione per la creazione di un Ordine (FluentValidation).</summary>
//
// DATA ANNOTATIONS vs FLUENTVALIDATION — quando conviene cosa:
//
//  DATA ANNOTATIONS ([Required], [MaxLength]...):
//   + zero dipendenze, regole VISIBILI sul DTO, integrazione automatica con
//     [ApiController] → perfette per regole SEMPLICI su singoli campi
//     (obbligatorietà, lunghezze, range, formati).
//   – scomode per regole CROSS-FIELD ("campo A dipende da campo B"), su
//     COLLEZIONI ("almeno una riga"), condizionali o che richiedono servizi;
//     e "sporcano" il DTO quando le regole crescono.
//
//  FLUENTVALIDATION:
//   + regole scritte in C# fluente e TESTABILI come una classe qualsiasi;
//     cross-field naturali (Must con accesso all'intero oggetto), RuleForEach
//     per le collezioni, When/Unless per le condizioni, validator annidati.
//   – pacchetto esterno, regole separate dal DTO (bisogna sapere che esistono).
//
//  Prassi comune (che seguiamo): annotations per i vincoli banali dei DTO
//  semplici (Azienda, Categoria), FluentValidation per i DTO complessi (Ordine).
//
// INTEGRAZIONE NEL PIPELINE: il pacchetto storico di auto-validazione MVC
// (FluentValidation.AspNetCore) è DEPRECATO dagli stessi autori (non supporta
// i validator async e nasconde il momento della validazione). L'approccio
// raccomandato oggi è ESPLICITO: si inietta IValidator<T> dove serve (nel
// controller), si chiama ValidateAsync e si riversano gli errori in ModelState
// → stessa risposta 400 ValidationProblemDetails delle annotations.
// I validator si registrano nel container con AddValidatorsFromAssemblyContaining
// (vedi Program.cs).
public sealed class OrdineCreateDtoValidator : AbstractValidator<OrdineCreateDto>
{
    public OrdineCreateDtoValidator()
    {
        // Le regole si dichiarano nel COSTRUTTORE con l'API fluente.
        RuleFor(x => x.Numero)
            .NotEmpty().WithMessage("Il numero d'ordine è obbligatorio.")
            .MaximumLength(20).WithMessage("Il numero d'ordine non può superare i 20 caratteri.");

        RuleFor(x => x.AziendaId)
            .GreaterThan(0).WithMessage("AziendaId deve essere un id valido (> 0).");

        // REGOLA SULLA COLLEZIONE (il primo requisito cross-field della specifica):
        // un ordine senza righe non ha senso. Con le annotations servirebbe un
        // attributo custom; qui è una riga.
        RuleFor(x => x.Righe)
            .NotEmpty().WithMessage("L'ordine deve contenere almeno una riga.");

        // RuleForEach applica un validator A OGNI ELEMENTO della collezione:
        // gli errori escono indicizzati (es. "Righe[1].Quantita") così il client
        // sa QUALE riga è sbagliata.
        RuleForEach(x => x.Righe).SetValidator(new RigaOrdineCreateDtoValidator());

        // REGOLA CROSS-FIELD vera e propria: coinvolge Data e... il calendario.
        // Must riceve l'intero valore e può incrociare più campi a piacere.
        RuleFor(x => x.Data)
            .LessThanOrEqualTo(_ => DateTime.UtcNow.AddDays(1))
            .WithMessage("La data dell'ordine non può essere nel futuro.");
    }
}

/// <summary>Regole per la singola riga d'ordine (usato da RuleForEach).</summary>
public sealed class RigaOrdineCreateDtoValidator : AbstractValidator<RigaOrdineCreateDto>
{
    public RigaOrdineCreateDtoValidator()
    {
        RuleFor(x => x.CategoriaId)
            .GreaterThan(0).WithMessage("CategoriaId deve essere un id valido (> 0).");

        RuleFor(x => x.Descrizione)
            .NotEmpty().WithMessage("La descrizione della riga è obbligatoria.")
            .MaximumLength(200).WithMessage("La descrizione non può superare i 200 caratteri.");

        // I requisiti numerici della specifica: quantità > 0, prezzo >= 0.
        RuleFor(x => x.Quantita)
            .GreaterThan(0).WithMessage("La quantità deve essere maggiore di zero.");

        RuleFor(x => x.PrezzoUnitario)
            .GreaterThanOrEqualTo(0).WithMessage("Il prezzo unitario non può essere negativo.");
    }
}
