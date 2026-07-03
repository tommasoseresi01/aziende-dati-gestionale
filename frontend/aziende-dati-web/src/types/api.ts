// Interfacce TypeScript che RISPECCHIANO i DTO del backend .NET.
// PERCHÉ tipizzare le risposte dell'API: il compilatore ci avvisa se usiamo un
// campo che non esiste o con il tipo sbagliato (es. `azienda.ragionesociale` con
// la S minuscola) → meno bug, autocompletamento in tutto il codice.
// NOTA: il JSON del backend è in camelCase (System.Text.Json), quindi qui i nomi
// sono in camelCase anche se nel C# erano PascalCase.

export interface AziendaReadDto {
  id: number
  ragioneSociale: string
  partitaIva: string
  dataRegistrazione: string // le date arrivano come stringa ISO
}

export interface AziendaCreateDto {
  ragioneSociale: string
  partitaIva: string
}

export type AziendaUpdateDto = AziendaCreateDto

export interface CategoriaReadDto {
  id: number
  nome: string
  descrizione?: string | null
}

export interface CategoriaCreateDto {
  nome: string
  descrizione?: string | null
}

export type CategoriaUpdateDto = CategoriaCreateDto

export interface DatoReadDto {
  id: number
  value: number
  timestamp: string
  categoriaId: number
  categoriaNome: string
}

export interface SommaPerCategoriaDto {
  categoriaId: number
  categoria: string
  somma: number
}

/** Risposta standard OAuth2 dell'endpoint /connect/token. */
export interface TokenResponse {
  access_token: string
  token_type: string
  expires_in: number
}

/** I due ruoli applicativi (valori del claim "role" nel JWT). */
export type Ruolo = 'data.company.owner' | 'data.company.reader'
