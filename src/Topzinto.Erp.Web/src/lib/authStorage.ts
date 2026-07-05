import type { StateStorage } from 'zustand/middleware'

const AUTH_KEY = 'topzinto-auth'
const REMEMBER_KEY = 'topzinto-auth-remember'

export function setRememberPreference(remember: boolean) {
  if (remember) {
    localStorage.setItem(REMEMBER_KEY, 'true')
  } else {
    localStorage.removeItem(REMEMBER_KEY)
  }
}

export function clearAuthStorage() {
  localStorage.removeItem(AUTH_KEY)
  sessionStorage.removeItem(AUTH_KEY)
  localStorage.removeItem(REMEMBER_KEY)
}

export function getStoredAccessToken(): string | null {
  return getStoredAuthField('accessToken')
}

export function getStoredRefreshToken(): string | null {
  return getStoredAuthField('refreshToken')
}

function getStoredAuthField(field: 'accessToken' | 'refreshToken'): string | null {
  for (const storage of [sessionStorage, localStorage]) {
    try {
      const raw = storage.getItem(AUTH_KEY)
      if (!raw) continue
      const value = JSON.parse(raw)?.state?.[field]
      if (typeof value === 'string' && value.length > 0) return value
    } catch {
      // ignore malformed storage
    }
  }
  return null
}

export const authPersistStorage: StateStorage = {
  getItem: (name) => sessionStorage.getItem(name) ?? localStorage.getItem(name),
  setItem: (name, value) => {
    const remember = localStorage.getItem(REMEMBER_KEY) === 'true'
    if (remember) {
      sessionStorage.removeItem(name)
      localStorage.setItem(name, value)
    } else {
      localStorage.removeItem(name)
      sessionStorage.setItem(name, value)
    }
  },
  removeItem: (name) => {
    localStorage.removeItem(name)
    sessionStorage.removeItem(name)
  },
}
