import { apiFetch } from './client'

export interface NotificationSummary {
  unreadCount: number
  total: number
}

export interface Notification {
  id: string
  title: string
  message: string
  category: string
  severity: string
  linkPath: string | null
  isRead: boolean
  createdAt: string
}

export function getNotificationSummary() {
  return apiFetch<NotificationSummary>('/notifications/summary')
}

export function getNotifications(unreadOnly?: boolean) {
  const q = unreadOnly ? '?unreadOnly=true' : ''
  return apiFetch<Notification[]>(`/notifications${q}`)
}

export function markNotificationRead(id: string) {
  return apiFetch<void>(`/notifications/${id}/read`, { method: 'POST' })
}

export function markAllNotificationsRead() {
  return apiFetch<void>('/notifications/read-all', { method: 'POST' })
}
