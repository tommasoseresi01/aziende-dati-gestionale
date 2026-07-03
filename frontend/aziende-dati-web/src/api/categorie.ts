import { api } from './client'
import type { CategoriaCreateDto, CategoriaReadDto, CategoriaUpdateDto } from '@/types/api'

export async function getCategorie(): Promise<CategoriaReadDto[]> {
  const { data } = await api.get<CategoriaReadDto[]>('/api/categorie')
  return data
}

export async function createCategoria(dto: CategoriaCreateDto): Promise<CategoriaReadDto> {
  const { data } = await api.post<CategoriaReadDto>('/api/categorie', dto)
  return data
}

export async function updateCategoria(id: number, dto: CategoriaUpdateDto): Promise<void> {
  await api.put(`/api/categorie/${id}`, dto)
}

export async function deleteCategoria(id: number): Promise<void> {
  await api.delete(`/api/categorie/${id}`)
}
