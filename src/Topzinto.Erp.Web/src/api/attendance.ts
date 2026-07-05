import { apiFetch } from './client'

export interface AttendanceRecord {
  id: string
  employeeId: string
  employeeName: string
  projectId: string | null
  projectName: string | null
  workDate: string
  status: string
  checkInTime: string | null
  checkOutTime: string | null
  hoursWorked: number | null
  notes: string | null
}

export interface AttendanceRecordInput {
  employeeId: string
  projectId?: string
  workDate: string
  status: string
  checkInTime?: string
  checkOutTime?: string
  hoursWorked?: number
  notes?: string
}

export function getAttendanceRecords(params?: {
  projectId?: string
  employeeId?: string
  status?: string
  fromDate?: string
  toDate?: string
}) {
  const q = new URLSearchParams()
  if (params?.projectId) q.set('projectId', params.projectId)
  if (params?.employeeId) q.set('employeeId', params.employeeId)
  if (params?.status) q.set('status', params.status)
  if (params?.fromDate) q.set('fromDate', params.fromDate)
  if (params?.toDate) q.set('toDate', params.toDate)
  const qs = q.toString()
  return apiFetch<AttendanceRecord[]>(`/attendance${qs ? `?${qs}` : ''}`)
}

export function getAttendanceRecord(id: string) {
  return apiFetch<AttendanceRecord>(`/attendance/${id}`)
}

export function createAttendanceRecord(data: AttendanceRecordInput) {
  return apiFetch<AttendanceRecord>('/attendance', { method: 'POST', body: JSON.stringify(data) })
}

export function updateAttendanceRecord(id: string, data: AttendanceRecordInput) {
  return apiFetch<AttendanceRecord>(`/attendance/${id}`, { method: 'PUT', body: JSON.stringify(data) })
}

export function deleteAttendanceRecord(id: string) {
  return apiFetch<void>(`/attendance/${id}`, { method: 'DELETE' })
}

export const ATTENDANCE_STATUSES = [
  'Present',
  'Absent',
  'Late',
  'HalfDay',
  'OnLeave',
  'PublicHoliday',
] as const
