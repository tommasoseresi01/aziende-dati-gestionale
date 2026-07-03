import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { createCategoria, deleteCategoria, getCategorie, updateCategoria } from '@/api/categorie'
import type { CategoriaCreateDto, CategoriaUpdateDto } from '@/types/api'

const KEY = ['categorie'] as const

export function useCategorie() {
  return useQuery({ queryKey: KEY, queryFn: getCategorie })
}

export function useCreateCategoria() {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: (dto: CategoriaCreateDto) => createCategoria(dto),
    onSuccess: () => qc.invalidateQueries({ queryKey: KEY }),
  })
}

export function useUpdateCategoria() {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: ({ id, dto }: { id: number; dto: CategoriaUpdateDto }) => updateCategoria(id, dto),
    onSuccess: () => qc.invalidateQueries({ queryKey: KEY }),
  })
}

export function useDeleteCategoria() {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: (id: number) => deleteCategoria(id),
    onSuccess: () => qc.invalidateQueries({ queryKey: KEY }),
  })
}
