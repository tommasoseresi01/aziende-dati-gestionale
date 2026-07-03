# AziendeDati — Frontend (React + TypeScript)

SPA che consuma la Web API .NET del progetto `AziendeDati` (Fase 12).
Login via JWT, dashboard con grafico, CRUD Aziende/Categorie, lista Dati filtrabile,
tema chiaro/scuro e azioni visibili in base al ruolo (owner vs reader).

## Stack

React 19 · TypeScript · Vite · Tailwind CSS v4 · componenti in stile shadcn/ui ·
TanStack Query (fetching/cache) · TanStack Table (tabelle) · React Router ·
React Hook Form + Zod (form e validazione) · Axios (con interceptor JWT) · Recharts (grafico).

## Prerequisiti

- **Node.js 20+** (`node --version`)
- La **Web API .NET in esecuzione** (di default su `http://localhost:5184`):
  ```bash
  # dalla radice del repository
  dotnet run --project src/AziendeDati.Api
  ```
  Il backend abilita il **CORS** per `http://localhost:5173` (la porta di questo frontend).

## Avvio

```bash
npm install          # installa le dipendenze
npm run dev          # avvia su http://localhost:5173
```

Poi apri http://localhost:5173. Accedi con una delle **credenziali demo** (dati di seed):

| Ruolo  | Client ID            | Client Secret             |
| ------ | -------------------- | ------------------------- |
| owner  | `acme-owner-client`  | `secret-owner-acme-2025`  |
| reader | `acme-reader-client` | `secret-reader-acme-2025` |

> L'**owner** vede i pulsanti di creazione/modifica/eliminazione; il **reader** vede tutto in sola lettura.

## Configurazione

L'URL dell'API si imposta in `.env` (vedi `.env.example`):

```
VITE_API_URL=http://localhost:5184
```

## Script

```bash
npm run dev           # server di sviluppo
npm run build         # type-check (tsc) + build di produzione (dist/)
npm run preview       # anteprima del build
npm run lint          # ESLint
npm run format        # Prettier (scrive)
npm run format:check  # Prettier (solo verifica)
```

## Struttura del progetto (cartella `src/`)

- `api/` — client HTTP (Axios) e una funzione per ogni endpoint del backend.
- `auth/` — contesto di autenticazione, storage del token, decodifica JWT, route guard.
- `hooks/` — hook di TanStack Query (dati del server: cache + stati loading/errore).
- `components/ui/` — primitive di interfaccia in stile shadcn/ui (button, card, dialog, table…).
- `components/layout/` — layout applicativo (sidebar + header).
- `components/theme/` — tema chiaro/scuro.
- `components/common/` — tabella dati, stati (loading/errore/vuoto), dialog di conferma.
- `pages/` — le schermate (Login, Dashboard, Aziende, Categorie, Dati).
- `types/` — le interfacce TypeScript che rispecchiano i DTO del backend.

## Screenshot / GIF

> _Inserisci qui uno screenshot o una GIF dell'app._
>
> ![Screenshot dell'app](docs/screenshot.png)
>
> Suggerimento: avvia backend + frontend, fai il login come `owner`, apri la
> Dashboard e cattura la schermata (salvandola in `docs/screenshot.png`).
