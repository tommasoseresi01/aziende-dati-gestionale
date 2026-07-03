import { useMemo, useState } from 'react'
import type { ColumnDef } from '@tanstack/react-table'
import { DataTable } from '@/components/common/DataTable'
import { EmptyState, ErrorState, LoadingState } from '@/components/common/PageState'
import { Label } from '@/components/ui/label'
import { Select } from '@/components/ui/select'
import { useAziende } from '@/hooks/use-aziende'
import { useDatiByAzienda } from '@/hooks/use-dati'
import { messaggioErrore } from '@/lib/errore'
import { formatData, formatNumero } from '@/lib/utils'
import type { DatoReadDto } from '@/types/api'

// Pagina Dati (sola lettura per tutti i ruoli): si sceglie un'azienda e si vedono
// le sue misurazioni, con un ulteriore filtro per categoria applicato lato client.
export function DatiPage() {
  const aziende = useAziende()
  const [aziendaId, setAziendaId] = useState<number | null>(null)
  const [categoriaFiltro, setCategoriaFiltro] = useState('')
  const dati = useDatiByAzienda(aziendaId)

  // Categorie effettivamente presenti nei dati caricati → popolano il filtro.
  const categorieDisponibili = useMemo(() => {
    const nomi = new Set((dati.data ?? []).map((d) => d.categoriaNome))
    return Array.from(nomi).sort()
  }, [dati.data])

  const datiFiltrati = useMemo(() => {
    const lista = dati.data ?? []
    return categoriaFiltro ? lista.filter((d) => d.categoriaNome === categoriaFiltro) : lista
  }, [dati.data, categoriaFiltro])

  const columns = useMemo<ColumnDef<DatoReadDto>[]>(
    () => [
      { accessorKey: 'categoriaNome', header: 'Categoria' },
      {
        accessorKey: 'value',
        header: 'Valore',
        cell: ({ getValue }) => formatNumero(getValue<number>()),
      },
      {
        accessorKey: 'timestamp',
        header: 'Data/ora',
        cell: ({ getValue }) => formatData(getValue<string>()),
      },
    ],
    [],
  )

  return (
    <div className="space-y-4">
      <div>
        <h1 className="text-2xl font-semibold">Dati</h1>
        <p className="text-sm text-muted-foreground">
          Misurazioni per azienda, filtrabili per categoria.
        </p>
      </div>

      <div className="grid gap-3 sm:max-w-xl sm:grid-cols-2">
        <div className="grid gap-1.5">
          <Label htmlFor="filtro-azienda">Azienda</Label>
          <Select
            id="filtro-azienda"
            value={aziendaId === null ? '' : String(aziendaId)}
            onChange={(e) => {
              setAziendaId(e.target.value ? Number(e.target.value) : null)
              setCategoriaFiltro('')
            }}
          >
            <option value="">— Seleziona un&apos;azienda —</option>
            {(aziende.data ?? []).map((a) => (
              <option key={a.id} value={a.id}>
                {a.ragioneSociale}
              </option>
            ))}
          </Select>
        </div>

        <div className="grid gap-1.5">
          <Label htmlFor="filtro-categoria">Categoria</Label>
          <Select
            id="filtro-categoria"
            value={categoriaFiltro}
            onChange={(e) => setCategoriaFiltro(e.target.value)}
            disabled={aziendaId === null}
          >
            <option value="">Tutte</option>
            {categorieDisponibili.map((c) => (
              <option key={c} value={c}>
                {c}
              </option>
            ))}
          </Select>
        </div>
      </div>

      {aziendaId === null ? (
        <EmptyState
          titolo="Seleziona un'azienda"
          descrizione="Scegli un'azienda per vederne i dati misurati."
        />
      ) : dati.isLoading ? (
        <LoadingState />
      ) : dati.isError ? (
        <ErrorState messaggio={messaggioErrore(dati.error)} />
      ) : (
        <DataTable
          columns={columns}
          data={datiFiltrati}
          placeholderRicerca="Cerca nei dati…"
          messaggioVuoto="Nessun dato per i filtri scelti."
        />
      )}
    </div>
  )
}
