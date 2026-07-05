import { apiFetch } from './client'

export interface ComplianceRecord {
  id: string
  title: string
  type: string
  entityType: string | null
  entityId: string | null
  projectId: string | null
  projectName: string | null
  issueDate: string
  expiryDate: string | null
  status: string
  responsiblePerson: string | null
  notes: string | null
}

export interface ComplianceRecordInput {
  title: string
  type: string
  entityType?: string
  entityId?: string
  projectId?: string
  issueDate: string
  expiryDate?: string
  status: string
  responsiblePerson?: string
  notes?: string
}

export function getComplianceRecords(params?: { projectId?: string; status?: string }) {
  const q = new URLSearchParams()
  if (params?.projectId) q.set('projectId', params.projectId)
  if (params?.status) q.set('status', params.status)
  const qs = q.toString()
  return apiFetch<ComplianceRecord[]>(`/compliance${qs ? `?${qs}` : ''}`)
}

export function getComplianceRecord(id: string) {
  return apiFetch<ComplianceRecord>(`/compliance/${id}`)
}

export function createComplianceRecord(data: ComplianceRecordInput) {
  return apiFetch<ComplianceRecord>('/compliance', { method: 'POST', body: JSON.stringify(data) })
}

export function updateComplianceRecord(id: string, data: ComplianceRecordInput) {
  return apiFetch<ComplianceRecord>(`/compliance/${id}`, { method: 'PUT', body: JSON.stringify(data) })
}

export function deleteComplianceRecord(id: string) {
  return apiFetch<void>(`/compliance/${id}`, { method: 'DELETE' })
}

export const COMPLIANCE_TYPES = ['Insurance', 'License', 'Certificate', 'Permit', 'Inspection', 'Other'] as const
export const COMPLIANCE_STATUSES = ['Valid', 'Expiring Soon', 'Expired', 'Pending', 'Revoked'] as const
