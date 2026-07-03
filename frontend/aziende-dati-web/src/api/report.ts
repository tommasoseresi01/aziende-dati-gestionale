import { api } from './client'
import type { SommaPerCategoriaDto } from '@/types/api'

// Il report della Fase 4: somma dei valori per categoria (ordinata decrescente).
export async function getSommaPerCategoria(): Promise<SommaPerCategoriaDto[]> {
  const { data } = await api.get<SommaPerCategoriaDto[]>('/api/report/somma-per-categoria')
  return data
}
