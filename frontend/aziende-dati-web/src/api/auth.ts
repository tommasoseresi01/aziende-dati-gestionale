import { api } from './client'
import type { TokenResponse } from '@/types/api'

// LOGIN via OAuth2 Client Credentials Flow (Fase 8): POST form-urlencoded a
// /connect/token. Il backend didattico non ha username/password: il "client"
// si autentica con client_id + client_secret e riceve un JWT.
// URLSearchParams produce il body application/x-www-form-urlencoded richiesto da OAuth2.
export async function login(clientId: string, clientSecret: string): Promise<string> {
  const body = new URLSearchParams({
    grant_type: 'client_credentials',
    client_id: clientId,
    client_secret: clientSecret,
  })

  const { data } = await api.post<TokenResponse>('/connect/token', body, {
    headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
  })

  return data.access_token
}
