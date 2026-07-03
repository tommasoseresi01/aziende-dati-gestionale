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
import { useCreateAzienda, useUpdateAzienda } from '@/hooks/use-aziende'
import { messaggioErrore } from '@/lib/errore'
import type { AziendaReadDto } from '@/types/api'

// VALIDAZIONE LATO CLIENT con Zod = specchio delle regole del backend (Fase 6).
// Serve a dare feedback IMMEDIATO all'utente; il backend rivalida comunque (mai
// fidarsi solo del client). z.infer ricava il tipo TS dallo schema: una sola
// fonte di verità per regole e tipi.
const schema = z.object({
  ragioneSociale: z
    .string()
    .min(1, 'La ragione sociale è obbligatoria.')
    .max(100, 'Massimo 100 caratteri.'),
  partitaIva: z.string().regex(/^[0-9]{11}$/, 'La partita IVA deve essere di 11 cifre.'),
})
type FormValori = z.infer<typeof schema>

interface Props {
  open: boolean
  onOpenChange: (open: boolean) => void
  azienda: AziendaReadDto | null // null = creazione; valorizzato = modifica
}

export function AziendaFormDialog({ open, onOpenChange, azienda }: Props) {
  const creazione = useCreateAzienda()
  const modifica = useUpdateAzienda()

  const {
    register,
    handleSubmit,
    reset,
    formState: { errors, isSubmitting },
  } = useForm<FormValori>({
    resolver: zodResolver(schema),
    defaultValues: { ragioneSociale: '', partitaIva: '' },
  })

  // All'apertura: precompila i campi (modifica) o li svuota (creazione).
  useEffect(() => {
    if (open) {
      reset(
        azienda
          ? { ragioneSociale: azienda.ragioneSociale, partitaIva: azienda.partitaIva }
          : { ragioneSociale: '', partitaIva: '' },
      )
    }
  }, [open, azienda, reset])

  const onSubmit = async (valori: FormValori) => {
    try {
      if (azienda) {
        await modifica.mutateAsync({ id: azienda.id, dto: valori })
        toast.success('Azienda aggiornata.')
      } else {
        await creazione.mutateAsync(valori)
        toast.success('Azienda creata.')
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
            <DialogTitle>{azienda ? 'Modifica azienda' : 'Nuova azienda'}</DialogTitle>
            <DialogDescription>Compila i campi e salva.</DialogDescription>
          </DialogHeader>

          <div className="grid gap-2">
            <Label htmlFor="ragioneSociale">Ragione sociale</Label>
            <Input id="ragioneSociale" {...register('ragioneSociale')} />
            {errors.ragioneSociale && (
              <p className="text-sm text-destructive">{errors.ragioneSociale.message}</p>
            )}
          </div>

          <div className="grid gap-2">
            <Label htmlFor="partitaIva">Partita IVA</Label>
            <Input id="partitaIva" placeholder="11 cifre" {...register('partitaIva')} />
            {errors.partitaIva && (
              <p className="text-sm text-destructive">{errors.partitaIva.message}</p>
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
