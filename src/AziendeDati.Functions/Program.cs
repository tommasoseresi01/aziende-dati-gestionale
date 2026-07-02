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
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.Hosting;

var builder = FunctionsApplication.CreateBuilder(args);

// Qui, nella Fase 10, registreremo i servizi necessari alle nostre function
// (es. accesso al database per salvare i Dati ricevuti dalla coda).

// Build() crea l'host; Run() lo avvia e resta in ascolto degli eventi
// (HTTP, code, timer...) inoltrati dal runtime di Azure Functions.
builder.Build().Run();
