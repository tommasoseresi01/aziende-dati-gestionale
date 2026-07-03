import type { ComponentProps } from 'react'
import { cn } from '@/lib/utils'

// Select NATIVO stilizzato (usato per i filtri della pagina Dati). Nativo = zero
// dipendenze in più e pienamente accessibile; per menu più ricchi si userebbe
// @radix-ui/react-select.
export function Select({ className, ...props }: ComponentProps<'select'>) {
  return (
    <select
      className={cn(
        'flex h-9 w-full rounded-md border border-input bg-background px-3 py-1 text-sm text-foreground shadow-sm focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring disabled:cursor-not-allowed disabled:opacity-50',
        className,
      )}
      {...props}
    />
  )
}
