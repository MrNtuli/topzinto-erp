import { apiFetch, authorizedFetch } from './client'

export interface AuditLog {
  id: string
  userEmail: string
  action: string
  module: string
  entityType: string
  entityId: string
  newValues: string | null
  createdAt: string
}

export interface BackupFile {
  fileName: string
  sizeBytes: number
  createdAt: string
}

export interface BackupHub {
  engine: string
  scheduleEnabled: boolean
  intervalHours: number
  retentionCount: number
  files: BackupFile[]
}

export interface UserAdmin {
  id: string
  email: string
  firstName: string
  lastName: string
  role: string
  systemRole: string
  isActive: boolean
  lastLoginAt: string | null
}

export interface RoleOption {
  value: string
  label: string
}

export interface CreateUserRequest {
  email: string
  firstName: string
  lastName: string
  role: string
  password: string
}

export interface UpdateUserRequest {
  firstName: string
  lastName: string
  role: string
  isActive: boolean
}

export function getAuditLogs(count = 100) {
  return apiFetch<AuditLog[]>(`/admin/audit?count=${count}`)
}

export function getBackupHub() {
  return apiFetch<BackupHub>('/admin/backups')
}

export function createBackup() {
  return apiFetch<{ fileName: string; message: string }>('/admin/backup', { method: 'POST' })
}

export async function downloadBackupFile(fileName: string) {
  const res = await authorizedFetch(`/admin/backups/${encodeURIComponent(fileName)}/download`)
  if (!res.ok) throw new Error('Download failed')
  const blob = await res.blob()
  const url = URL.createObjectURL(blob)
  const a = document.createElement('a')
  a.href = url
  a.download = fileName
  a.click()
  URL.revokeObjectURL(url)
}

export function getUsers() {
  return apiFetch<UserAdmin[]>('/admin/users')
}

export function getRoles() {
  return apiFetch<RoleOption[]>('/admin/roles')
}

export function createUser(data: CreateUserRequest) {
  return apiFetch<UserAdmin>('/admin/users', { method: 'POST', body: JSON.stringify(data) })
}

export function updateUser(id: string, data: UpdateUserRequest) {
  return apiFetch<UserAdmin>(`/admin/users/${id}`, { method: 'PUT', body: JSON.stringify(data) })
}

export async function resetUserPassword(id: string, newPassword: string) {
  const res = await authorizedFetch(`/admin/users/${id}/reset-password`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ newPassword }),
  })
  if (!res.ok) {
    const err = await res.json().catch(() => ({}))
    throw new Error(err.message || 'Password reset failed')
  }
  return res.json() as Promise<{ message: string }>
}

export async function invalidateCache() {
  return apiFetch<{ message: string }>('/admin/cache/invalidate', { method: 'POST' })
}

export function scanSystemAlerts() {
  return apiFetch<{ count: number; message: string }>('/admin/alerts/scan', { method: 'POST' })
}

export async function exportAuditLogsCsv(count = 1000) {
  const res = await authorizedFetch(`/admin/audit/export?count=${count}`)
  if (!res.ok) throw new Error('Export failed')
  const blob = await res.blob()
  const url = URL.createObjectURL(blob)
  const a = document.createElement('a')
  a.href = url
  a.download = `audit_log_${new Date().toISOString().slice(0, 10)}.csv`
  a.click()
  URL.revokeObjectURL(url)
}
