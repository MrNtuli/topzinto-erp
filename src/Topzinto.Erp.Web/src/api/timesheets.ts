import { apiFetch } from './client'

export interface TimesheetEntry {
  id: string
  employeeId: string
  employeeName: string
  projectId: string
  projectName: string
  workDate: string
  hours: number
  status: string
  description: string | null
  notes: string | null
  labourCost: number | null
}

export interface ProjectLabourSummary {
  projectId: string
  projectName: string
  totalHours: number
  totalLabourCost: number
  entryCount: number
}

export function getTimesheets(params?: { projectId?: string; employeeId?: string; status?: string }) {
  const q = new URLSearchParams()
  if (params?.projectId) q.set('projectId', params.projectId)
  if (params?.employeeId) q.set('employeeId', params.employeeId)
  if (params?.status) q.set('status', params.status)
  const query = q.toString()
  return apiFetch<TimesheetEntry[]>(`/timesheets${query ? `?${query}` : ''}`)
}

export function getTimesheet(id: string) {
  return apiFetch<TimesheetEntry>(`/timesheets/${id}`)
}

export function getLabourSummary(projectId?: string) {
  const q = projectId ? `?projectId=${projectId}` : ''
  return apiFetch<ProjectLabourSummary[]>(`/timesheets/labour-summary${q}`)
}

export function createTimesheet(data: {
  employeeId: string
  projectId: string
  workDate: string
  hours: number
  status: string
  description?: string
  notes?: string
}) {
  return apiFetch<TimesheetEntry>('/timesheets', { method: 'POST', body: JSON.stringify(data) })
}

export function updateTimesheet(id: string, data: {
  employeeId: string
  projectId: string
  workDate: string
  hours: number
  status: string
  description?: string
  notes?: string
}) {
  return apiFetch<TimesheetEntry>(`/timesheets/${id}`, { method: 'PUT', body: JSON.stringify(data) })
}
