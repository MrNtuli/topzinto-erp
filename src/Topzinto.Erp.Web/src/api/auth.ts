import { authorizedFetch } from './client'
import { getMyProfile, updateMyProfile, type UserProfile } from './users'

export interface CurrentUser {
  id: string
  email: string
  firstName: string
  lastName: string
  role: string
  phone?: string | null
  lastLoginAt?: string | null
}

export interface ChangePasswordRequest {
  currentPassword: string
  newPassword: string
}

export interface UpdateProfileRequest {
  firstName: string
  lastName: string
  phone?: string | null
}

export interface ResetPasswordWithTokenRequest {
  email: string
  token: string
  newPassword: string
}

function mapProfile(profile: UserProfile): CurrentUser {
  return {
    id: profile.id,
    email: profile.email,
    firstName: profile.firstName,
    lastName: profile.lastName,
    role: profile.role,
    phone: profile.phone,
    lastLoginAt: profile.lastLoginAt,
  }
}

export function getCurrentUser() {
  return getMyProfile().then(mapProfile)
}

export function updateProfile(data: UpdateProfileRequest) {
  return updateMyProfile(data).then(mapProfile)
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
