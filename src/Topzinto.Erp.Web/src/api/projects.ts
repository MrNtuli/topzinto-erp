import { apiFetch } from './client'

export interface Project {
  id: string
  code: string
  name: string
  clientName: string
  clientId: string
  status: string
  progress: number
  endDate: string | null
  contractValue: number
}

export interface ProjectDetail extends Project {
  startDate: string | null
  budget: number
  description: string | null
  siteLocation: string | null
  contractId: string | null
  tenderId: string | null
  startDateInput: string | null
  endDateInput: string | null
}

export function getProjects(params?: { search?: string; status?: string; clientId?: string }) {
  const q = new URLSearchParams()
  if (params?.search) q.set('search', params.search)
  if (params?.status) q.set('status', params.status)
  if (params?.clientId) q.set('clientId', params.clientId)
  const query = q.toString()
  return apiFetch<Project[]>(`/projects${query ? `?${query}` : ''}`)
}

export function getProject(id: string) {
  return apiFetch<ProjectDetail>(`/projects/${id}`)
}

export function createProject(data: {
  code: string
  name: string
  clientId: string
  status: string
  progress: number
  startDate?: string
  endDate?: string
  contractValue: number
  budget: number
  description?: string
  siteLocation?: string
}) {
  return apiFetch<ProjectDetail>('/projects', { method: 'POST', body: JSON.stringify(data) })
}

export function updateProject(id: string, data: {
  code: string
  name: string
  clientId: string
  status: string
  progress: number
  startDate?: string
  endDate?: string
  contractValue: number
  budget: number
  description?: string
  siteLocation?: string
}) {
  return apiFetch<ProjectDetail>(`/projects/${id}`, { method: 'PUT', body: JSON.stringify(data) })
}

export interface ProjectActivityItem {
  id: string
  action: string
  module: string
  entityType: string
  entityId: string
  userEmail: string
  createdAt: string
  summary: string
}

export function getProjectActivity(projectId: string) {
  return apiFetch<ProjectActivityItem[]>(`/projects/${projectId}/activity`)
}

export function formatCurrency(amount: number) {
  return new Intl.NumberFormat('en-ZA', {
    style: 'currency',
    currency: 'ZAR',
    maximumFractionDigits: 0,
  }).format(amount)
}
