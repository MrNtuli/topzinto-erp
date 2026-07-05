import { create } from 'zustand'
import { createJSONStorage, persist } from 'zustand/middleware'
import { authPersistStorage, setRememberPreference } from '@/lib/authStorage'

export interface AuthUser {
  id: string
  email: string
  firstName: string
  lastName: string
  role: string
}

interface AuthState {
  user: AuthUser | null
  accessToken: string | null
  refreshToken: string | null
  isAuthenticated: boolean
  login: (user: AuthUser, accessToken: string, refreshToken: string, remember?: boolean) => void
  setTokens: (accessToken: string, refreshToken: string) => void
  updateUser: (user: AuthUser) => void
  logout: () => void
}

export const useAuthStore = create<AuthState>()(
  persist(
    (set) => ({
      user: null,
      accessToken: null,
      refreshToken: null,
      isAuthenticated: false,
      login: (user, accessToken, refreshToken, remember = false) => {
        setRememberPreference(remember)
        set({ user, accessToken, refreshToken, isAuthenticated: true })
      },
      setTokens: (accessToken, refreshToken) => set({ accessToken, refreshToken }),
      updateUser: (user) => set({ user }),
      logout: () => set({ user: null, accessToken: null, refreshToken: null, isAuthenticated: false }),
    }),
    {
      name: 'topzinto-auth',
      storage: createJSONStorage(() => authPersistStorage),
    },
  ),
)
