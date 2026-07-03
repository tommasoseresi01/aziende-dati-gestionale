import { AlertCircle, Inbox } from 'lucide-react'
import type { ReactNode } from 'react'
import { Skeleton } from '@/components/ui/skeleton'

// Tre stati che OGNI schermata guidata da dati deve gestire (requisito della
// specifica): CARICAMENTO, ERRORE, VUOTO. Averli come componenti riutilizzabili
// evita di reinventarli in ogni pagina e dà un'esperienza coerente.

/** Stato di CARICAMENTO: righe "scheletro" al posto dei dati non ancora arrivati. */
export function LoadingState({ righe = 5 }: { righe?: number }) {
  return (
    <div className="space-y-2">
      {Array.from({ length: righe }).map((_, i) => (
        <Skeleton key={i} className="h-11 w-full" />
      ))}
    </div>
  )
}

/** Stato di ERRORE: mostra un messaggio utile (dal backend, ProblemDetails Fase 7). */
export function ErrorState({ messaggio }: { messaggio?: string }) {
  return (
    <div className="flex flex-col items-center justify-center gap-2 rounded-lg border border-destructive/30 bg-destructive/5 p-8 text-center">
      <AlertCircle className="size-8 text-destructive" />
      <p className="font-medium">Si è verificato un errore</p>
      <p className="text-sm text-muted-foreground">
        {messaggio ?? 'Riprova più tardi o controlla che la API sia in esecuzione.'}
      </p>
    </div>
  )
}

/** Stato VUOTO: nessun dato da mostrare (con eventuale azione, es. "Crea"). */
export function EmptyState({
  titolo,
  descrizione,
  azione,
}: {
  titolo: string
  descrizione?: string
  azione?: ReactNode
}) {
  return (
    <div className="flex flex-col items-center justify-center gap-2 rounded-lg border border-dashed border-border p-10 text-center">
      <Inbox className="size-8 text-muted-foreground" />
      <p className="font-medium">{titolo}</p>
      {descrizione && <p className="text-sm text-muted-foreground">{descrizione}</p>}
      {azione && <div className="mt-2">{azione}</div>}
    </div>
  )
}
