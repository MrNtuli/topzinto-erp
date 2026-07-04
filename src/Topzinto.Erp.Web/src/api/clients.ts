import { apiFetch } from './client'

export interface Client {
  id: string
  name: string
  type: string
  city: string | null
  province: string | null
  projectCount: number
  primaryContact: string | null
}

export interface ClientDetail extends Omit<Client, 'projectCount' | 'primaryContact'> {
  registrationNumber: string | null
  address: string | null
  notes: string | null
  contacts: { id: string; name: string; title: string | null; phone: string | null; email: string | null; isPrimary: boolean }[]
}

export function getClients(search?: string) {
  const q = search ? `?search=${encodeURIComponent(search)}` : ''
  return apiFetch<Client[]>(`/clients${q}`)
}

export function getClient(id: string) {
  return apiFetch<ClientDetail>(`/clients/${id}`)
}

export function createClient(data: {
  name: string
  type: string
  registrationNumber?: string
  address?: string
  city?: string
  province?: string
  notes?: string
  contacts?: { name: string; title?: string; phone?: string; email?: string; isPrimary: boolean }[]
}) {
  return apiFetch<ClientDetail>('/clients', { method: 'POST', body: JSON.stringify(data) })
}

export function updateClient(id: string, data: {
  name: string
  type: string
  registrationNumber?: string
  address?: string
  city?: string
  province?: string
  notes?: string
}) {
  return apiFetch<ClientDetail>(`/clients/${id}`, { method: 'PUT', body: JSON.stringify(data) })
}
