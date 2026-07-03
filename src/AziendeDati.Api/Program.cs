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

using System.Reflection;
using System.Text;
using AziendeDati.Api.Auth;
using AziendeDati.Api.Handlers;
using AziendeDati.Api.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using AziendeDati.Application.Dtos;
using AziendeDati.Application.Options;
using AziendeDati.Application.Services;
using AziendeDati.Application.Validators;
using FluentValidation;
using AziendeDati.Domain.Repositories;
using AziendeDati.Infrastructure;
using AziendeDati.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;

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

// GESTIONE CENTRALIZZATA DELLE ECCEZIONI (Fase 7):
// - AddProblemDetails abilita la produzione del formato standard ProblemDetails;
// - AddExceptionHandler registra la NOSTRA strategia (GlobalExceptionHandler),
//   che il middleware UseExceptionHandler (vedi pipeline) invocherà per ogni
//   eccezione non gestita. Tutta la logica di mappatura sta nell'handler.
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

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
builder.Services.AddScoped<IUtentiRepository, UtentiRepository>();
builder.Services.AddScoped<IAziendeService, AziendeService>();
builder.Services.AddScoped<ICategorieService, CategorieService>();
builder.Services.AddScoped<IDatiService, DatiService>();
builder.Services.AddScoped<IOrdiniService, OrdiniService>();

// FLUENTVALIDATION (Fase 6): scandisce l'assembly di Application e registra
// nel container OGNI classe che deriva da AbstractValidator<T>, come
// IValidator<T> (Scoped di default). Aggiungendo un validator nuovo non si
// torna a toccare questo file — stessa filosofia di ApplyConfigurationsFromAssembly.
builder.Services.AddValidatorsFromAssemblyContaining<OrdineCreateDtoValidator>();

// ============================================================================
// AUTENTICAZIONE JWT BEARER (Fase 8) — "chi sei?"
//
// Le impostazioni Jwt arrivano dall'Options pattern (Fase 5) e sono validate
// all'avvio (Fase 6). Qui però ci servono SUBITO (per configurare lo schema),
// quindi usiamo anche il binding manuale Get<T>() citato in Fase 5.
// ============================================================================
builder.Services.AddOptions<JwtOption>()
    .Bind(builder.Configuration.GetSection(JwtOption.SectionName))
    .ValidateDataAnnotations()
    .ValidateOnStart();

builder.Services.AddOptions<OAuthOption>()
    .Bind(builder.Configuration.GetSection(OAuthOption.SectionName));

var jwtOption = builder.Configuration.GetSection(JwtOption.SectionName).Get<JwtOption>()
    ?? throw new InvalidOperationException("Sezione 'Jwt' mancante in appsettings.json.");

// AddAuthentication registra i servizi e fissa lo SCHEMA DI DEFAULT ("Bearer"):
// è lo schema che UseAuthentication userà per ogni richiesta.
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    // AddJwtBearer insegna al middleware a: estrarre il token dall'header
    // "Authorization: Bearer <jwt>", verificarne la firma e VALIDARLO secondo
    // i TokenValidationParameters qui sotto. Token assente/invalido → la
    // richiesta prosegue ANONIMA (sarà l'autorizzazione a rispondere 401).
    .AddJwtBearer(options =>
    {
        // MapInboundClaims=false: i claim mantengono i NOMI ORIGINALI del JWT
        // ("sub", "role"), senza la rinomina storica negli URI SOAP/WS-Fed
        // (http://schemas.xmlsoap.org/...). Raccomandato dalla doc Microsoft:
        // https://learn.microsoft.com/aspnet/core/security/authentication/claims
        options.MapInboundClaims = false;

        options.TokenValidationParameters = new TokenValidationParameters
        {
            // FIRMA — il controllo più importante: garantisce che il token
            // l'abbiamo emesso NOI e che nessuno l'ha manomesso. Senza questa
            // verifica chiunque potrebbe fabbricarsi un token con i claim che vuole.
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOption.SigningKey)),

            // ISSUER ("iss") — il token deve venire dal NOSTRO emettitore:
            // un token firmato da altri (anche se con chiave compromessa
            // altrove) non deve passare.
            ValidateIssuer = true,
            ValidIssuer = jwtOption.Issuer,

            // AUDIENCE ("aud") — il token deve essere PER questa API: un token
            // legittimo emesso per un altro servizio non va accettato qui
            // (limita il "riuso" dei token tra sistemi).
            ValidateAudience = true,
            ValidAudience = jwtOption.Audience,

            // SCADENZA ("exp"/"nbf") — un token scaduto è spazzatura. ClockSkew
            // è la tolleranza per orologi non allineati tra server: il default
            // è 5 minuti, lo riduciamo per far rispettare meglio la scadenza.
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(1),

            // Con MapInboundClaims=false dobbiamo dire noi quali claim usare
            // per Identity.Name e per i RUOLI (RequireRole/IsInRole leggono
            // il claim indicato da RoleClaimType).
            NameClaimType = "sub",
            RoleClaimType = "role"
        };
    });

// ============================================================================
// AUTORIZZAZIONE A POLICY (Fase 8) — "cosa puoi fare?"
//
// RUOLI vs CLAIM — qual è la differenza?
// Un CLAIM è un'affermazione chiave/valore sull'identità ("role"="...",
// "azienda_id"="1"). I RUOLI non sono un meccanismo separato: sono
// SEMPLICEMENTE i claim del tipo indicato da RoleClaimType ("role" nel nostro
// caso). Quindi:
//  - RequireRole("x")        → zucchero sintattico: controlla il claim di
//    ruolo; in più abilita helper come User.IsInRole("x").
//  - RequireClaim("t", "v")  → generale: controlla QUALUNQUE claim, anche
//    non-ruolo (es. RequireClaim("azienda_id", "1")).
// I ruoli APPLICATIVI (tabella Ruoli nel DB) diventano claim "role" nel token
// al momento dell'emissione — è così che il modello di dominio si aggancia
// al modello di sicurezza.
//
// Le POLICY danno un NOME a un requisito: i controller citano il nome
// ([Authorize(Policy = ...)]), la definizione sta qui in un punto solo.
// ============================================================================
builder.Services.AddAuthorization(options =>
{
    // Policy per la SCRITTURA: richiede il ruolo owner (via RequireRole).
    options.AddPolicy(Policies.CompanyOwner,
        policy => policy.RequireRole("data.company.owner"));

    // Policy per la LETTURA: mostrata con RequireClaim (l'alternativa citata
    // dalla specifica). Accetta ENTRAMBI i ruoli: chi può scrivere può anche
    // leggere — senza questa riga un owner riceverebbe 403 sulle GET.
    options.AddPolicy(Policies.CompanyReader,
        policy => policy.RequireClaim("role", "data.company.reader", "data.company.owner"));
});

// CLAIMS TRANSFORMATION (Fase 8): Scoped perché dipende dal repository Scoped.
builder.Services.AddScoped<IClaimsTransformation, MyClaimsTransformation>();

// Il servizio che emette i token: Singleton (stateless, dipende solo da IOptions).
builder.Services.AddSingleton<ITokenService, JwtTokenService>();

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

// ============================================================================
// CORS (Fase 12) — permettere al frontend React (altra origine) di chiamare l'API.
//
// SAME-ORIGIN POLICY: per sicurezza il browser blocca le chiamate JavaScript
// (fetch/XHR) verso un'ORIGINE diversa da quella della pagina. "Origine" =
// schema + host + porta. In sviluppo il frontend gira su http://localhost:5173
// (Vite) e l'API su http://localhost:5184: stesso host ma PORTA diversa →
// origini diverse → senza CORS il browser rifiuterebbe le risposte
// ("blocked by CORS policy").
//
// COS'È IL CORS: è il modo con cui il SERVER dichiara "mi fido di quest'origine".
// Il browser manda un preflight (richiesta OPTIONS) e legge gli header
// Access-Control-Allow-* che questo middleware aggiunge; se l'origine è permessa,
// consegna la risposta al codice JS.
//
// ATTENZIONE: il CORS è un controllo del BROWSER, non una difesa del server
// (curl/Postman lo ignorano). La vera protezione resta il JWT: qui apriamo SOLO
// l'origine di sviluppo del frontend. In produzione l'origine (dominio reale)
// andrebbe letta dalla configurazione, non scritta nel codice.
// Fonte: https://learn.microsoft.com/aspnet/core/security/cors
// ============================================================================
const string corsFrontendDev = "FrontendDev";
builder.Services.AddCors(options =>
{
    options.AddPolicy(corsFrontendDev, policy =>
        policy.WithOrigins("http://localhost:5173") // l'origine di Vite in sviluppo
              .AllowAnyHeader()   // include Authorization (Bearer) e Content-Type
              .AllowAnyMethod()); // GET, POST, PUT, DELETE e il preflight OPTIONS
});

// ============================================================================
// SWAGGER / OPENAPI (Fase 9) — documentazione interattiva e test manuale dell'API.
//
// COS'È: Swashbuckle ispeziona a runtime controller, action e DTO e genera un
// documento OPENAPI (un JSON standard esposto su /swagger/v1/swagger.json) che
// DESCRIVE l'API: rotte, parametri, tipi di richiesta/risposta, codici di stato.
// Swagger UI è la pagina web (/swagger) che trasforma quel JSON in una console
// per PROVARE gli endpoint dal browser, senza scrivere un client.
// "OpenAPI" è la specifica (il formato del JSON); "Swagger" è la famiglia di
// strumenti che la usano (Swashbuckle, Swagger UI). I due termini si confondono
// spesso perché storicamente erano la stessa cosa.
//
// ATTENZIONE VERSIONI: Swashbuckle v10 usa Microsoft.OpenApi v2, che ha CAMBIATO
// alcune API rispetto a v1: i tipi ora stanno nel namespace `Microsoft.OpenApi`
// (non più `Microsoft.OpenApi.Models`) e AddSecurityRequirement vuole un DELEGATO
// (document => requisito) invece di un oggetto. Il codice qui sotto è per v2.
// Fonte: https://learn.microsoft.com/aspnet/core/tutorials/getting-started-with-swashbuckle
//        https://github.com/domaindrivendev/Swashbuckle.AspNetCore/blob/master/docs/migrating-to-v10.md
// ============================================================================

// ID dello schema di sicurezza: lo definiamo una volta e lo RIUSIAMO sia nella
// definizione sia nel requisito, così i due non possono divergere per un refuso.
const string schemaSicurezzaId = "Bearer";

builder.Services.AddSwaggerGen(options =>
{
    // 1) INFO DEL DOCUMENTO: titolo/versione/descrizione mostrati in cima alla UI.
    //    "v1" è l'ID del documento e compare nell'URL (/swagger/v1/swagger.json).
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "AziendeDati API",
        Version = "v1",
        Description = "Gestionale multi-azienda — API didattica (progetto AziendeDati).\n\n"
                    + "Per provare gli endpoint protetti: ottieni un JWT da POST /connect/token "
                    + "(grant_type=client_credentials), poi premi **Authorize** e incolla il token."
    });

    // 2) COMMENTI XML → DESCRIZIONI IN SWAGGER.
    //    Con <GenerateDocumentationFile>true</GenerateDocumentationFile> (nei .csproj)
    //    il compilatore estrae i /// <summary> in file XML. Qui li diamo a
    //    Swashbuckle così i summary diventano le descrizioni visibili nella UI.
    //    Servono DUE file: quello dell'assembly Api (i METODI/controller) e quello
    //    dell'assembly Application (i MODELLI/DTO) — la specifica chiede entrambi.
    //    I percorsi si costruiscono da AppContext.BaseDirectory (la cartella di
    //    output, bin/...): più robusto di un path fisso, funziona ovunque giri l'app.
    var apiXml = Path.Combine(AppContext.BaseDirectory,
        $"{Assembly.GetExecutingAssembly().GetName().Name}.xml");
    if (File.Exists(apiXml))
    {
        // includeControllerXmlComments: true → usa come descrizione del GRUPPO anche
        // il <summary> messo SULLA CLASSE del controller, non solo sulle action.
        options.IncludeXmlComments(apiXml, includeControllerXmlComments: true);
    }

    // AziendaReadDto è un tipo "marcatore" per raggiungere l'assembly Application
    // e ricavarne il nome (→ AziendeDati.Application.xml).
    var modelliXml = Path.Combine(AppContext.BaseDirectory,
        $"{typeof(AziendaReadDto).Assembly.GetName().Name}.xml");
    if (File.Exists(modelliXml))
    {
        options.IncludeXmlComments(modelliXml);
    }
    // PERCHÉ le guardie `if (File.Exists(...))`: se un XML non fosse stato generato,
    // IncludeXmlComments lancerebbe un'eccezione all'AVVIO. Con la guardia, al più
    // la UI mostra qualche descrizione in meno — ma l'app parte ("degrada con grazia").

    // 3) PULSANTE "AUTHORIZE" PER IL BEARER TOKEN — due passi:
    //    (a) DEFINIRE lo schema di autenticazione; (b) RICHIEDERLO sugli endpoint.
    //
    //    (a) AddSecurityDefinition descrive COM'È fatta l'autenticazione.
    //        Type = Http + Scheme = "bearer" è il modo corretto per un JWT Bearer.
    //        VANTAGGIO rispetto al vecchio Type = ApiKey: nel dialog Authorize si
    //        incolla SOLO il token "nudo" e Swagger aggiunge lui il prefisso
    //        "Bearer " all'header Authorization; con ApiKey dovevi scriverlo a mano.
    //        BearerFormat = "JWT" è solo informativo (etichetta mostrata nella UI).
    options.AddSecurityDefinition(schemaSicurezzaId, new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",              // nome dello schema HTTP, minuscolo per RFC 7235
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Name = "Authorization",
        Description = "Incolla qui SOLO il JWT (senza la parola 'Bearer'): "
                    + "lo ottieni da POST /connect/token con client_id/client_secret."
    });

    //    (b) AddSecurityRequirement dichiara che gli endpoint VOGLIONO quello schema,
    //        così la UI mostra il lucchetto e allega automaticamente il token.
    //        NOVITÀ v10/OpenApi v2: vuole un DELEGATO `document => requisito` e la
    //        chiave è un `OpenApiSecuritySchemeReference` (il vecchio
    //        `Reference = new OpenApiReference{...}` NON esiste più). L'Id del
    //        riferimento DEVE combaciare con quello dato ad AddSecurityDefinition.
    options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
        // chiave = riferimento allo schema "Bearer" definito sopra;
        // valore = elenco degli SCOPE OAuth2 richiesti. Un JWT Bearer non usa
        // scope, quindi lista VUOTA [].
        [new OpenApiSecuritySchemeReference(schemaSicurezzaId, document)] = []
    });
});

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

// 1) GESTIONE ECCEZIONI — PER PRIMA (placeholder della Fase 1, attivato in Fase 7).
//    PERCHÉ prima: il middleware di exception handling avvolge TUTTI i
//    successivi; solo stando in cima al "tubo" può catturare le eccezioni
//    lanciate da qualunque punto a valle (routing, auth, controller) e
//    trasformarle in una risposta JSON pulita (ProblemDetails) invece di un
//    errore grezzo. La strategia (mappatura eccezione → status code, logging,
//    niente stack trace in Production) sta in GlobalExceptionHandler.
//    NOTA: in Development ASP.NET Core aggiungerebbe di suo la Developer
//    Exception Page (pagina HTML con stack trace); registrando UseExceptionHandler
//    esplicitamente, la nostra gestione JSON vale in OGNI ambiente — un solo
//    comportamento da capire e testare.
app.UseExceptionHandler();

// 1.5) SWAGGER — SOLO in Development.
//    UseSwagger espone il documento OpenAPI (/swagger/v1/swagger.json);
//    UseSwaggerUI serve la pagina interattiva (/swagger) che lo consuma.
//    PERCHÉ solo in Development: in Production pubblicare la mappa completa
//    dell'API allarga la superficie d'attacco (chi la vuole online la protegge
//    o la pubblica a parte). In alternativa si potrebbe lasciarla attiva ma
//    dietro autorizzazione con MapSwagger().RequireAuthorization().
//    PERCHÉ QUI, prima di UseAuthentication/UseAuthorization: così la UI è
//    raggiungibile SENZA token; sarà poi il pulsante Authorize ad allegare il
//    JWT alle chiamate verso gli endpoint protetti.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        // Etichetta + URL del documento nel menù a tendina in alto a destra.
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "AziendeDati API v1");
    });
}

// 2) ROUTING — decide QUALE endpoint corrisponde alla richiesta (matching),
//    ma NON lo esegue ancora: da qui in poi i middleware successivi sanno
//    "dove si sta andando" e possono usarlo per decidere.
//    NOTA: WebApplication lo aggiungerebbe da solo in questa posizione;
//    lo scriviamo esplicitamente per rendere visibile l'ordine (niente magia).
app.UseRouting();

// 2.5) CORS — DOPO UseRouting, PRIMA di UseAuthentication/UseAuthorization.
//    PERCHÉ questa posizione: la policy CORS deve essere valutata prima che
//    l'autorizzazione possa cortocircuitare la richiesta e prima degli endpoint.
//    Se al preflight OPTIONS non arrivassero gli header CORS, il browser non
//    invierebbe mai la vera richiesta (con il Bearer token) e il frontend
//    vedrebbe solo errori CORS. Il nome richiama la policy registrata sopra.
app.UseCors(corsFrontendDev);

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

// ----------------------------------------------------------------------------
// TEST DI INTEGRAZIONE (Fase 11): con i "top-level statements" il compilatore
// genera una classe Program INTERNA e senza nome utilizzabile. WebApplicationFactory
// <Program> (Microsoft.AspNetCore.Mvc.Testing) però ha bisogno di un tipo PUBBLICO
// come punto d'ingresso per avviare l'app in memoria. Dichiarare qui una
// "partial class Program" pubblica e vuota la rende visibile al progetto di test,
// senza aggiungere codice: è il pattern raccomandato dalla doc Microsoft.
// Fonte: https://learn.microsoft.com/aspnet/core/test/integration-tests#basic-tests-with-the-default-webapplicationfactory
public partial class Program { }
