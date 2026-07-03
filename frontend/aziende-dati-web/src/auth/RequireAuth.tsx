import { Navigate, Outlet } from 'react-router-dom'
import { useAuth } from './AuthContext'

// ROUTE GUARD: protegge le pagine riservate. Se non c'è un utente autenticato,
// reindirizza al login (replace = non lascia la pagina protetta nella cronologia,
// così il tasto "indietro" non ci riporta su una pagina che non possiamo vedere).
// Se invece l'utente c'è, mostra la rotta figlia tramite <Outlet />.
export function RequireAuth() {
  const { utente } = useAuth()

  if (!utente) {
    return <Navigate to="/login" replace />
  }

  return <Outlet />
}
