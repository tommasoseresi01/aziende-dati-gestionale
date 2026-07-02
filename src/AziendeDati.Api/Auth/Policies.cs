namespace AziendeDati.Api.Auth;

/// <summary>Nomi delle policy di autorizzazione, come costanti (niente magic string negli attributi).</summary>
public static class Policies
{
    public const string CompanyOwner = "COMPANY_OWNER";
    public const string CompanyReader = "COMPANY_READER";
}
