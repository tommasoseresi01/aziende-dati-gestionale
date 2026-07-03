import { clsx, type ClassValue } from 'clsx'
import { twMerge } from 'tailwind-merge'

// cn() = "class names": unisce classi Tailwind condizionali (clsx) e RISOLVE i
// conflitti (tailwind-merge). Es. cn('p-2', condizione && 'p-4') → 'p-4' vince.
// È l'helper standard dei componenti in stile shadcn/ui.
export function cn(...inputs: ClassValue[]) {
  return twMerge(clsx(inputs))
}

// Formatta un decimale come valuta/numero italiano (per tabelle e grafici).
export function formatNumero(valore: number): string {
  return new Intl.NumberFormat('it-IT', { maximumFractionDigits: 2 }).format(valore)
}

// Formatta una data ISO (dal backend) in formato leggibile italiano.
export function formatData(iso: string): string {
  return new Date(iso).toLocaleDateString('it-IT', {
    day: '2-digit',
    month: '2-digit',
    year: 'numeric',
  })
}
