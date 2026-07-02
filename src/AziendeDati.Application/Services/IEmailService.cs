namespace AziendeDati.Application.Services;

/// <summary>Servizio di invio email (per ora fittizio: serve a esercitare l'Options pattern).</summary>
public interface IEmailService
{
    /// <summary>La porta SMTP letta dalla configurazione tipizzata.</summary>
    int GetEmailPort();

    /// <summary>Descrive la configurazione corrente (host:porta, mittente) — solo dimostrativo.</summary>
    string DescriviConfigurazione();
}
