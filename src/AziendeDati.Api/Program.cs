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
using AziendeDati.Application.Options;
using AziendeDati.Application.Services;
using AziendeDati.Application.Validators;
using FluentValidation;
using AziendeDati.Domain.Repositories;
using AziendeDati.Infrastructure;
using AziendeDati.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

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

// ----------------------------------------------------------------------------
// REGISTRAZIONE DEL DbContext (Fase 2).
//
// La stringa di connessione NON si scrive nel codice ma in appsettings.json
// (sezione "ConnectionStrings"): GetConnectionString("AziendeDati") la legge da
// lì. Il "?? throw" fallisce SUBITO all'avvio se manca — meglio un errore
// esplicito al bootstrap che un NullReferenceException alla prima query.
//
// AddDbContext registra AziendeDbContext con lifetime SCOPED (è il default e
// NON va cambiato). PERCHÉ il DbContext DEVE essere Scoped:
//  1. UNA ISTANZA PER RICHIESTA HTTP: tutti i servizi della stessa richiesta
//     condividono lo stesso change tracker e la stessa unità di lavoro →
//     un solo SaveChangesAsync atomico a fine operazione.
//  2. NON È THREAD-SAFE: se fosse Singleton, richieste concorrenti userebbero
//     la STESSA istanza da thread diversi → corruzione del change tracker ed
//     eccezioni ("A second operation was started on this context...").
//  3. Se fosse Transient, servizi diversi della stessa richiesta avrebbero
//     contesti DIVERSI: tracking incoerente e transazioni spezzate.
//  In più, a fine richiesta il container fa il Dispose del contesto per noi
//  (la connessione torna al pool automaticamente).
// Fonte: https://learn.microsoft.com/ef/core/dbcontext-configuration/#the-dbcontext-lifetime
// ----------------------------------------------------------------------------
var connectionString = builder.Configuration.GetConnectionString("AziendeDati")
    ?? throw new InvalidOperationException("Connection string 'AziendeDati' mancante in appsettings.json.");

builder.Services.AddDbContext<AziendeDbContext>(options =>
{
    options.UseSqlServer(connectionString); // provider SQL Server (pacchetto Microsoft.EntityFrameworkCore.SqlServer)

    // ------------------------------------------------------------------------
    // COME VEDERE L'SQL GENERATO DA EF CORE (Fase 4).
    // EF logga già ogni comando SQL sul logger di ASP.NET Core (categoria
    // "Microsoft.EntityFrameworkCore.Database.Command", livello Information):
    // basta che appsettings.Development.json non la filtri.
    // Qui aggiungiamo, SOLO in Development:
    //  - EnableSensitiveDataLogging: mostra anche i VALORI dei parametri
    //    (@__p_0 = 5) invece di "?". VIETATO in Production: i log
    //    conterrebbero dati personali/sensibili degli utenti.
    //  - EnableDetailedErrors: messaggi di errore più ricchi (costo extra).
    // ALTERNATIVA "fai da te": options.LogTo(Console.WriteLine) manda i log
    // dove vuoi tu (utile nei test/console app, ridondante qui perché il
    // logger di ASP.NET stampa già su console).
    // Fonte: https://learn.microsoft.com/ef/core/logging-events-diagnostics/
    // ------------------------------------------------------------------------
    if (builder.Environment.IsDevelopment())
    {
        options.EnableSensitiveDataLogging();
        options.EnableDetailedErrors();
    }
});

// ----------------------------------------------------------------------------
// REGISTRAZIONE REPOSITORY E SERVIZI APPLICATIVI (Fase 3).
//
// Qui la DI "chiude il cerchio" della Dependency Inversion: le interfacce
// stanno in Domain (repository) e in Application (servizi), le implementazioni
// in Infrastructure e Application — e SOLO questo file sa quale implementazione
// concreta corrisponde a quale interfaccia. È l'unico punto in cui Api usa il
// riferimento a Infrastructure (come promesso in ARCHITETTURA.md).
//
// PERCHÉ Scoped e non altro? Questi servizi dipendono (direttamente o in
// catena) dal DbContext, che è Scoped:
//  - Singleton è VIETATO: intrappolerebbe il DbContext per sempre (captive
//    dependency, vedi il commento sui lifetime più su) e lo condividerebbe
//    tra richieste concorrenti — proprio ciò che Scoped evita.
//  - Transient funzionerebbe, ma senza vantaggi: creerebbe più istanze di
//    servizio nella stessa richiesta, che comunque condividono lo STESSO
//    DbContext scoped. Meglio la coerenza: tutta la catena
//    controller → servizio → repository → DbContext vive "per richiesta".
// ----------------------------------------------------------------------------
builder.Services.AddScoped<IAziendeRepository, AziendeRepository>();
builder.Services.AddScoped<ICategorieRepository, CategorieRepository>();
builder.Services.AddScoped<IDatiRepository, DatiRepository>();
builder.Services.AddScoped<IOrdiniRepository, OrdiniRepository>();
builder.Services.AddScoped<IAziendeService, AziendeService>();
builder.Services.AddScoped<ICategorieService, CategorieService>();
builder.Services.AddScoped<IDatiService, DatiService>();
builder.Services.AddScoped<IOrdiniService, OrdiniService>();

// FLUENTVALIDATION (Fase 6): scandisce l'assembly di Application e registra
// nel container OGNI classe che deriva da AbstractValidator<T>, come
// IValidator<T> (Scoped di default). Aggiungendo un validator nuovo non si
// torna a toccare questo file — stessa filosofia di ApplyConfigurationsFromAssembly.
builder.Services.AddValidatorsFromAssemblyContaining<OrdineCreateDtoValidator>();

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

// ----------------------------------------------------------------------------
// OPTIONS PATTERN (Fase 5): configurazione fortemente tipizzata.
//
// Configure<T> fa due cose: (1) BINDING — lega la sezione "EmailService" di
// appsettings.json alla classe EmailServiceOption (proprietà omonime, tipi
// convertiti); (2) REGISTRAZIONE — rende disponibili IOptions<T>,
// IOptionsSnapshot<T> e IOptionsMonitor<T> nel container DI.
//
// ALTERNATIVE per leggere la configurazione (citate per confronto):
//  - var opt = new EmailServiceOption();
//    builder.Configuration.GetSection("EmailService").Bind(opt);
//    → binding manuale una tantum: comodo quando il valore serve QUI in
//      Program.cs durante il bootstrap, non nei servizi via DI.
//  - builder.Configuration.GetValue<int>("EmailService:Port")
//    → lettura puntuale di UNA chiave (nota i ":" per scendere nella
//      gerarchia JSON): ok per valori singoli, ma è una "magic string" —
//      refusi scoperti solo a runtime.
// L'Options pattern resta la scelta giusta per gruppi di impostazioni
// consumati dai servizi.
//
// GERARCHIA DELLE SORGENTI (chi vince in caso di conflitto):
// WebApplication.CreateBuilder registra le sorgenti IN ORDINE, e chi viene
// DOPO SOVRASCRIVE chi viene prima (a parità di chiave):
//   1. appsettings.json                    (base, committato)
//   2. appsettings.{Environment}.json      (es. .Development.json: override per ambiente)
//   3. User Secrets                        (solo in Development: segreti FUORI dal repo)
//   4. Variabili d'ambiente                (es. EmailService__Port=2525 — il
//                                           doppio underscore "__" sostituisce i ":")
//   5. Argomenti da riga di comando        (--EmailService:Port=9999)
// Quindi una variabile d'ambiente vince su TUTTI i file JSON: è così che in
// produzione (container, cloud) si configura l'app senza toccare i file.
// Fonte: https://learn.microsoft.com/aspnet/core/fundamentals/configuration/
// ----------------------------------------------------------------------------
// AddOptions + ValidateDataAnnotations (Fase 6): le option si VALIDANO come i
// DTO. ValidateOnStart anticipa il controllo all'AVVIO dell'app: se in
// appsettings.json manca l'Host o la porta è fuori range, l'app NON PARTE e
// dice subito quale chiave è sbagliata — molto meglio di scoprirlo alla prima
// email fallita in produzione ("fail fast").
builder.Services.AddOptions<EmailServiceOption>()
    .Bind(builder.Configuration.GetSection(EmailServiceOption.SectionName))
    .ValidateDataAnnotations()
    .ValidateOnStart();

// EmailService è Singleton: non ha stato per-richiesta e dipende solo da
// IOptions<T> (a sua volta Singleton) — lifetime uguale o più lungo, regola ok.
builder.Services.AddSingleton<IEmailService, EmailService>();

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
