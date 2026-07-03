import { Building2, Sigma, Tags } from 'lucide-react'
import type { LucideIcon } from 'lucide-react'
import {
  Bar,
  BarChart,
  CartesianGrid,
  ResponsiveContainer,
  Tooltip,
  XAxis,
  YAxis,
} from 'recharts'
import {
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
} from '@/components/ui/card'
import { EmptyState, ErrorState, LoadingState } from '@/components/common/PageState'
import { useAziende } from '@/hooks/use-aziende'
import { useCategorie } from '@/hooks/use-categorie'
import { useSommaPerCategoria } from '@/hooks/use-report'
import { messaggioErrore } from '@/lib/errore'
import { formatNumero } from '@/lib/utils'

// Card riassuntiva (numero grande + icona).
function StatCard({
  icona: Icona,
  etichetta,
  valore,
}: {
  icona: LucideIcon
  etichetta: string
  valore: string
}) {
  return (
    <Card>
      <CardHeader className="flex-row items-center justify-between pb-2">
        <CardDescription>{etichetta}</CardDescription>
        <Icona className="size-5 text-muted-foreground" />
      </CardHeader>
      <CardContent>
        <div className="text-3xl font-semibold">{valore}</div>
      </CardContent>
    </Card>
  )
}

export function DashboardPage() {
  const aziende = useAziende()
  const categorie = useCategorie()
  const report = useSommaPerCategoria()

  const sommaTotale = (report.data ?? []).reduce((acc, riga) => acc + riga.somma, 0)

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-2xl font-semibold">Dashboard</h1>
        <p className="text-sm text-muted-foreground">Panoramica del gestionale.</p>
      </div>

      <div className="grid gap-4 sm:grid-cols-3">
        <StatCard
          icona={Building2}
          etichetta="Aziende"
          valore={aziende.isLoading ? '—' : String(aziende.data?.length ?? 0)}
        />
        <StatCard
          icona={Tags}
          etichetta="Categorie"
          valore={categorie.isLoading ? '—' : String(categorie.data?.length ?? 0)}
        />
        <StatCard
          icona={Sigma}
          etichetta="Somma valori"
          valore={report.isLoading ? '—' : formatNumero(sommaTotale)}
        />
      </div>

      <Card>
        <CardHeader>
          <CardTitle>Somma dei valori per categoria</CardTitle>
          <CardDescription>
            Report aggregato dal backend (GET /api/report/somma-per-categoria, Fase 4).
          </CardDescription>
        </CardHeader>
        <CardContent>
          {report.isLoading ? (
            <LoadingState righe={4} />
          ) : report.isError ? (
            <ErrorState messaggio={messaggioErrore(report.error)} />
          ) : (report.data?.length ?? 0) === 0 ? (
            <EmptyState
              titolo="Nessun dato"
              descrizione="Non ci sono ancora misurazioni da aggregare."
            />
          ) : (
            <ResponsiveContainer width="100%" height={320}>
              <BarChart data={report.data}>
                <CartesianGrid strokeDasharray="3 3" stroke="var(--color-border)" />
                <XAxis dataKey="categoria" tick={{ fontSize: 12 }} stroke="var(--color-muted-foreground)" />
                <YAxis tick={{ fontSize: 12 }} stroke="var(--color-muted-foreground)" />
                <Tooltip
                  cursor={{ fill: 'var(--color-muted)' }}
                  contentStyle={{
                    background: 'var(--color-popover)',
                    border: '1px solid var(--color-border)',
                    borderRadius: 8,
                    color: 'var(--color-popover-foreground)',
                  }}
                />
                <Bar dataKey="somma" fill="var(--color-chart-1)" radius={[4, 4, 0, 0]} />
              </BarChart>
            </ResponsiveContainer>
          )}
        </CardContent>
      </Card>
    </div>
  )
}
