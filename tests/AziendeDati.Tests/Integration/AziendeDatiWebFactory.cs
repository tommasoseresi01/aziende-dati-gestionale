using AziendeDati.Infrastructure;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace AziendeDati.Tests.Integration;

/// <summary>
/// Avvia l'INTERA Web API in memoria per i test di integrazione, ma sostituendo
/// il database SQL Server con uno InMemory (Fase 11).
/// </summary>
// COS'È WebApplicationFactory: fa partire l'app usando lo STESSO Program.cs (stessa
// pipeline: middleware, autenticazione JWT, autorizzazione, exception handler,
// controller) dentro un server di test in-process. Le richieste HTTP non escono
// sulla rete: passano da un HttpClient direttamente all'app. Così testiamo il
// comportamento REALE end-to-end (401/403/200/400/500), non i singoli pezzi.
//
// PERCHÉ sostituire il DbContext: i test non devono dipendere da SQL Server LocalDB
// (né sporcarlo). Rimpiazziamo la registrazione con InMemory e lo POPOLIAMO col
// seed dell'app (EnsureCreated applica gli HasData: aziende, categorie, ruoli e
// UTENTI — mario.rossi=owner, laura.bianchi=reader — che servono a /connect/token).
public sealed class AziendeDatiWebFactory : WebApplicationFactory<Program>
{
    // Nome fisso per istanza di factory: tutte le richieste dei test condividono
    // lo stesso store InMemory (i test qui sono di sola lettura/negativi, non
    // scrivono, quindi la condivisione è sicura).
    private readonly string _dbName = $"it-{Guid.NewGuid()}";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // Ambiente "Testing": carica appsettings.json (Jwt, OAuth, EmailService),
        // ma NON attiva le cose da solo-sviluppo (es. Swagger).
        builder.UseEnvironment("Testing");

        // ConfigureTestServices gira DOPO la registrazione dei servizi dell'app,
        // quindi qui abbiamo l'ultima parola: togliamo il DbContext SQL Server...
        builder.ConfigureTestServices(services =>
        {
            RimuoviRegistrazioniDbContext(services);
            // ...e mettiamo InMemory al suo posto.
            services.AddDbContext<AziendeDbContext>(options =>
                options.UseInMemoryDatabase(_dbName));
        });
    }

    /// <summary>Crea e popola (seed) il database InMemory. Idempotente: chiamabile a ogni test.</summary>
    public void InizializzaDatabase()
    {
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AziendeDbContext>();
        // EnsureCreated su InMemory applica i dati di seed (HasData). Se il DB
        // esiste già, non fa nulla e non duplica il seed.
        db.Database.EnsureCreated();
    }

    // Rimuove OGNI registrazione legata al DbContext (le opzioni, la loro
    // configurazione e il contesto stesso), così InMemory non entra in conflitto
    // col provider SQL Server registrato dall'app.
    private static void RimuoviRegistrazioniDbContext(IServiceCollection services)
    {
        var daRimuovere = services.Where(d =>
            d.ServiceType == typeof(AziendeDbContext) ||
            (d.ServiceType.FullName?.Contains("DbContextOptions") ?? false))
            .ToList();

        foreach (var descriptor in daRimuovere)
        {
            services.Remove(descriptor);
        }
    }
}
