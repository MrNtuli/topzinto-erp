import { apiFetch } from './client'
import { formatCurrency } from './projects'

export interface Tender {
  id: string
  referenceNumber: string
  title: string
  clientName: string
  clientId: string
  status: string
  closingDate: string
  estimatedValue: number
}

export function getTenders(params?: { search?: string; status?: string }) {
  const q = new URLSearchParams()
  if (params?.search) q.set('search', params.search)
  if (params?.status) q.set('status', params.status)
  const query = q.toString()
  return apiFetch<Tender[]>(`/tenders${query ? `?${query}` : ''}`)
}

export function createTender(data: {
  referenceNumber: string
  title: string
  clientId: string
  closingDate: string
  status: string
  estimatedValue: number
  notes?: string
}) {
  return apiFetch<Tender & { notes?: string | null }>('/tenders', { method: 'POST', body: JSON.stringify(data) })
}

export function getTender(id: string) {
  return apiFetch<Tender & { notes: string | null }>(`/tenders/${id}`)
}

export function updateTender(id: string, data: {
  referenceNumber: string
  title: string
  clientId: string
  closingDate: string
  status: string
  estimatedValue: number
  notes?: string
}) {
  return apiFetch<Tender & { notes: string | null }>(`/tenders/${id}`, { method: 'PUT', body: JSON.stringify(data) })
}

export { formatCurrency }
