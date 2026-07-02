using AziendeDati.Application.Options;
using Microsoft.Extensions.Options;

namespace AziendeDati.Application.Services;

/// <summary>Implementazione (fittizia) del servizio email: consuma la configurazione tipizzata.</summary>
public class EmailService : IEmailService
{
    // Si salva DIRETTAMENTE il .Value (l'oggetto EmailServiceOption), non
    // l'involucro IOptions: il codice del servizio resta pulito.
    private readonly EmailServiceOption _options;

    // Si inietta IOptions<EmailServiceOption>, NON EmailServiceOption nudo:
    // è il contratto che Configure<T> registra nel container (vedi Program.cs).
    //
    // LE TRE VARIANTI dell'Options pattern — quale iniettare?
    //  - IOptions<T>          (Singleton): valore letto UNA volta al primo uso e
    //    congelato per tutta la vita dell'app. Il default giusto per config
    //    che non cambia mentre l'app gira (il nostro caso).
    //  - IOptionsSnapshot<T>  (Scoped): rilegge la configurazione A OGNI
    //    RICHIESTA HTTP — se modifichi appsettings.json a app avviata
    //    (reloadOnChange), la richiesta successiva vede il valore nuovo.
    //    Non iniettabile nei Singleton (è Scoped!).
    //  - IOptionsMonitor<T>   (Singleton): .CurrentValue sempre aggiornato +
    //    evento OnChange per reagire subito alla modifica. Per servizi
    //    long-running (background worker) che non possono aspettare la
    //    prossima richiesta.
    // Fonte: https://learn.microsoft.com/aspnet/core/fundamentals/configuration/options
    public EmailService(IOptions<EmailServiceOption> options)
    {
        _options = options.Value;
    }

    // NIENTE stringhe literal tipo config["EmailService:Port"] sparse nel
    // codice: la porta arriva dall'oggetto tipizzato, con un solo punto di
    // verità (la sezione EmailService) e conversione già fatta dal binder.
    public int GetEmailPort() => _options.Port;

    public string DescriviConfigurazione() =>
        $"SMTP {_options.Host}:{_options.Port}, mittente {_options.From}";
}
