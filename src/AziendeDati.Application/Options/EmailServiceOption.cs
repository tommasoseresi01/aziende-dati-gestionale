using System.ComponentModel.DataAnnotations;

namespace AziendeDati.Application.Options;

/// <summary>
/// Impostazioni del servizio email, legate alla sezione "EmailService" di appsettings.json.
/// </summary>
// OPTIONS PATTERN — configurazione FORTEMENTE TIPIZZATA.
//
// Il problema: leggere la configurazione con stringhe sparse nel codice
// (config["EmailService:Port"]) è fragile — un refuso nella chiave non dà
// errori di compilazione, restituisce null a runtime; e il valore è una
// stringa da convertire a mano.
//
// La soluzione: una classe POCO che RISPECCHIA la sezione del JSON. Il binding
// (in Program.cs) copia i valori della sezione nelle proprietà omonime,
// convertendo i tipi (Port: "587" → int 587). Chi consuma riceve un oggetto
// tipizzato via DI: refusi impossibili, IntelliSense, un solo punto di verità.
// Fonte: https://learn.microsoft.com/aspnet/core/fundamentals/configuration/options
public sealed class EmailServiceOption
{
    // Il nome della sezione in appsettings.json, come costante: evita la
    // "magic string" duplicata tra questo file e Program.cs.
    public const string SectionName = "EmailService";

    // Proprietà con GET e SET pubblici: il binder le valorizza via reflection.
    // Le data annotations (Fase 6) rendono la configurazione AUTO-VALIDANTE:
    // grazie a ValidateDataAnnotations + ValidateOnStart (Program.cs), se una
    // chiave manca o è fuori range l'app si rifiuta di partire (fail fast).
    [Required(ErrorMessage = "EmailService:Host è obbligatorio.")]
    public string Host { get; set; } = string.Empty;

    [Range(1, 65535, ErrorMessage = "EmailService:Port deve essere una porta valida (1-65535).")]
    public int Port { get; set; }

    [Required(ErrorMessage = "EmailService:From è obbligatorio.")]
    [EmailAddress(ErrorMessage = "EmailService:From deve essere un indirizzo email valido.")]
    public string From { get; set; } = string.Empty;
}
