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
// ============================================================================

using AziendeDati.Api.Services;

var builder = WebApplication.CreateBuilder(args);

// ----------------------------------------------------------------------------
// FASE A: registrazione servizi nel container di Dependency Injection
// ----------------------------------------------------------------------------
// COS'È LA DEPENDENCY INJECTION (DI): invece di creare le dipendenze con "new"
// dentro le classi, le classi DICHIARANO cosa gli serve (parametri del
// costruttore) e un "container" centrale le costruisce, inietta e distrugge al
// momento giusto. ASP.NET Core ha un container integrato: qui sotto gli diciamo
// QUALI servizi esistono e con QUALE CICLO DI VITA (lifetime).
// Fonte: https://learn.microsoft.com/aspnet/core/fundamentals/dependency-injection

// AddControllers registra i servizi del framework MVC necessari ai controller
// API (routing per attributi, model binding, serializzazione JSON, filtri).
// NOTA: registra i servizi ma NON espone ancora nulla: gli endpoint compaiono
// solo con app.MapControllers() più in basso.
builder.Services.AddControllers();

// Health check integrati (introdotti nella Fase 0): endpoint /health per
// load balancer e sistemi di monitoraggio.
builder.Services.AddHealthChecks();

// Servizi di autenticazione ("chi sei?") e autorizzazione ("cosa puoi fare?").
// Oggi sono GUSCI VUOTI: nessuno schema configurato, nessuna policy. Li
// registriamo già ora perché i middleware UseAuthentication/UseAuthorization
// (vedi pipeline sotto) richiedono questi servizi per partire. Nella Fase 8
// qui verranno configurati JWT Bearer e le policy sui ruoli.
builder.Services.AddAuthentication();
builder.Services.AddAuthorization();

// ----------------------------------------------------------------------------
// I TRE LIFETIME della Dependency Injection — il concetto più chiesto all'esame.
// Il lifetime decide QUANTE istanze del servizio esistono e QUANDO vengono create:
//
// • TRANSIENT  → nuova istanza OGNI VOLTA che qualcuno chiede il servizio
//                (anche più volte nella stessa richiesta HTTP).
//                Esempio pratico: un formattatore/calcolatore leggero e senza
//                stato di cui non importa condividere nulla.
//                  services.AddTransient<INumeroFatturaFormatter, ...>()
//
// • SCOPED     → UNA istanza per RICHIESTA HTTP: tutti i componenti che
//                collaborano alla stessa richiesta ricevono la STESSA istanza,
//                richieste diverse ricevono istanze diverse.
//                Esempio pratico: il DbContext di EF Core (Fase 2) — così tutta
//                la richiesta condivide la stessa "unità di lavoro"/transazione,
//                ma due richieste concorrenti non si pestano i piedi (il
//                DbContext NON è thread-safe).
//                  services.AddScoped<IAziendeService, AziendeService>()
//
// • SINGLETON  → UNA SOLA istanza per TUTTA la vita dell'applicazione,
//                condivisa da tutte le richieste. DEVE essere thread-safe.
//                Esempio pratico: una cache in memoria, un client HTTP
//                configurato, o il nostro ClockService (stateless: leggere
//                l'orologio non ha stato da proteggere).
//                  services.AddSingleton<IClockService, ClockService>()
//
// ERRORE CLASSICO DA EVITARE (captive dependency): un Singleton che riceve nel
// costruttore un servizio Scoped. Il Singleton vive per sempre e "intrappola"
// lo Scoped, che smette di essere per-richiesta → bug subdoli e accessi
// concorrenti. Il container in Development lo rileva e lancia un'eccezione.
// Regola pratica: un servizio può dipendere solo da servizi con lifetime
// UGUALE o PIÙ LUNGO del suo.
// ----------------------------------------------------------------------------
builder.Services.AddSingleton<IClockService, ClockService>();

var app = builder.Build();

// ----------------------------------------------------------------------------
// FASE B: la PIPELINE dei middleware — L'ORDINE È TUTTO.
//
// Ogni richiesta HTTP attraversa i middleware NELL'ORDINE in cui sono
// registrati qui (e la risposta li riattraversa a ritroso, come una cipolla):
//
//   richiesta → [ExceptionHandler] → [Routing] → [AuthN] → [AuthZ] → endpoint
//   risposta  ← ........................................................←
//
// Ogni middleware può: elaborare la richiesta, passarla al successivo, o
// CORTOCIRCUITARE (rispondere subito senza proseguire — es. AuthZ che nega
// l'accesso con 403 senza mai eseguire il controller).
// Fonte: https://learn.microsoft.com/aspnet/core/fundamentals/middleware/#middleware-order
// ----------------------------------------------------------------------------

// 1) GESTIONE ECCEZIONI — deve stare PER PRIMA. ★ PLACEHOLDER: si attiva in Fase 7 ★
//    PERCHÉ prima: il middleware di exception handling avvolge TUTTI i
//    successivi; solo stando in cima al "tubo" può catturare le eccezioni
//    lanciate da qualunque punto a valle (routing, auth, controller) e
//    trasformarle in una risposta JSON pulita (ProblemDetails) invece di un
//    errore grezzo. Nella Fase 7 scriveremo:
//      app.UseExceptionHandler(...);

// 2) ROUTING — decide QUALE endpoint corrisponde alla richiesta (matching),
//    ma NON lo esegue ancora: da qui in poi i middleware successivi sanno
//    "dove si sta andando" e possono usarlo per decidere.
//    NOTA: WebApplication lo aggiungerebbe da solo in questa posizione;
//    lo scriviamo esplicitamente per rendere visibile l'ordine (niente magia).
app.UseRouting();

// 3) AUTENTICAZIONE — stabilisce CHI STA CHIAMANDO: legge le credenziali
//    (nella Fase 8: il token JWT nell'header Authorization) e costruisce
//    l'identità (ClaimsPrincipal) in HttpContext.User. Non blocca nessuno:
//    si limita a "mettere il cartellino" alla richiesta.
app.UseAuthentication();

// 4) AUTORIZZAZIONE — decide SE chi chiama PUÒ accedere all'endpoint scelto
//    dal routing (ruoli, policy, [Authorize]). Se no: 401/403 e cortocircuito.
//
//    PERCHÉ l'ordine UseAuthentication → UseAuthorization è OBBLIGATORIO:
//    l'autorizzazione ragiona sull'identità (HttpContext.User) che SOLO
//    l'autenticazione costruisce. Invertirli significa autorizzare un utente
//    ancora anonimo → tutto ciò che richiede login risponderebbe 401 anche
//    con credenziali valide. Logica prima ancora che tecnica: prima sai CHI È,
//    poi decidi COSA PUÒ FARE.
app.UseAuthorization();

// 5) ENDPOINT — l'ultimo anello: esegue l'endpoint scelto dal routing.
//    MapControllers attiva le route dichiarate con gli attributi [Route]/[HttpGet]
//    sui controller (es. PingController → GET /api/ping).
app.MapControllers();

// Endpoint di health check (Fase 0): 200 "Healthy" se l'app è sana.
app.MapHealthChecks("/health");

// Run() avvia il web server Kestrel e BLOCCA il thread finché l'app non viene
// fermata (Ctrl+C): da qui in poi l'app resta in ascolto delle richieste HTTP.
app.Run();
