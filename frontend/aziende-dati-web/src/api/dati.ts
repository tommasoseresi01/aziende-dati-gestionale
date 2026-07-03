import { api } from './client'
import type { DatoReadDto } from '@/types/api'

// I dati misurati si leggono per azienda: GET /api/aziende/{id}/dati.
// Il filtro per categoria lo faremo lato client sulla lista risultante.
export async function getDatiByAzienda(aziendaId: number): Promise<DatoReadDto[]> {
  const { data } = await api.get<DatoReadDto[]>(`/api/aziende/${aziendaId}/dati`)
  return data
}
