const API_BASE = '/api'

import { clearAuthStorage, getStoredAccessToken, getStoredRefreshToken } from '@/lib/authStorage'

export interface LoginRequest {
  email: string
  password: string
  rememberMe?: boolean
}

export interface LoginResponse {
  accessToken: string
  refreshToken: string
  user: {
    id: string
    email: string
    firstName: string
    lastName: string
    role: string
  }
}

export type LoginApiResult =
  | { kind: 'authenticated'; response: LoginResponse }
  | { kind: 'mfa'; mfaToken: string; message: string }

let refreshPromise: Promise<boolean> | null = null

export function clearSessionAndRedirect(reason = 'session-expired') {
  clearAuthStorage()
  if (!window.location.pathname.startsWith('/login')) {
    window.location.href = `/login?reason=${encodeURIComponent(reason)}`
  }
}

export async function handleUnauthorizedResponse(res: Response): Promise<never> {
  const err = await res.json().catch(() => ({}))
  clearSessionAndRedirect()
  throw new Error(err.message || 'Session expired')
}

async function tryRefreshAccessToken(): Promise<boolean> {
  if (refreshPromise) return refreshPromise

  refreshPromise = (async () => {
    const refreshToken = getStoredRefreshToken()
    if (!refreshToken) return false

    try {
      const res = await fetch(`${API_BASE}/auth/refresh`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ refreshToken }),
      })
      if (!res.ok) return false

      const body = (await res.json()) as { accessToken: string; refreshToken: string }
      const { useAuthStore } = await import('@/stores/authStore')
      useAuthStore.getState().setTokens(body.accessToken, body.refreshToken)
      return true
    } catch {
      return false
    } finally {
      refreshPromise = null
    }
  })()

  return refreshPromise
}

export async function loginApi(data: LoginRequest): Promise<LoginApiResult> {
  const res = await fetch(`${API_BASE}/auth/login`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(data),
  })
  const body = await res.json().catch(() => ({}))
  if (!res.ok) {
    throw new Error(body.message || 'Login failed')
  }
  if (typeof body.mfaToken === 'string' && body.mfaToken.length > 0) {
    return {
      kind: 'mfa',
      mfaToken: body.mfaToken,
      message: body.message || 'Enter your authenticator code.',
    }
  }
  return { kind: 'authenticated', response: body as LoginResponse }
}

export async function apiFetch<T>(path: string, options: RequestInit = {}, retry = true): Promise<T> {
  const token = getStoredAccessToken()
  const res = await fetch(`${API_BASE}${path}`, {
    ...options,
    headers: {
      'Content-Type': 'application/json',
      ...(token ? { Authorization: `Bearer ${token}` } : {}),
      ...options.headers,
    },
  })
  if (res.status === 401 && retry) {
    const refreshed = await tryRefreshAccessToken()
    if (refreshed) return apiFetch<T>(path, options, false)
    await handleUnauthorizedResponse(res)
  }
  if (res.status === 401) await handleUnauthorizedResponse(res)
  if (!res.ok) throw new Error(`API error: ${res.status}`)
  if (res.status === 204) return undefined as T
  const text = await res.text()
  if (!text) return undefined as T
  return JSON.parse(text) as T
}

export async function authorizedFetch(path: string, options: RequestInit = {}, retry = true) {
  const token = getStoredAccessToken()
  const res = await fetch(`${API_BASE}${path}`, {
    ...options,
    headers: {
      ...(token ? { Authorization: `Bearer ${token}` } : {}),
      ...options.headers,
    },
  })
  if (res.status === 401 && retry) {
    const refreshed = await tryRefreshAccessToken()
    if (refreshed) return authorizedFetch(path, options, false)
    await handleUnauthorizedResponse(res)
  }
  if (res.status === 401) await handleUnauthorizedResponse(res)
  return res
}
