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
  isAuthenticated: boolean
  login: (user: AuthUser, token: string, remember?: boolean) => void
  updateUser: (user: AuthUser) => void
  logout: () => void
}

export const useAuthStore = create<AuthState>()(
  persist(
    (set) => ({
      user: null,
      accessToken: null,
      isAuthenticated: false,
      login: (user, accessToken, remember = false) => {
        setRememberPreference(remember)
        set({ user, accessToken, isAuthenticated: true })
      },
      updateUser: (user) => set({ user }),
      logout: () => set({ user: null, accessToken: null, isAuthenticated: false }),
    }),
    {
      name: 'topzinto-auth',
      storage: createJSONStorage(() => authPersistStorage),
    },
  ),
)
