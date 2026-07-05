import { apiFetch } from './client'

export interface SafetyIncident {
  id: string
  projectId: string
  projectName: string
  incidentDate: string
  title: string
  description: string
  severity: string
  status: string
  location: string | null
  reportedByName: string | null
  correctiveAction: string | null
}

export interface SafetyIncidentInput {
  projectId: string
  incidentDate: string
  title: string
  description: string
  severity: string
  status: string
  location?: string
  reportedByName?: string
  correctiveAction?: string
}

export function getSafetyIncidents(params?: { projectId?: string; status?: string }) {
  const q = new URLSearchParams()
  if (params?.projectId) q.set('projectId', params.projectId)
  if (params?.status) q.set('status', params.status)
  const qs = q.toString()
  return apiFetch<SafetyIncident[]>(`/safety${qs ? `?${qs}` : ''}`)
}

export function getSafetyIncident(id: string) {
  return apiFetch<SafetyIncident>(`/safety/${id}`)
}

export function createSafetyIncident(data: SafetyIncidentInput) {
  return apiFetch<SafetyIncident>('/safety', { method: 'POST', body: JSON.stringify(data) })
}

export function updateSafetyIncident(id: string, data: SafetyIncidentInput) {
  return apiFetch<SafetyIncident>(`/safety/${id}`, { method: 'PUT', body: JSON.stringify(data) })
}

export function deleteSafetyIncident(id: string) {
  return apiFetch<void>(`/safety/${id}`, { method: 'DELETE' })
}

export const SAFETY_SEVERITIES = ['Low', 'Medium', 'High', 'Critical'] as const
export const SAFETY_STATUSES = ['Reported', 'Investigating', 'Resolved', 'Closed'] as const
