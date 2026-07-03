import {
  createContext,
  useCallback,
  useContext,
  useEffect,
  useMemo,
  useState,
  type ReactNode,
} from 'react'
import { login as loginApi } from '@/api/auth'
import { clearToken, getToken, setToken } from '@/auth/token-store'
import { decodeJwt, tokenScaduto, type UtenteAutenticato } from '@/auth/jwt'

interface AuthContextValue {
  utente: UtenteAutenticato | null
  isOwner: boolean
  login: (clientId: string, clientSecret: string) => Promise<void>
  logout: () => void
}

const AuthContext = createContext<AuthContextValue | null>(null)

// Provider dello stato di autenticazione: mette a disposizione utente + login/logout
// a tutta l'app tramite React Context (niente "prop drilling").
export function AuthProvider({ children }: { children: ReactNode }) {
  // Stato iniziale: se in sessionStorage c'è un token valido e non scaduto,
  // ripristiniamo la sessione (l'utente resta loggato dopo un refresh).
  const [utente, setUtente] = useState<UtenteAutenticato | null>(() => {
    const token = getToken()
    if (!token) return null
    const decodificato = decodeJwt(token)
    if (!decodificato || tokenScaduto(decodificato)) {
      clearToken()
      return null
    }
    return decodificato
  })

  const logout = useCallback(() => {
    clearToken()
    setUtente(null)
  }, [])

  const login = useCallback(async (clientId: string, clientSecret: string) => {
    const token = await loginApi(clientId, clientSecret)
    setToken(token)
    const decodificato = decodeJwt(token)
    if (!decodificato) {
      throw new Error('Il token ricevuto non è valido.')
    }
    setUtente(decodificato)
  }, [])

  // L'interceptor Axios emette 'auth:logout' su un 401 (token scaduto lato server):
  // qui reagiamo sganciando l'utente → il route guard rimanda al login.
  useEffect(() => {
    const onLogout = () => setUtente(null)
    window.addEventListener('auth:logout', onLogout)
    return () => window.removeEventListener('auth:logout', onLogout)
  }, [])

  const value = useMemo<AuthContextValue>(
    () => ({
      utente,
      // isOwner guida la visibilità delle azioni di scrittura (specchio delle
      // policy della Fase 8): l'owner vede crea/modifica/elimina, il reader no.
      isOwner: utente?.ruolo === 'data.company.owner',
      login,
      logout,
    }),
    [utente, login, logout],
  )

  // React 19: si può usare il Context stesso come provider (<Context value=...>).
  return <AuthContext value={value}>{children}</AuthContext>
}

// Hook di comodo: legge il contesto e fallisce chiaramente se usato fuori dal Provider.
// eslint-disable-next-line react-refresh/only-export-components
export function useAuth(): AuthContextValue {
  const ctx = useContext(AuthContext)
  if (!ctx) {
    throw new Error('useAuth deve essere usato dentro <AuthProvider>.')
  }
  return ctx
}
