# aziende-dati-gestionale

Gestionale full-stack **didattico**: Web API ASP.NET Core (.NET 10) con EF Core,
JWT/OAuth2, Swagger e Azure Functions + frontend React/TypeScript.
Costruito passo passo per imparare le tecnologie enterprise .NET: il codice è
commentato in italiano per spiegare *cosa* fa e *perché*.

**Architettura**: soluzione a più progetti con la "dependency rule" della
Clean/Onion Architecture — `Api → Application → Domain`, `Infrastructure → Domain`;
i controller non toccano mai il database direttamente.

## Prerequisiti

| Strumento | Versione | Note |
|---|---|---|
| [.NET SDK](https://dotnet.microsoft.com/download/dotnet/10.0) | **10.0** | verifica con `dotnet --version` |
| SQL Server **LocalDB** | qualsiasi recente | incluso in Visual Studio ("Sviluppo desktop e Web"); verifica con `sqllocaldb info` |
| Node.js | 20+ | solo per il frontend (Fase 12) |
| Azurite + Azure Functions Core Tools | ultime | solo per le Functions in locale (Fase 10) |

## Comandi principali

Tutti i comandi si lanciano dalla **radice** del repository.

```bash
# Compila l'intera soluzione
dotnet build

# Avvia la Web API (http://localhost:5184, vedi launchSettings.json)
dotnet run --project src/AziendeDati.Api

# Verifica rapida: endpoint di health
curl http://localhost:5184/health     # → 200 "Healthy"

# Esegue i test
dotnet test

# Migrations EF Core (dalla Fase 2 in poi)
dotnet ef migrations add NomeMigrazione --project src/AziendeDati.Infrastructure --startup-project src/AziendeDati.Api
dotnet ef database update --project src/AziendeDati.Infrastructure --startup-project src/AziendeDati.Api
```

## Database

SQL Server LocalDB, stringa di connessione (dalla Fase 2):

```
Server=(localdb)\MSSQLLocalDB;Database=AziendeDati;Trusted_Connection=True;MultipleActiveResultSets=true
```

## Stato di avanzamento

| Fase | Contenuto | Stato |
|---|---|---|
| 0 | Setup soluzione multi-progetto + `/health` | ✅ |
| 1 | Web API base, middleware, Dependency Injection | ✅ |
| 2 | EF Core: DbContext, Fluent API, migrations, seed | ✅ |
| 3 | Repository, servizi applicativi, DTO | ✅ |
| 4 | LINQ e query di report | ✅ |
| 5 | Configurazione e Options pattern | ✅ |
| 6 | Validazione (data annotations + FluentValidation) | ✅ |
| 7 | Gestione centralizzata eccezioni (ProblemDetails) | ✅ |
| 8 | Autenticazione JWT + OAuth2, policy, claims | ⬜ |
| 9 | Swagger / Swashbuckle | ⬜ |
| 10 | Azure Functions (HTTP + Queue trigger) | ⬜ |
| 11 | Test unitari e di integrazione | ⬜ |
| 12 | Frontend SPA React + TypeScript | ⬜ |
