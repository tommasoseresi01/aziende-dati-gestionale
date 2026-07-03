// Store del token JWT, condiviso tra l'interceptor Axios e il contesto Auth.
//
// STRATEGIA DI STORAGE (trade-off di sicurezza — punto richiesto dalla specifica):
//  - IN MEMORIA (variabile JS): il più sicuro contro il furto (sparisce chiudendo
//    la tab, altri script non lo ritrovano dopo un reload), MA si perde a ogni refresh.
//  - sessionStorage: sopravvive al refresh della tab, ma è leggibile da JavaScript
//    → vulnerabile a XSS (uno script iniettato potrebbe rubarlo).
//  - cookie httpOnly: il più robusto contro XSS (JS non lo legge), ma richiede
//    gestione lato server (Set-Cookie, protezione CSRF): fuori dallo scope qui.
// COMPROMESSO DIDATTICO: teniamo il token in memoria E in sessionStorage, così il
// login "resiste" al refresh restando semplice da capire.

const STORAGE_KEY = 'aziendedati.token'

let tokenInMemoria: string | null = sessionStorage.getItem(STORAGE_KEY)

export function getToken(): string | null {
  return tokenInMemoria
}

export function setToken(token: string): void {
  tokenInMemoria = token
  sessionStorage.setItem(STORAGE_KEY, token)
}

export function clearToken(): void {
  tokenInMemoria = null
  sessionStorage.removeItem(STORAGE_KEY)
}
