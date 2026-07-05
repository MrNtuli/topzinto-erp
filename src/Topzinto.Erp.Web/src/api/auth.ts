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

export async function logoutApi(refreshToken?: string | null) {
  const res = await authorizedFetch('/auth/logout', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ refreshToken: refreshToken ?? null }),
  }, false)
  if (!res.ok && res.status !== 401) {
    const err = await res.json().catch(() => ({}))
    throw new Error(err.message || 'Logout failed')
  }
}

export function getMfaStatus() {
  return authorizedFetch('/auth/mfa/status').then((r) => r.json()) as Promise<{ enabled: boolean }>
}

export function beginMfaSetup() {
  return authorizedFetch('/auth/mfa/setup', { method: 'POST' }).then((r) => r.json()) as Promise<{
    sharedKey: string
    authenticatorUri: string
  }>
}

export function enableMfa(code: string) {
  return authorizedFetch('/auth/mfa/enable', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ code }),
  }).then(async (r) => {
    if (!r.ok) {
      const err = await r.json().catch(() => ({}))
      throw new Error(err.message || 'Unable to enable MFA')
    }
    return r.json() as Promise<{ message: string }>
  })
}

export function disableMfa(code: string) {
  return authorizedFetch('/auth/mfa/disable', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ code }),
  }).then(async (r) => {
    if (!r.ok) {
      const err = await r.json().catch(() => ({}))
      throw new Error(err.message || 'Unable to disable MFA')
    }
    return r.json() as Promise<{ message: string }>
  })
}

export async function verifyMfaLogin(mfaToken: string, code: string, rememberMe = false) {
  const res = await fetch('/api/auth/mfa/verify', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ mfaToken, code, rememberMe }),
  })
  const body = await res.json().catch(() => ({}))
  if (!res.ok) throw new Error(body.message || 'Invalid verification code')
  return body as {
    accessToken: string
    refreshToken: string
    user: { id: string; email: string; firstName: string; lastName: string; role: string }
  }
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
