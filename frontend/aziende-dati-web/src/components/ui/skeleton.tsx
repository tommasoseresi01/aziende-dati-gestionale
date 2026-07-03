import type { ComponentProps } from 'react'
import { cn } from '@/lib/utils'

// Placeholder "pulsante" mostrato durante il caricamento (stato di loading).
export function Skeleton({ className, ...props }: ComponentProps<'div'>) {
  return <div className={cn('animate-pulse rounded-md bg-muted', className)} {...props} />
}
