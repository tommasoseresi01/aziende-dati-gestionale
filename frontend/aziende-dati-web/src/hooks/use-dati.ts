import { useQuery } from '@tanstack/react-query'
import { getDatiByAzienda } from '@/api/dati'

// Dati di un'azienda. La query key include l'aziendaId: cambiando azienda cambia
// la key → TanStack Query rifà il fetch (e mantiene in cache i risultati precedenti).
// `enabled` evita di chiamare l'API finché non è stata scelta un'azienda.
export function useDatiByAzienda(aziendaId: number | null) {
  return useQuery({
    queryKey: ['dati', aziendaId],
    queryFn: () => getDatiByAzienda(aziendaId as number),
    enabled: aziendaId != null,
  })
}
