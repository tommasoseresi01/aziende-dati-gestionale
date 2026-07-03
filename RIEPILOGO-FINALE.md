# Riepilogo finale — mappa esame → codice

Questo documento collega **ogni argomento d'esame** di Programmazione Enterprise .NET
al **file e/o endpoint** del progetto `AziendeDati` che lo esercita, e — dove presente —
al **test** che lo verifica. È la chiusura del ciclo di apprendimento (Fase 11).

## Come eseguire i test

```bash
dotnet test            # dalla radice del repository
```

Esito atteso: **13 test verdi** (8 unitari + 5 di integrazione). I test unitari usano
un DbContext **InMemory** (nessun SQL Server necessario); quelli di integrazione avviano
l'intera Web API in memoria con `WebApplicationFactory` e un database InMemory seminato.

## Mappa argomento d'esame → file / endpoint

| Argomento d'esame | Dove nel progetto (file / endpoint) | Test che lo verifica |
|---|---|---|
| **ASP.NET Core Web API + pipeline middleware** | [Program.cs](src/AziendeDati.Api/Program.cs) (ordine `UseExceptionHandler → UseRouting → UseAuthentication → UseAuthorization → MapControllers`); `ControllerBase` + `[ApiController]` in [PingController.cs](src/AziendeDati.Api/Controllers/PingController.cs) · `GET /api/ping` | — |
| **Dependency Injection e lifetime** (Transient/Scoped/Singleton) | [Program.cs](src/AziendeDati.Api/Program.cs) (commento sui 3 lifetime; `AddScoped` servizi/repository, `AddSingleton` per `IClockService`/`IEmailService`/`ITokenService`, `AddDbContext` scoped) · [ClockService.cs](src/AziendeDati.Api/Services/ClockService.cs) | (indiretto: tutti i test risolvono i servizi via DI) |
| **EF Core: DbContext, SaveChanges, Fluent API** | [AziendeDbContext.cs](src/AziendeDati.Infrastructure/AziendeDbContext.cs) · [Configurations/](src/AziendeDati.Infrastructure/Configurations) (`IEntityTypeConfiguration<T>`) · [Migrations/](src/AziendeDati.Infrastructure/Migrations) | [CategorieServiceTests.cs](tests/AziendeDati.Tests/Unit/CategorieServiceTests.cs) |
| **Modellazione relazionale** (chiavi, 1‑N, navigation, `OnDelete`) | [Entities/](src/AziendeDati.Domain/Entities) · Configurations con `HasOne/WithMany/HasForeignKey/OnDelete` (es. [UtenteConfiguration.cs](src/AziendeDati.Infrastructure/Configurations/UtenteConfiguration.cs)) | — |
| **LINQ su EF Core** (GroupBy, Sum, OrderByDescending, proiezioni, deferred execution) | [DatiRepository.cs](src/AziendeDati.Infrastructure/Repositories/DatiRepository.cs) `GetSommaPerCategoriaAsync` · `GET /api/report/somma-per-categoria` ([ReportController.cs](src/AziendeDati.Api/Controllers/ReportController.cs)) | [ReportQueryTests.cs](tests/AziendeDati.Tests/Unit/ReportQueryTests.cs) |
| **Configurazione e Options pattern** (`IOptions`, `Configure`, `ValidateOnStart`) | [EmailServiceOption.cs](src/AziendeDati.Application/Options/EmailServiceOption.cs) · [JwtOption.cs](src/AziendeDati.Api/Auth/JwtOption.cs) · [OAuthOption.cs](src/AziendeDati.Api/Auth/OAuthOption.cs) · [Program.cs](src/AziendeDati.Api/Program.cs) · [EmailService.cs](src/AziendeDati.Application/Services/EmailService.cs) · `GET /api/ping/email-config` | — |
| **Validazione modelli** (data annotations + `[ApiController]` + FluentValidation) | Data annotations: [AziendaDtos.cs](src/AziendeDati.Application/Dtos/AziendaDtos.cs), [CategoriaDtos.cs](src/AziendeDati.Application/Dtos/CategoriaDtos.cs) · FluentValidation: [OrdineCreateDtoValidator.cs](src/AziendeDati.Application/Validators/OrdineCreateDtoValidator.cs) | [OrdineCreateDtoValidatorTests.cs](tests/AziendeDati.Tests/Unit/OrdineCreateDtoValidatorTests.cs) · **400** in [ApiEndpointsTests.cs](tests/AziendeDati.Tests/Integration/ApiEndpointsTests.cs) |
| **Gestione centralizzata eccezioni** (`UseExceptionHandler`, `ProblemDetails`) | [GlobalExceptionHandler.cs](src/AziendeDati.Api/Handlers/GlobalExceptionHandler.cs) · eccezioni di dominio [NotFoundException.cs](src/AziendeDati.Domain/Exceptions/NotFoundException.cs), [ValidationException.cs](src/AziendeDati.Domain/Exceptions/ValidationException.cs) · `GET /api/ping/errore` | **500 → ProblemDetails** in [ApiEndpointsTests.cs](tests/AziendeDati.Tests/Integration/ApiEndpointsTests.cs) |
| **Autenticazione JWT Bearer + OAuth2 Client Credentials** | [Program.cs](src/AziendeDati.Api/Program.cs) (`AddJwtBearer` + `TokenValidationParameters`) · [JwtTokenService.cs](src/AziendeDati.Api/Auth/JwtTokenService.cs) · `POST /connect/token` ([TokenController.cs](src/AziendeDati.Api/Controllers/TokenController.cs)) | **401** + emissione token in [ApiEndpointsTests.cs](tests/AziendeDati.Tests/Integration/ApiEndpointsTests.cs) |
| **Autorizzazione per ruoli e claim + policy** | [Program.cs](src/AziendeDati.Api/Program.cs) (`AddAuthorization`: `RequireRole`/`RequireClaim`) · [Policies.cs](src/AziendeDati.Api/Auth/Policies.cs) · `[Authorize(Policy=…)]` in [AziendeController.cs](src/AziendeDati.Api/Controllers/AziendeController.cs) | **403** (reader→scrittura) e **200** (reader→lettura) in [ApiEndpointsTests.cs](tests/AziendeDati.Tests/Integration/ApiEndpointsTests.cs) |
| **Claims transformation** (`IClaimsTransformation`) | [MyClaimsTransformation.cs](src/AziendeDati.Api/Auth/MyClaimsTransformation.cs) (ricarica l'utente dal DB, ruolo reale, blocca i disattivati) | (esercitata da ogni test con token: il ruolo effettivo arriva dal DB) |
| **Swagger / Swashbuckle** (OpenAPI, commenti XML, auth in UI) | [Program.cs](src/AziendeDati.Api/Program.cs) (`AddSwaggerGen`/`UseSwagger`/`UseSwaggerUI` + `AddSecurityDefinition` Bearer) · `<GenerateDocumentationFile>` in [AziendeDati.Api.csproj](src/AziendeDati.Api/AziendeDati.Api.csproj) · `GET /swagger` | — |
| **Azure Functions** (trigger HTTP e Queue) | HTTP trigger `POST /api/import-dato` [ImportDatoHttpFunction.cs](src/AziendeDati.Functions/Functions/ImportDatoHttpFunction.cs) · Queue trigger [ProcessaDatoQueueFunction.cs](src/AziendeDati.Functions/Functions/ProcessaDatoQueueFunction.cs) · [Program.cs](src/AziendeDati.Functions/Program.cs) | (verifica locale con Azurite, vedi FASE-10-NOTE) |
| **Frontend SPA React + TypeScript** e **autenticazione lato client** | **Fase 12 — da implementare** (`frontend/aziende-dati-web/`) | — |

## Dettaglio della suite di test (Fase 11)

**Test unitari** (`tests/AziendeDati.Tests/Unit/`)
- **CategorieServiceTests** — servizio applicativo su repository reale + DbContext InMemory:
  `GetById` lancia `NotFoundException`; `Create` persiste e rilegge; `GetAll` ordina per nome.
- **OrdineCreateDtoValidatorTests** — FluentValidation: ordine valido; senza righe; riga con
  quantità 0 (errore indicizzato `Righe[0].Quantita`); data nel futuro.
- **ReportQueryTests** — la query LINQ `GroupBy + Sum + OrderByDescending` produce i totali
  corretti nell'ordine decrescente.

**Test di integrazione** (`tests/AziendeDati.Tests/Integration/`, `WebApplicationFactory` + InMemory)
- **401** `GET /api/aziende` senza token · **403** `POST /api/aziende` con token *reader* ·
  **200** `GET /api/aziende` con token *reader* · **400** `POST /api/aziende` con payload
  invalido (token *owner*) · **500 → ProblemDetails** su `GET /api/ping/errore`.

## Stato delle fasi

Fasi 0–11 completate. Manca la **Fase 12** (frontend React + TypeScript), che consumerà
questa Web API via HTTP/JSON (login, CRUD, dashboard con il report somma‑per‑categoria).
