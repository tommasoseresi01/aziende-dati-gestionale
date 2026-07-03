import axios, { type AxiosError } from 'axios'
import { clearToken, getToken } from '@/auth/token-store'

// Istanza Axios centralizzata: UNA sola configurazione (baseURL + interceptor)
// per TUTTE le chiamate. baseURL arriva dall'.env (VITE_API_URL).
export const api = axios.create({
  baseURL: import.meta.env.VITE_API_URL,
})

// INTERCEPTOR DI RICHIESTA — il cuore dell'autenticazione lato client.
// Prima di OGNI chiamata, se c'è un token lo allega come header
// "Authorization: Bearer <jwt>". Così i componenti non ci pensano: chiamano
// l'API e basta, e il token viaggia in automatico.
api.interceptors.request.use((config) => {
  const token = getToken()
  if (token) {
    config.headers.Authorization = `Bearer ${token}`
  }
  return config
})

// INTERCEPTOR DI RISPOSTA — gestione centralizzata del 401.
// Se il server risponde 401 (token mancante o scaduto), cancelliamo il token e
// avvisiamo l'app con un evento: il contesto Auth farà il logout e il route guard
// rimanderà al login. Escludiamo l'endpoint del token (un login sbagliato dà 401
// ma NON deve innescare il logout automatico: sei già fuori).
api.interceptors.response.use(
  (response) => response,
  (error: AxiosError) => {
    const status = error.response?.status
    const url = error.config?.url ?? ''
    if (status === 401 && !url.includes('/connect/token')) {
      clearToken()
      window.dispatchEvent(new Event('auth:logout'))
    }
    return Promise.reject(error)
  },
)
