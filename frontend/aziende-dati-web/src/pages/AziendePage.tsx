import { useMemo, useState } from 'react'
import type { ColumnDef } from '@tanstack/react-table'
import { Pencil, Plus, Trash2 } from 'lucide-react'
import { toast } from 'sonner'
import { AziendaFormDialog } from '@/components/aziende/AziendaFormDialog'
import { ConfirmDialog } from '@/components/common/ConfirmDialog'
import { DataTable } from '@/components/common/DataTable'
import { ErrorState, LoadingState } from '@/components/common/PageState'
import { Button } from '@/components/ui/button'
import { useAuth } from '@/auth/AuthContext'
import { useAziende, useDeleteAzienda } from '@/hooks/use-aziende'
import { messaggioErrore } from '@/lib/errore'
import { formatData } from '@/lib/utils'
import type { AziendaReadDto } from '@/types/api'

export function AziendePage() {
  // isOwner (dal claim "role" del token) decide se mostrare le azioni di scrittura:
  // è lo SPECCHIO lato UI delle policy della Fase 8 (il reader vede solo in lettura).
  const { isOwner } = useAuth()
  const { data, isLoading, isError, error } = useAziende()
  const elimina = useDeleteAzienda()

  const [formAperto, setFormAperto] = useState(false)
  const [aziendaCorrente, setAziendaCorrente] = useState<AziendaReadDto | null>(null)
  const [daEliminare, setDaEliminare] = useState<AziendaReadDto | null>(null)

  const confermaEliminazione = async () => {
    if (!daEliminare) return
    try {
      await elimina.mutateAsync(daEliminare.id)
      toast.success('Azienda eliminata.')
      setDaEliminare(null)
    } catch (e) {
      toast.error(messaggioErrore(e))
    }
  }

  const columns = useMemo<ColumnDef<AziendaReadDto>[]>(() => {
    const base: ColumnDef<AziendaReadDto>[] = [
      { accessorKey: 'ragioneSociale', header: 'Ragione sociale' },
      { accessorKey: 'partitaIva', header: 'Partita IVA' },
      {
        accessorKey: 'dataRegistrazione',
        header: 'Registrata il',
        cell: ({ getValue }) => formatData(getValue<string>()),
      },
    ]
    if (isOwner) {
      base.push({
        id: 'azioni',
        header: '',
        enableSorting: false,
        cell: ({ row }) => (
          <div className="flex justify-end gap-1">
            <Button
              variant="ghost"
              size="icon"
              aria-label="Modifica"
              onClick={() => {
                setAziendaCorrente(row.original)
                setFormAperto(true)
              }}
            >
              <Pencil />
            </Button>
            <Button
              variant="ghost"
              size="icon"
              aria-label="Elimina"
              onClick={() => setDaEliminare(row.original)}
            >
              <Trash2 />
            </Button>
          </div>
        ),
      })
    }
    return base
  }, [isOwner])

  return (
    <div className="space-y-4">
      <div className="flex items-center justify-between gap-2">
        <div>
          <h1 className="text-2xl font-semibold">Aziende</h1>
          <p className="text-sm text-muted-foreground">Le aziende registrate nel gestionale.</p>
        </div>
        {isOwner && (
          <Button
            onClick={() => {
              setAziendaCorrente(null)
              setFormAperto(true)
            }}
          >
            <Plus /> Nuova azienda
          </Button>
        )}
      </div>

      {isLoading ? (
        <LoadingState />
      ) : isError ? (
        <ErrorState messaggio={messaggioErrore(error)} />
      ) : (
        <DataTable
          columns={columns}
          data={data ?? []}
          placeholderRicerca="Cerca per ragione sociale o P. IVA…"
          messaggioVuoto="Nessuna azienda registrata."
        />
      )}

      <AziendaFormDialog open={formAperto} onOpenChange={setFormAperto} azienda={aziendaCorrente} />
      <ConfirmDialog
        open={daEliminare !== null}
        onOpenChange={(aperto) => !aperto && setDaEliminare(null)}
        titolo="Eliminare l'azienda?"
        descrizione={`"${daEliminare?.ragioneSociale}" e i dati collegati verranno eliminati. L'operazione non è reversibile.`}
        onConferma={confermaEliminazione}
        inCorso={elimina.isPending}
      />
    </div>
  )
}
