import { useLocation } from 'react-router-dom'
import { useAuthStore } from '@/stores/authStore'
import { canAccessRoute } from '@/lib/roleAccess'
import { AccessDeniedPage } from '@/pages/auth/AccessDeniedPage'

export function RoleRoute({ children }: { children: React.ReactNode }) {
  const role = useAuthStore((s) => s.user?.role)
  const { pathname } = useLocation()

  if (!canAccessRoute(role, pathname)) {
    return <AccessDeniedPage />
  }

  return <>{children}</>
}
