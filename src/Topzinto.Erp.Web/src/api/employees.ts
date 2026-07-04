import { apiFetch } from './client'

export interface Employee {
  id: string
  employeeNumber: string
  fullName: string
  jobTitle: string
  department: string
  trade: string
  status: string
  phone: string | null
  email: string | null
  assignedProjectName: string | null
  hireDate: string
}

export interface EmployeeDetail {
  id: string
  employeeNumber: string
  firstName: string
  lastName: string
  idNumber: string | null
  jobTitle: string
  department: string
  trade: string
  status: string
  phone: string | null
  email: string | null
  hireDate: string
  terminationDate: string | null
  assignedProjectId: string | null
  assignedProjectName: string | null
  hourlyRate: number | null
  notes: string | null
}

export function getEmployees(params?: { search?: string; status?: string; department?: string }) {
  const q = new URLSearchParams()
  if (params?.search) q.set('search', params.search)
  if (params?.status) q.set('status', params.status)
  if (params?.department) q.set('department', params.department)
  const query = q.toString()
  return apiFetch<Employee[]>(`/employees${query ? `?${query}` : ''}`)
}

export function getEmployee(id: string) {
  return apiFetch<EmployeeDetail>(`/employees/${id}`)
}

export function createEmployee(data: {
  employeeNumber: string
  firstName: string
  lastName: string
  idNumber?: string
  jobTitle: string
  department: string
  trade: string
  status: string
  phone?: string
  email?: string
  hireDate: string
  terminationDate?: string
  assignedProjectId?: string
  hourlyRate?: number
  notes?: string
}) {
  return apiFetch<EmployeeDetail>('/employees', { method: 'POST', body: JSON.stringify(data) })
}

export function updateEmployee(id: string, data: {
  employeeNumber: string
  firstName: string
  lastName: string
  idNumber?: string
  jobTitle: string
  department: string
  trade: string
  status: string
  phone?: string
  email?: string
  hireDate: string
  terminationDate?: string
  assignedProjectId?: string
  hourlyRate?: number
  notes?: string
}) {
  return apiFetch<EmployeeDetail>(`/employees/${id}`, { method: 'PUT', body: JSON.stringify(data) })
}
