import axios from 'axios'

// Estrae un messaggio LEGGIBILE da un errore di chiamata API. Sfrutta il formato
// ProblemDetails del backend (Fase 7): per i 400 di validazione legge la mappa
// "errors" (campo → messaggi), altrimenti "detail"/"title". È il ponte che rende
// utili all'utente gli errori strutturati prodotti dall'API.
export function messaggioErrore(errore: unknown): string {
  if (axios.isAxiosError(errore)) {
    const dati = errore.response?.data as
      | { title?: string; detail?: string; errors?: Record<string, string[]> }
      | undefined

    if (dati?.errors) {
      const messaggi = Object.values(dati.errors).flat()
      if (messaggi.length > 0) return messaggi.join(' ')
    }
    return dati?.detail ?? dati?.title ?? errore.message
  }

  if (errore instanceof Error) return errore.message
  return 'Errore sconosciuto.'
}
