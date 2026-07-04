import { apiFetch } from './client'

export interface Supplier {
  id: string
  code: string
  name: string
  category: string
  status: string
  contactPerson: string | null
  phone: string | null
  email: string | null
  city: string | null
  purchaseOrderCount: number
}

export interface SupplierDetail extends Omit<Supplier, 'purchaseOrderCount'> {
  address: string | null
  province: string | null
  vatNumber: string | null
  notes: string | null
  recentOrders: { id: string; poNumber: string; title: string; status: string; projectName: string | null; totalAmount: number; orderDate: string }[]
}

export function getSuppliers(params?: { search?: string; status?: string }) {
  const q = new URLSearchParams()
  if (params?.search) q.set('search', params.search)
  if (params?.status) q.set('status', params.status)
  const query = q.toString()
  return apiFetch<Supplier[]>(`/suppliers${query ? `?${query}` : ''}`)
}

export function getSupplier(id: string) {
  return apiFetch<SupplierDetail>(`/suppliers/${id}`)
}

export function createSupplier(data: {
  code: string
  name: string
  category: string
  status: string
  contactPerson?: string
  phone?: string
  email?: string
  address?: string
  city?: string
  province?: string
  vatNumber?: string
  notes?: string
}) {
  return apiFetch<SupplierDetail>('/suppliers', { method: 'POST', body: JSON.stringify(data) })
}

export function updateSupplier(id: string, data: {
  code: string
  name: string
  category: string
  status: string
  contactPerson?: string
  phone?: string
  email?: string
  address?: string
  city?: string
  province?: string
  vatNumber?: string
  notes?: string
}) {
  return apiFetch<SupplierDetail>(`/suppliers/${id}`, { method: 'PUT', body: JSON.stringify(data) })
}
