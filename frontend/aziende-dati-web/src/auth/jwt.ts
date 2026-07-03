import type { Ruolo } from '@/types/api'

/** I dati dell'utente ricavati dai claim del token. */
export interface UtenteAutenticato {
  username: string
  ruolo: Ruolo
  aziendaId: number
  scadenza: number // claim "exp": secondi dall'epoch
}

// Decodifica il PAYLOAD del JWT (la parte centrale, base64url). NON verifica la
// firma: quello è compito del server. Al client i claim servono solo per l'UI
// (mostrare/nascondere le funzioni in base al ruolo). Non ci si "fida" di questi
// dati per la sicurezza vera: ogni chiamata è comunque validata dal backend.
export function decodeJwt(token: string): UtenteAutenticato | null {
  try {
    const payloadBase64Url = token.split('.')[1]
    // base64url → base64 (- → +, _ → /) prima di atob.
    const json = atob(payloadBase64Url.replace(/-/g, '+').replace(/_/g, '/'))
    const claims = JSON.parse(json) as {
      sub: string
      role: Ruolo
      azienda_id: string
      exp: number
    }
    return {
      username: claims.sub,
      ruolo: claims.role,
      aziendaId: Number(claims.azienda_id),
      scadenza: claims.exp,
    }
  } catch {
    return null
  }
}

/** True se il token è scaduto (exp è in secondi, Date.now() in millisecondi). */
export function tokenScaduto(utente: UtenteAutenticato): boolean {
  return utente.scadenza * 1000 <= Date.now()
}
