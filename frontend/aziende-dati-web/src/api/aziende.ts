import { api } from './client'
import type { AziendaCreateDto, AziendaReadDto, AziendaUpdateDto } from '@/types/api'

// Funzioni "una per endpoint": isolano i dettagli HTTP dal resto dell'app.
// I componenti non conoscono URL né Axios: chiamano getAziende(), createAzienda(), ...

export async function getAziende(): Promise<AziendaReadDto[]> {
  const { data } = await api.get<AziendaReadDto[]>('/api/aziende')
  return data
}

export async function createAzienda(dto: AziendaCreateDto): Promise<AziendaReadDto> {
  const { data } = await api.post<AziendaReadDto>('/api/aziende', dto)
  return data
}

export async function updateAzienda(id: number, dto: AziendaUpdateDto): Promise<void> {
  await api.put(`/api/aziende/${id}`, dto)
}

export async function deleteAzienda(id: number): Promise<void> {
  await api.delete(`/api/aziende/${id}`)
}
