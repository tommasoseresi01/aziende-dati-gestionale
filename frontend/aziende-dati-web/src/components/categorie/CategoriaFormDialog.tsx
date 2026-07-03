import { useEffect } from 'react'
import { useForm } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { z } from 'zod'
import { toast } from 'sonner'
import { Button } from '@/components/ui/button'
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from '@/components/ui/dialog'
import { Input } from '@/components/ui/input'
import { Label } from '@/components/ui/label'
import { Textarea } from '@/components/ui/textarea'
import { useCreateCategoria, useUpdateCategoria } from '@/hooks/use-categorie'
import { messaggioErrore } from '@/lib/errore'
import type { CategoriaReadDto } from '@/types/api'

const schema = z.object({
  nome: z.string().min(1, 'Il nome è obbligatorio.').max(50, 'Massimo 50 caratteri.'),
  descrizione: z.string().max(250, 'Massimo 250 caratteri.').optional(),
})
type FormValori = z.infer<typeof schema>

interface Props {
  open: boolean
  onOpenChange: (open: boolean) => void
  categoria: CategoriaReadDto | null
}

export function CategoriaFormDialog({ open, onOpenChange, categoria }: Props) {
  const creazione = useCreateCategoria()
  const modifica = useUpdateCategoria()

  const {
    register,
    handleSubmit,
    reset,
    formState: { errors, isSubmitting },
  } = useForm<FormValori>({
    resolver: zodResolver(schema),
    defaultValues: { nome: '', descrizione: '' },
  })

  useEffect(() => {
    if (open) {
      reset(
        categoria
          ? { nome: categoria.nome, descrizione: categoria.descrizione ?? '' }
          : { nome: '', descrizione: '' },
      )
    }
  }, [open, categoria, reset])

  const onSubmit = async (valori: FormValori) => {
    // Descrizione vuota → inviamo null (campo opzionale lato backend).
    const dto = { nome: valori.nome, descrizione: valori.descrizione?.trim() || null }
    try {
      if (categoria) {
        await modifica.mutateAsync({ id: categoria.id, dto })
        toast.success('Categoria aggiornata.')
      } else {
        await creazione.mutateAsync(dto)
        toast.success('Categoria creata.')
      }
      onOpenChange(false)
    } catch (e) {
      toast.error(messaggioErrore(e))
    }
  }

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent>
        <form onSubmit={handleSubmit(onSubmit)} className="grid gap-4">
          <DialogHeader>
            <DialogTitle>{categoria ? 'Modifica categoria' : 'Nuova categoria'}</DialogTitle>
            <DialogDescription>Compila i campi e salva.</DialogDescription>
          </DialogHeader>

          <div className="grid gap-2">
            <Label htmlFor="nome">Nome</Label>
            <Input id="nome" {...register('nome')} />
            {errors.nome && <p className="text-sm text-destructive">{errors.nome.message}</p>}
          </div>

          <div className="grid gap-2">
            <Label htmlFor="descrizione">Descrizione (opzionale)</Label>
            <Textarea id="descrizione" {...register('descrizione')} />
            {errors.descrizione && (
              <p className="text-sm text-destructive">{errors.descrizione.message}</p>
            )}
          </div>

          <DialogFooter>
            <Button type="button" variant="outline" onClick={() => onOpenChange(false)}>
              Annulla
            </Button>
            <Button type="submit" disabled={isSubmitting}>
              {isSubmitting ? 'Salvo…' : 'Salva'}
            </Button>
          </DialogFooter>
        </form>
      </DialogContent>
    </Dialog>
  )
}
