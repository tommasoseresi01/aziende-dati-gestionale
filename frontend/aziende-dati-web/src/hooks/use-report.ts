import { useQuery } from '@tanstack/react-query'
import { getSommaPerCategoria } from '@/api/report'

export function useSommaPerCategoria() {
  return useQuery({
    queryKey: ['report', 'somma-per-categoria'],
    queryFn: getSommaPerCategoria,
  })
}
