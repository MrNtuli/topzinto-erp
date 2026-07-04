import { apiFetch } from './client'
import { formatCurrency } from './projects'

export interface ProcurementSummary {
  totalOrders: number
  pendingApproval: number
  approved: number
  delivered: number
  totalValue: number
}

export interface PurchaseOrder {
  id: string
  poNumber: string
  title: string
  supplierName: string
  projectName: string | null
  status: string
  totalAmount: number
  orderDate: string
  requiredDate: string | null
}

export interface PurchaseOrderDetail extends PurchaseOrder {
  supplierId: string
  projectId: string | null
  requestedByName: string | null
  approvedByName: string | null
  notes: string | null
  lines: { id: string; description: string; quantity: number; unit: string; unitPrice: number; lineTotal: number }[]
}

export function getProcurementSummary() {
  return apiFetch<ProcurementSummary>('/procurement/summary')
}

export function getPurchaseOrders(params?: { search?: string; status?: string }) {
  const q = new URLSearchParams()
  if (params?.search) q.set('search', params.search)
  if (params?.status) q.set('status', params.status)
  const query = q.toString()
  return apiFetch<PurchaseOrder[]>(`/procurement${query ? `?${query}` : ''}`)
}

export function getPurchaseOrder(id: string) {
  return apiFetch<PurchaseOrderDetail>(`/procurement/${id}`)
}

export function createPurchaseOrder(data: {
  poNumber: string
  title: string
  supplierId: string
  projectId?: string
  status: string
  orderDate: string
  requiredDate?: string
  requestedByName?: string
  notes?: string
  lines: { description: string; quantity: number; unit: string; unitPrice: number }[]
}) {
  return apiFetch<PurchaseOrderDetail>('/procurement', { method: 'POST', body: JSON.stringify(data) })
}

export function updatePurchaseOrder(id: string, data: {
  title: string
  supplierId: string
  projectId?: string
  status: string
  orderDate: string
  requiredDate?: string
  requestedByName?: string
  approvedByName?: string
  notes?: string
  lines: { description: string; quantity: number; unit: string; unitPrice: number }[]
}) {
  return apiFetch<PurchaseOrderDetail>(`/procurement/${id}`, { method: 'PUT', body: JSON.stringify(data) })
}

export { formatCurrency }
