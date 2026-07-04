import { apiFetch } from './client'
import { formatCurrency } from './projects'

export interface Contract {
  id: string
  contractNumber: string
  title: string
  clientName: string
  clientId: string
  status: string
  value: number
  endDate: string | null
  projectName: string | null
}

export function getContracts(params?: { search?: string; status?: string }) {
  const q = new URLSearchParams()
  if (params?.search) q.set('search', params.search)
  if (params?.status) q.set('status', params.status)
  const query = q.toString()
  return apiFetch<Contract[]>(`/contracts${query ? `?${query}` : ''}`)
}

export function createContract(data: {
  contractNumber: string
  title: string
  clientId: string
  projectId?: string
  value: number
  startDate?: string
  endDate?: string
  retentionPercent: number
  status: string
  notes?: string
}) {
  return apiFetch<Contract>('/contracts', { method: 'POST', body: JSON.stringify(data) })
}

export function getContract(id: string) {
  return apiFetch<Contract & { startDate: string | null; retentionPercent: number; notes: string | null; projectId: string | null }>(`/contracts/${id}`)
}

export function updateContract(id: string, data: {
  contractNumber: string
  title: string
  clientId: string
  projectId?: string
  value: number
  startDate?: string
  endDate?: string
  retentionPercent: number
  status: string
  notes?: string
}) {
  return apiFetch<Contract>(`/contracts/${id}`, { method: 'PUT', body: JSON.stringify(data) })
}

export { formatCurrency }
