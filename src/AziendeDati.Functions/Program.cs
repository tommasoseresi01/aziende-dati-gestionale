// Program.cs del progetto Azure Functions (modello "isolated worker").
//
// Nel modello isolated siamo NOI a creare e avviare l'host del processo worker
// (nel modello in-process, ormai superato, lo faceva il runtime di Azure).
// Questo ci dà accesso diretto a configurazione e Dependency Injection,
// esattamente come in una normale app ASP.NET Core.
//
// FunctionsApplication.CreateBuilder(args) è il punto di partenza raccomandato
// da Microsoft (richiede i pacchetti "core" versione 2.x+): configura i converter
// di default, il logging integrato con Functions e il supporto gRPC con cui il
// worker comunica con il runtime.
// Fonte: https://learn.microsoft.com/azure/azure-functions/dotnet-isolated-process-guide#start-up-and-configuration
using AziendeDati.Infrastructure;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = FunctionsApplication.CreateBuilder(args);

// ----------------------------------------------------------------------------
// INTEGRAZIONE CON LO STACK DELLA WEB API (Fase 10).
//
// Questo progetto è un HOST diverso (un processo serverless) ma CONDIVIDE con la
// Web API gli stessi progetti .NET: Domain (le entità, es. Dato) e Infrastructure
// (AziendeDbContext, EF Core, le migrations). Non riscriviamo nulla: registriamo
// lo STESSO DbContext e scriviamo sullo STESSO database SQL Server. È il senso di
// "restare nello stesso stack .NET": la Function e l'API parlano lo stesso
// linguaggio di dominio e la stessa persistenza, cambia solo il MODO di essere
// attivate (un trigger di coda invece di una richiesta HTTP a un controller).
//
// La stringa di connessione arriva dalla configurazione (local.settings.json in
// locale, App Settings in Azure): stessa chiave "AziendeDati" della Web API.
// Il "?? throw" fa fallire SUBITO l'avvio se manca, invece di esplodere alla
// prima query dentro la coda (fail fast).
//
// NOTA sui lifetime: AddDbContext registra il contesto come SCOPED. Il worker di
// Functions apre uno SCOPE di DI per OGNI invocazione della function, quindi ogni
// messaggio elaborato ottiene il proprio DbContext, isolato dagli altri — come una
// richiesta HTTP nella Web API. (Vedi il commento sui tre lifetime in Program.cs
// dell'Api, Fase 1.)
// ----------------------------------------------------------------------------
var connectionString = builder.Configuration.GetConnectionString("AziendeDati")
    ?? throw new InvalidOperationException(
        "Connection string 'AziendeDati' mancante: impostala in local.settings.json " +
        "(Values → ConnectionStrings__AziendeDati) o nelle App Settings su Azure.");

builder.Services.AddDbContext<AziendeDbContext>(options =>
    options.UseSqlServer(connectionString));

// Build() crea l'host; Run() lo avvia e resta in ascolto degli eventi
// (HTTP, code, timer...) inoltrati dal runtime di Azure Functions.
builder.Build().Run();
