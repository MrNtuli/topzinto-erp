import { apiFetch, authorizedFetch } from './client'

export interface CurrentUser {
  id: string
  email: string
  firstName: string
  lastName: string
  role: string
}

export interface ChangePasswordRequest {
  currentPassword: string
  newPassword: string
}

export interface UpdateProfileRequest {
  firstName: string
  lastName: string
}

export interface ResetPasswordWithTokenRequest {
  email: string
  token: string
  newPassword: string
}

export function getCurrentUser() {
  return apiFetch<CurrentUser>('/auth/me')
}

export function updateProfile(data: UpdateProfileRequest) {
  return apiFetch<CurrentUser>('/auth/profile', {
    method: 'PUT',
    body: JSON.stringify(data),
  })
}

export async function changePassword(data: ChangePasswordRequest) {
  const res = await authorizedFetch('/auth/change-password', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(data),
  })
  if (!res.ok) {
    const err = await res.json().catch(() => ({}))
    throw new Error(err.message || 'Password change failed')
  }
  return res.json() as Promise<{ message: string }>
}

export async function requestPasswordReset(email: string) {
  const res = await fetch('/api/auth/forgot-password', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ email }),
  })
  if (!res.ok) throw new Error('Request failed')
  return res.json() as Promise<{ message: string; devResetLink?: string | null }>
}

export async function resetPasswordWithToken(data: ResetPasswordWithTokenRequest) {
  const res = await fetch('/api/auth/reset-password', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(data),
  })
  if (!res.ok) {
    const err = await res.json().catch(() => ({}))
    throw new Error(err.message || 'Password reset failed')
  }
  return res.json() as Promise<{ message: string }>
}
