// ============================================================================
// Program.cs — punto di ingresso della Web API.
//
// Da .NET 6 in poi si usano i "top-level statements": niente classe Program né
// metodo Main espliciti, il compilatore li genera per noi. Il file si legge
// dall'alto verso il basso in DUE FASI ben distinte:
//
//   FASE A) builder  → REGISTRAZIONE dei servizi nel container di Dependency
//                      Injection (tutto ciò che è builder.Services.Add...).
//   FASE B) app      → COSTRUZIONE della PIPELINE HTTP (i middleware) e
//                      mapping degli endpoint.
//
// Tenere a mente questa separazione aiuta a capire ogni Program.cs di ASP.NET Core.
// La pipeline vera e propria (routing, auth, exception handling...) verrà
// costruita e commentata passo passo nella Fase 1.
// ============================================================================

var builder = WebApplication.CreateBuilder(args);

// --- FASE A: registrazione servizi -----------------------------------------

// AddHealthChecks registra i servizi del sistema di "health check" di ASP.NET Core.
// COSA SONO: endpoint che dichiarano se l'app è viva/sana; li interrogano sistemi
// esterni (load balancer, Kubernetes, monitoraggio) per decidere se mandare traffico.
// PERCHÉ usare il meccanismo integrato invece di un banale MapGet("/health", ...)
// che risponde "OK": in futuro potremo agganciare controlli reali (es. "il database
// risponde?") con AddCheck/AddDbContextCheck senza cambiare l'endpoint.
// Fonte: https://learn.microsoft.com/aspnet/core/host-and-deploy/health-checks
builder.Services.AddHealthChecks();

var app = builder.Build();

// --- FASE B: pipeline HTTP ed endpoint --------------------------------------

// MapHealthChecks espone l'endpoint GET /health: esegue tutti i check registrati
// e risponde 200 "Healthy" se va tutto bene (503 se un check fallisce).
// Oggi non abbiamo check specifici, quindi "app avviata" = "sana".
app.MapHealthChecks("/health");

// Run() avvia il web server Kestrel e BLOCCA il thread finché l'app non viene
// fermata (Ctrl+C): da qui in poi l'app resta in ascolto delle richieste HTTP.
app.Run();
