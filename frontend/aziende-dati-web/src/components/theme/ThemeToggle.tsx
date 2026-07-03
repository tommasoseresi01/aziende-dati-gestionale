import { Moon, Sun } from 'lucide-react'
import { Button } from '@/components/ui/button'
import { useTheme } from './ThemeProvider'

export function ThemeToggle() {
  const { theme, toggle } = useTheme()
  return (
    <Button variant="ghost" size="icon" onClick={toggle} aria-label="Cambia tema chiaro/scuro">
      {theme === 'dark' ? <Sun /> : <Moon />}
    </Button>
  )
}
