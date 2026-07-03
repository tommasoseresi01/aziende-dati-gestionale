import { Toaster as SonnerToaster } from 'sonner'
import { useTheme } from '@/components/theme/ThemeProvider'

// "Toast" = notifiche brevi in un angolo (successo/errore delle operazioni).
// Passiamo il tema così i toast seguono il chiaro/scuro dell'app.
export function Toaster() {
  const { theme } = useTheme()
  return <SonnerToaster theme={theme} richColors position="top-right" />
}
