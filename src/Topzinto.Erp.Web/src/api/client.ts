const API_BASE = '/api'

export interface LoginRequest {
  email: string
  password: string
}

export interface LoginResponse {
  accessToken: string
  user: {
    id: string
    email: string
    firstName: string
    lastName: string
    role: string
  }
}

export function clearSessionAndRedirect(reason = 'session-expired') {
  localStorage.removeItem('topzinto-auth')
  if (!window.location.pathname.startsWith('/login')) {
    window.location.href = `/login?reason=${encodeURIComponent(reason)}`
  }
}

export async function handleUnauthorizedResponse(res: Response): Promise<never> {
  const err = await res.json().catch(() => ({}))
  clearSessionAndRedirect()
  throw new Error(err.message || 'Session expired')
}

export async function loginApi(data: LoginRequest): Promise<LoginResponse> {
  const res = await fetch(`${API_BASE}/auth/login`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(data),
  })
  if (!res.ok) {
    const err = await res.json().catch(() => ({}))
    throw new Error(err.message || 'Login failed')
  }
  return res.json()
}

export async function apiFetch<T>(path: string, options: RequestInit = {}): Promise<T> {
  const token = JSON.parse(localStorage.getItem('topzinto-auth') || '{}')?.state?.accessToken
  const res = await fetch(`${API_BASE}${path}`, {
    ...options,
    headers: {
      'Content-Type': 'application/json',
      ...(token ? { Authorization: `Bearer ${token}` } : {}),
      ...options.headers,
    },
  })
  if (res.status === 401) await handleUnauthorizedResponse(res)
  if (!res.ok) throw new Error(`API error: ${res.status}`)
  if (res.status === 204) return undefined as T
  const text = await res.text()
  if (!text) return undefined as T
  return JSON.parse(text) as T
}

export async function authorizedFetch(path: string, options: RequestInit = {}) {
  const token = JSON.parse(localStorage.getItem('topzinto-auth') || '{}')?.state?.accessToken
  const res = await fetch(`${API_BASE}${path}`, {
    ...options,
    headers: {
      ...(token ? { Authorization: `Bearer ${token}` } : {}),
      ...options.headers,
    },
  })
  if (res.status === 401) await handleUnauthorizedResponse(res)
  return res
}
