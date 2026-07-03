import { useState } from 'react'
import { NavLink, Outlet } from 'react-router-dom'
import {
  BarChart3,
  Building2,
  Database,
  LayoutDashboard,
  LogOut,
  Menu,
  Tags,
  X,
} from 'lucide-react'
import { Badge } from '@/components/ui/badge'
import { Button } from '@/components/ui/button'
import { ThemeToggle } from '@/components/theme/ThemeToggle'
import { useAuth } from '@/auth/AuthContext'
import { cn } from '@/lib/utils'

// Voci di navigazione della sidebar. `end` (solo per la Dashboard "/") evita che
// resti evidenziata quando siamo su una sottorotta.
const voci = [
  { to: '/', label: 'Dashboard', icon: LayoutDashboard, end: true },
  { to: '/aziende', label: 'Aziende', icon: Building2, end: false },
  { to: '/categorie', label: 'Categorie', icon: Tags, end: false },
  { to: '/dati', label: 'Dati', icon: Database, end: false },
]

// Layout applicativo: sidebar di navigazione + header (utente, tema, logout).
// È responsive: su schermi piccoli la sidebar diventa un menu a scomparsa.
export function AppLayout() {
  const { utente, isOwner, logout } = useAuth()
  const [menuAperto, setMenuAperto] = useState(false)

  const navigazione = (
    <nav className="flex flex-col gap-1 p-3">
      {voci.map(({ to, label, icon: Icon, end }) => (
        <NavLink
          key={to}
          to={to}
          end={end}
          onClick={() => setMenuAperto(false)}
          className={({ isActive }) =>
            cn(
              'flex items-center gap-3 rounded-md px-3 py-2 text-sm font-medium transition-colors',
              isActive
                ? 'bg-primary text-primary-foreground'
                : 'text-muted-foreground hover:bg-accent hover:text-accent-foreground',
            )
          }
        >
          <Icon className="size-4" />
          {label}
        </NavLink>
      ))}
    </nav>
  )

  return (
    <div className="min-h-screen">
      {/* Sidebar fissa su desktop (md+) */}
      <aside className="fixed inset-y-0 left-0 hidden w-60 border-r border-border bg-card md:block">
        <div className="flex h-14 items-center gap-2 border-b border-border px-5 font-semibold">
          <BarChart3 className="size-5 text-primary" /> AziendeDati
        </div>
        {navigazione}
      </aside>

      {/* Sidebar a scomparsa su mobile */}
      {menuAperto && (
        <div className="fixed inset-0 z-40 md:hidden">
          <div
            className="absolute inset-0 bg-black/50"
            onClick={() => setMenuAperto(false)}
            aria-hidden
          />
          <aside className="absolute inset-y-0 left-0 w-60 border-r border-border bg-card">
            <div className="flex h-14 items-center justify-between border-b border-border px-5 font-semibold">
              <span className="flex items-center gap-2">
                <BarChart3 className="size-5 text-primary" /> AziendeDati
              </span>
              <Button variant="ghost" size="icon" onClick={() => setMenuAperto(false)}>
                <X />
              </Button>
            </div>
            {navigazione}
          </aside>
        </div>
      )}

      {/* Contenuto: lascia spazio alla sidebar su desktop */}
      <div className="md:pl-60">
        <header className="sticky top-0 z-30 flex h-14 items-center gap-3 border-b border-border bg-background/95 px-4 backdrop-blur">
          <Button
            variant="ghost"
            size="icon"
            className="md:hidden"
            onClick={() => setMenuAperto(true)}
            aria-label="Apri il menu"
          >
            <Menu />
          </Button>

          <div className="ml-auto flex items-center gap-3">
            <div className="hidden text-right sm:block">
              <div className="text-sm font-medium leading-tight">{utente?.username}</div>
              <Badge variant={isOwner ? 'default' : 'secondary'} className="mt-0.5">
                {isOwner ? 'owner' : 'reader'}
              </Badge>
            </div>
            <ThemeToggle />
            <Button variant="outline" size="sm" onClick={logout}>
              <LogOut className="size-4" />
              <span className="hidden sm:inline">Esci</span>
            </Button>
          </div>
        </header>

        <main className="p-4 md:p-6">
          <Outlet />
        </main>
      </div>
    </div>
  )
}
