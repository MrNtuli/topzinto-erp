import { apiFetch } from './client'

export interface EquipmentSummary {
  total: number
  available: number
  inUse: number
  maintenance: number
  inspectionDue: number
}

export interface Equipment {
  id: string
  assetTag: string
  name: string
  category: string
  status: string
  operatorName: string | null
  assignedProjectName: string | null
  nextServiceDue: string | null
  isInspectionDue: boolean
}

export interface EquipmentDetail extends Equipment {
  makeModel: string | null
  serialNumber: string | null
  lastInspectionDate: string | null
  nextInspectionDue: string | null
  lastServiceDate: string | null
  assignedProjectId: string | null
  notes: string | null
  nextInspectionDueInput: string | null
  nextServiceDueInput: string | null
  bookings: { id: string; projectName: string; startDate: string; endDate: string; bookedByName: string | null }[]
  inspections: { id: string; inspectionDate: string; result: string; inspectorName: string | null; nextDueDate: string | null }[]
}

export function getEquipmentSummary() {
  return apiFetch<EquipmentSummary>('/equipment/summary')
}

export function getEquipmentList(params?: { search?: string; status?: string }) {
  const q = new URLSearchParams()
  if (params?.search) q.set('search', params.search)
  if (params?.status) q.set('status', params.status)
  const query = q.toString()
  return apiFetch<Equipment[]>(`/equipment${query ? `?${query}` : ''}`)
}

export function getEquipment(id: string) {
  return apiFetch<EquipmentDetail>(`/equipment/${id}`)
}

export function createEquipment(data: {
  assetTag: string
  name: string
  category: string
  status: string
  makeModel?: string
  serialNumber?: string
  operatorName?: string
  nextInspectionDue?: string
  nextServiceDue?: string
  assignedProjectId?: string
  notes?: string
}) {
  return apiFetch<EquipmentDetail>('/equipment', { method: 'POST', body: JSON.stringify(data) })
}

export function updateEquipment(id: string, data: Parameters<typeof createEquipment>[0]) {
  return apiFetch<EquipmentDetail>(`/equipment/${id}`, { method: 'PUT', body: JSON.stringify(data) })
}
