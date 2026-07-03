import { StrictMode } from 'react'
import { createRoot } from 'react-dom/client'
import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { BrowserRouter } from 'react-router-dom'
import './index.css'
import App from './App'
import { AuthProvider } from '@/auth/AuthContext'
import { ThemeProvider } from '@/components/theme/ThemeProvider'
import { Toaster } from '@/components/ui/sonner'

// L'ORDINE dei Provider conta (dall'esterno all'interno):
//  - ThemeProvider: tema chiaro/scuro (lo usa anche il Toaster).
//  - QueryClientProvider: la cache di TanStack Query per tutte le chiamate API.
//  - BrowserRouter: la navigazione (URL ↔ pagine).
//  - AuthProvider: stato di login, disponibile a rotte e componenti.
const queryClient = new QueryClient({
  defaultOptions: {
    queries: {
      retry: 1, // un solo tentativo di ripetizione in caso di errore
      refetchOnWindowFocus: false, // niente refetch tornando sulla tab (didattico)
    },
  },
})

createRoot(document.getElementById('root')!).render(
  <StrictMode>
    <ThemeProvider>
      <QueryClientProvider client={queryClient}>
        <BrowserRouter>
          <AuthProvider>
            <App />
            <Toaster />
          </AuthProvider>
        </BrowserRouter>
      </QueryClientProvider>
    </ThemeProvider>
  </StrictMode>,
)
