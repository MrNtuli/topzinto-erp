import { apiFetch } from './client'
import { formatCurrency } from './projects'

export interface StoresSummary {
  totalItems: number
  lowStock: number
  totalValue: number
  transactionsThisMonth: number
}

export interface InventoryItem {
  id: string
  itemCode: string
  name: string
  category: string
  unit: string
  quantityOnHand: number
  reorderLevel: number
  location: string | null
  unitCost: number
  isLowStock: boolean
}

export interface InventoryItemDetail extends Omit<InventoryItem, 'isLowStock'> {
  notes: string | null
  recentTransactions: InventoryTransaction[]
}

export interface InventoryTransaction {
  id: string
  itemCode: string
  itemName: string
  transactionType: string
  quantity: number
  transactionDate: string
  reference: string | null
  projectName: string | null
  recordedByName: string | null
  notes: string | null
}

export function getStoresSummary() {
  return apiFetch<StoresSummary>('/stores/summary')
}

export function getInventoryItems(params?: { search?: string; lowStockOnly?: boolean }) {
  const q = new URLSearchParams()
  if (params?.search) q.set('search', params.search)
  if (params?.lowStockOnly) q.set('lowStockOnly', 'true')
  const query = q.toString()
  return apiFetch<InventoryItem[]>(`/stores/items${query ? `?${query}` : ''}`)
}

export function getInventoryItem(id: string) {
  return apiFetch<InventoryItemDetail>(`/stores/items/${id}`)
}

export function getInventoryTransactions(search?: string) {
  const q = new URLSearchParams()
  if (search) q.set('search', search)
  const query = q.toString()
  return apiFetch<InventoryTransaction[]>(`/stores/transactions${query ? `?${query}` : ''}`)
}

export function createInventoryItem(data: {
  itemCode: string
  name: string
  category: string
  unit: string
  quantityOnHand: number
  reorderLevel: number
  location?: string
  unitCost: number
  notes?: string
}) {
  return apiFetch<InventoryItemDetail>('/stores/items', { method: 'POST', body: JSON.stringify(data) })
}

export function createInventoryTransaction(data: {
  inventoryItemId: string
  transactionType: string
  quantity: number
  transactionDate: string
  reference?: string
  projectId?: string
  notes?: string
}) {
  return apiFetch<InventoryTransaction>('/stores/transactions', { method: 'POST', body: JSON.stringify(data) })
}

export function updateInventoryItem(id: string, data: {
  itemCode: string
  name: string
  category: string
  unit: string
  reorderLevel: number
  location?: string
  unitCost: number
  notes?: string
}) {
  return apiFetch<InventoryItemDetail>(`/stores/items/${id}`, { method: 'PUT', body: JSON.stringify(data) })
}

export { formatCurrency }
