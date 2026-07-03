import { useMemo, useState } from 'react'
import type { ColumnDef } from '@tanstack/react-table'
import { Pencil, Plus, Trash2 } from 'lucide-react'
import { toast } from 'sonner'
import { CategoriaFormDialog } from '@/components/categorie/CategoriaFormDialog'
import { ConfirmDialog } from '@/components/common/ConfirmDialog'
import { DataTable } from '@/components/common/DataTable'
import { ErrorState, LoadingState } from '@/components/common/PageState'
import { Button } from '@/components/ui/button'
import { useAuth } from '@/auth/AuthContext'
import { useCategorie, useDeleteCategoria } from '@/hooks/use-categorie'
import { messaggioErrore } from '@/lib/errore'
import type { CategoriaReadDto } from '@/types/api'

export function CategoriePage() {
  const { isOwner } = useAuth()
  const { data, isLoading, isError, error } = useCategorie()
  const elimina = useDeleteCategoria()

  const [formAperto, setFormAperto] = useState(false)
  const [categoriaCorrente, setCategoriaCorrente] = useState<CategoriaReadDto | null>(null)
  const [daEliminare, setDaEliminare] = useState<CategoriaReadDto | null>(null)

  const confermaEliminazione = async () => {
    if (!daEliminare) return
    try {
      await elimina.mutateAsync(daEliminare.id)
      toast.success('Categoria eliminata.')
      setDaEliminare(null)
    } catch (e) {
      toast.error(messaggioErrore(e))
    }
  }

  const columns = useMemo<ColumnDef<CategoriaReadDto>[]>(() => {
    const base: ColumnDef<CategoriaReadDto>[] = [
      { accessorKey: 'nome', header: 'Nome' },
      {
        accessorKey: 'descrizione',
        header: 'Descrizione',
        cell: ({ getValue }) => getValue<string | null>() ?? '—',
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
                setCategoriaCorrente(row.original)
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
          <h1 className="text-2xl font-semibold">Categorie</h1>
          <p className="text-sm text-muted-foreground">Le categorie con cui si classificano i dati.</p>
        </div>
        {isOwner && (
          <Button
            onClick={() => {
              setCategoriaCorrente(null)
              setFormAperto(true)
            }}
          >
            <Plus /> Nuova categoria
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
          placeholderRicerca="Cerca per nome…"
          messaggioVuoto="Nessuna categoria."
        />
      )}

      <CategoriaFormDialog
        open={formAperto}
        onOpenChange={setFormAperto}
        categoria={categoriaCorrente}
      />
      <ConfirmDialog
        open={daEliminare !== null}
        onOpenChange={(aperto) => !aperto && setDaEliminare(null)}
        titolo="Eliminare la categoria?"
        descrizione={`"${daEliminare?.nome}" verrà eliminata. Se è usata da dei dati, il backend rifiuterà l'operazione (409).`}
        onConferma={confermaEliminazione}
        inCorso={elimina.isPending}
      />
    </div>
  )
}
