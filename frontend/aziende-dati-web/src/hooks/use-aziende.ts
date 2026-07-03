import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { createAzienda, deleteAzienda, getAziende, updateAzienda } from '@/api/aziende'
import type { AziendaCreateDto, AziendaUpdateDto } from '@/types/api'

// TANSTACK QUERY: separa i dati del SERVER dallo stato locale. useQuery gestisce
// da solo fetch, CACHE, e gli stati isLoading/isError/data — niente useEffect a
// mano. La "queryKey" identifica la cache: componenti diversi con la stessa key
// condividono gli stessi dati.
const KEY = ['aziende'] as const

export function useAziende() {
  return useQuery({ queryKey: KEY, queryFn: getAziende })
}

// useMutation: per le operazioni che CAMBIANO i dati. Su onSuccess invalidiamo la
// cache 'aziende' → la lista si ricarica automaticamente (nessun refresh manuale).
export function useCreateAzienda() {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: (dto: AziendaCreateDto) => createAzienda(dto),
    onSuccess: () => qc.invalidateQueries({ queryKey: KEY }),
  })
}

export function useUpdateAzienda() {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: ({ id, dto }: { id: number; dto: AziendaUpdateDto }) => updateAzienda(id, dto),
    onSuccess: () => qc.invalidateQueries({ queryKey: KEY }),
  })
}

export function useDeleteAzienda() {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: (id: number) => deleteAzienda(id),
    onSuccess: () => qc.invalidateQueries({ queryKey: KEY }),
  })
}
