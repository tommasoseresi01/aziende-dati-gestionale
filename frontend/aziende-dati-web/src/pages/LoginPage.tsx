import { useState, type FormEvent } from 'react'
import { Navigate, useNavigate } from 'react-router-dom'
import { BarChart3 } from 'lucide-react'
import { Button } from '@/components/ui/button'
import {
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
} from '@/components/ui/card'
import { Input } from '@/components/ui/input'
import { Label } from '@/components/ui/label'
import { useAuth } from '@/auth/AuthContext'
import { messaggioErrore } from '@/lib/errore'

export function LoginPage() {
  const { utente, login } = useAuth()
  const navigate = useNavigate()

  const [clientId, setClientId] = useState('acme-owner-client')
  const [clientSecret, setClientSecret] = useState('')
  const [errore, setErrore] = useState<string | null>(null)
  const [inCorso, setInCorso] = useState(false)

  // Se già autenticato, non ha senso mostrare il login: si va alla dashboard.
  if (utente) {
    return <Navigate to="/" replace />
  }

  const onSubmit = async (e: FormEvent) => {
    e.preventDefault()
    setErrore(null)
    setInCorso(true)
    try {
      await login(clientId, clientSecret)
      navigate('/', { replace: true })
    } catch (err) {
      setErrore(messaggioErrore(err))
    } finally {
      setInCorso(false)
    }
  }

  return (
    <div className="flex min-h-screen items-center justify-center p-4">
      <Card className="w-full max-w-sm">
        <CardHeader className="items-center text-center">
          <BarChart3 className="size-8 text-primary" />
          <CardTitle className="text-xl">AziendeDati</CardTitle>
          <CardDescription>
            Accedi con le credenziali client (OAuth2 Client Credentials, Fase 8).
          </CardDescription>
        </CardHeader>
        <CardContent>
          <form onSubmit={onSubmit} className="grid gap-4">
            <div className="grid gap-2">
              <Label htmlFor="clientId">Client ID</Label>
              <Input
                id="clientId"
                value={clientId}
                onChange={(e) => setClientId(e.target.value)}
                autoComplete="username"
              />
            </div>
            <div className="grid gap-2">
              <Label htmlFor="clientSecret">Client Secret</Label>
              <Input
                id="clientSecret"
                type="password"
                value={clientSecret}
                onChange={(e) => setClientSecret(e.target.value)}
                autoComplete="current-password"
              />
            </div>
            {errore && <p className="text-sm text-destructive">{errore}</p>}
            <Button type="submit" disabled={inCorso}>
              {inCorso ? 'Accesso…' : 'Accedi'}
            </Button>
          </form>

          <div className="mt-4 rounded-md bg-muted p-3 text-xs text-muted-foreground">
            <p className="mb-1 font-medium text-foreground">Credenziali demo (dati di seed):</p>
            <p>
              owner → <code>acme-owner-client</code> / <code>secret-owner-acme-2025</code>
            </p>
            <p>
              reader → <code>acme-reader-client</code> / <code>secret-reader-acme-2025</code>
            </p>
          </div>
        </CardContent>
      </Card>
    </div>
  )
}
