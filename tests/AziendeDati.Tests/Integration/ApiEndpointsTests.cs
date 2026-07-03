using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace AziendeDati.Tests.Integration;

/// <summary>
/// Test di integrazione end-to-end sulla Web API (Fase 11): verificano i criteri
/// di sicurezza e di gestione errori attraverso richieste HTTP reali.
/// </summary>
// IClassFixture<AziendeDatiWebFactory>: xUnit crea UNA sola factory (una sola app
// in memoria) condivisa da tutti i test della classe → avvio veloce. Il seed viene
// (ri)applicato nel costruttore, ma EnsureCreated è idempotente.
public class ApiEndpointsTests : IClassFixture<AziendeDatiWebFactory>
{
    private readonly AziendeDatiWebFactory _factory;

    public ApiEndpointsTests(AziendeDatiWebFactory factory)
    {
        _factory = factory;
        _factory.InizializzaDatabase();
    }

    // ---- SICUREZZA: 401 / 403 / 200 (verifica la Fase 8) --------------------

    [Fact]
    public async Task GET_aziende_SENZA_token_risponde_401()
    {
        var client = _factory.CreateClient();

        var risposta = await client.GetAsync("/api/aziende");

        // Nessuna credenziale → non autenticato.
        Assert.Equal(HttpStatusCode.Unauthorized, risposta.StatusCode);
    }

    [Fact]
    public async Task POST_aziende_con_token_READER_risponde_403()
    {
        var client = _factory.CreateClient();
        var token = await OttieniTokenAsync(client, "acme-reader-client", "secret-reader-acme-2025");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Il reader è autenticato ma la scrittura richiede il ruolo owner → 403.
        // (L'autorizzazione scatta PRIMA del binding del corpo: il payload è irrilevante.)
        var risposta = await client.PostAsJsonAsync("/api/aziende",
            new { ragioneSociale = "Prova S.p.A.", partitaIva = "11122233344" });

        Assert.Equal(HttpStatusCode.Forbidden, risposta.StatusCode);
    }

    [Fact]
    public async Task GET_aziende_con_token_READER_risponde_200()
    {
        var client = _factory.CreateClient();
        var token = await OttieniTokenAsync(client, "acme-reader-client", "secret-reader-acme-2025");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Il reader PUÒ leggere.
        var risposta = await client.GetAsync("/api/aziende");

        Assert.Equal(HttpStatusCode.OK, risposta.StatusCode);
    }

    // ---- VALIDAZIONE: 400 (verifica la Fase 6) ------------------------------

    [Fact]
    public async Task POST_aziende_con_payload_invalido_risponde_400()
    {
        var client = _factory.CreateClient();
        // Token OWNER: passa l'autorizzazione, così arriviamo alla VALIDAZIONE.
        var token = await OttieniTokenAsync(client, "acme-owner-client", "secret-owner-acme-2025");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // RagioneSociale vuota + PartitaIva non numerica → [ApiController] risponde
        // 400 da solo (validazione automatica delle data annotations, Fase 6).
        var risposta = await client.PostAsJsonAsync("/api/aziende",
            new { ragioneSociale = "", partitaIva = "non-valida" });

        Assert.Equal(HttpStatusCode.BadRequest, risposta.StatusCode);
    }

    // ---- ERRORE NON GESTITO: 500 → ProblemDetails (verifica la Fase 7) ------

    [Fact]
    public async Task Un_endpoint_che_lancia_produce_500_in_formato_ProblemDetails()
    {
        var client = _factory.CreateClient();

        // /api/ping/errore lancia un'eccezione di proposito.
        var risposta = await client.GetAsync("/api/ping/errore");

        Assert.Equal(HttpStatusCode.InternalServerError, risposta.StatusCode);

        // Il corpo dev'essere un ProblemDetails JSON con status e traceId,
        // NON uno stack trace grezzo (Fase 7).
        var corpo = await risposta.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(corpo);
        Assert.Equal(500, doc.RootElement.GetProperty("status").GetInt32());
        Assert.True(doc.RootElement.TryGetProperty("traceId", out _));
    }

    // ---- Helper: ottiene un JWT reale via il Client Credentials Flow --------
    private static async Task<string> OttieniTokenAsync(HttpClient client, string clientId, string clientSecret)
    {
        // OAuth2: POST application/x-www-form-urlencoded a /connect/token (Fase 8).
        var form = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["grant_type"] = "client_credentials",
            ["client_id"] = clientId,
            ["client_secret"] = clientSecret
        });

        var risposta = await client.PostAsync("/connect/token", form);
        risposta.EnsureSuccessStatusCode();

        var corpo = await risposta.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(corpo);
        return doc.RootElement.GetProperty("access_token").GetString()!;
    }
}
