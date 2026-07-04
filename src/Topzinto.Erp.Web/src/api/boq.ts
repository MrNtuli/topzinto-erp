import { apiFetch } from './client'
import { formatCurrency } from './projects'

export interface BoqSummary {
  totalValue: number
  itemCount: number
  projectCount: number
  averageRate: number
}

export interface FinancialSummary {
  boqTotal: number
  pendingClaims: number
  paidClaims: number
  outstandingInvoices: number
  paidInvoices: number
}

export interface BoqItem {
  id: string
  itemCode: string
  description: string
  category: string
  unit: string
  quantity: number
  rate: number
  amount: number
  projectId: string
  projectName: string
}

export interface Claim {
  id: string
  claimNumber: string
  title: string
  projectName: string
  status: string
  amount: number
  claimDate: string
  periodFrom: string | null
  periodTo: string | null
}

export interface Invoice {
  id: string
  invoiceNumber: string
  projectName: string
  clientName: string
  status: string
  amount: number
  invoiceDate: string
  dueDate: string | null
}

export function getBoqSummary() {
  return apiFetch<BoqSummary>('/boq/summary')
}

export function getFinancialSummary() {
  return apiFetch<FinancialSummary>('/boq/financial-summary')
}

export function getBoqItems(params?: { projectId?: string; search?: string }) {
  const q = new URLSearchParams()
  if (params?.projectId) q.set('projectId', params.projectId)
  if (params?.search) q.set('search', params.search)
  const query = q.toString()
  return apiFetch<BoqItem[]>(`/boq${query ? `?${query}` : ''}`)
}

export function getClaims(params?: { status?: string; projectId?: string }) {
  const q = new URLSearchParams()
  if (params?.status) q.set('status', params.status)
  if (params?.projectId) q.set('projectId', params.projectId)
  const query = q.toString()
  return apiFetch<Claim[]>(`/claims${query ? `?${query}` : ''}`)
}

export function getInvoices(params?: { status?: string; projectId?: string }) {
  const q = new URLSearchParams()
  if (params?.status) q.set('status', params.status)
  if (params?.projectId) q.set('projectId', params.projectId)
  const query = q.toString()
  return apiFetch<Invoice[]>(`/invoices${query ? `?${query}` : ''}`)
}

export function createBoqItem(data: {
  projectId: string
  itemCode: string
  description: string
  category: string
  unit: string
  quantity: number
  rate: number
  notes?: string
}) {
  return apiFetch<BoqItem>('/boq', { method: 'POST', body: JSON.stringify(data) })
}

export function createClaim(data: {
  projectId: string
  claimNumber: string
  title: string
  claimDate: string
  periodFrom?: string
  periodTo?: string
  amount: number
  status: string
  submittedByName?: string
  notes?: string
}) {
  return apiFetch<Claim>('/claims', { method: 'POST', body: JSON.stringify(data) })
}

export { formatCurrency }
