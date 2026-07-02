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
    // I default ("", 0) valgono se la chiave manca nella configurazione:
    // nella Fase 6 vedremo come VALIDARE le option all'avvio invece di
    // scoprire a runtime che mancano.
    public string Host { get; set; } = string.Empty;

    public int Port { get; set; }

    public string From { get; set; } = string.Empty;
}
