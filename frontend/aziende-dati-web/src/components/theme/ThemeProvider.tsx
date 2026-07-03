import { createContext, useContext, useEffect, useState, type ReactNode } from 'react'

type Theme = 'light' | 'dark'

interface ThemeContextValue {
  theme: Theme
  toggle: () => void
}

const ThemeContext = createContext<ThemeContextValue | null>(null)
const STORAGE_KEY = 'aziendedati.theme'

function temaIniziale(): Theme {
  const salvato = localStorage.getItem(STORAGE_KEY)
  if (salvato === 'light' || salvato === 'dark') return salvato
  // Nessuna preferenza salvata → seguiamo il tema del sistema operativo.
  return window.matchMedia('(prefers-color-scheme: dark)').matches ? 'dark' : 'light'
}

// Gestisce il tema chiaro/scuro: applica/rimuove la classe .dark su <html> (che
// attiva le variabili CSS scure di index.css) e ricorda la scelta in localStorage.
export function ThemeProvider({ children }: { children: ReactNode }) {
  const [theme, setTheme] = useState<Theme>(temaIniziale)

  useEffect(() => {
    document.documentElement.classList.toggle('dark', theme === 'dark')
    localStorage.setItem(STORAGE_KEY, theme)
  }, [theme])

  const toggle = () => setTheme((corrente) => (corrente === 'dark' ? 'light' : 'dark'))

  return <ThemeContext value={{ theme, toggle }}>{children}</ThemeContext>
}

// eslint-disable-next-line react-refresh/only-export-components
export function useTheme(): ThemeContextValue {
  const ctx = useContext(ThemeContext)
  if (!ctx) {
    throw new Error('useTheme deve essere usato dentro <ThemeProvider>.')
  }
  return ctx
}
