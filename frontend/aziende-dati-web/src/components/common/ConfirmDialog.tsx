import { Button } from '@/components/ui/button'
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from '@/components/ui/dialog'

interface ConfirmDialogProps {
  open: boolean
  onOpenChange: (open: boolean) => void
  titolo: string
  descrizione: string
  onConferma: () => void
  inCorso?: boolean
}

// Dialog di conferma per le azioni distruttive (eliminazione): meglio un clic in
// più che una cancellazione accidentale.
export function ConfirmDialog({
  open,
  onOpenChange,
  titolo,
  descrizione,
  onConferma,
  inCorso = false,
}: ConfirmDialogProps) {
  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent>
        <DialogHeader>
          <DialogTitle>{titolo}</DialogTitle>
          <DialogDescription>{descrizione}</DialogDescription>
        </DialogHeader>
        <DialogFooter>
          <Button variant="outline" onClick={() => onOpenChange(false)} disabled={inCorso}>
            Annulla
          </Button>
          <Button variant="destructive" onClick={onConferma} disabled={inCorso}>
            {inCorso ? 'Elimino…' : 'Elimina'}
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  )
}
