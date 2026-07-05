import { Link } from 'react-router-dom'
import { Menu } from 'lucide-react'
import { useAuthStore } from '@/stores/authStore'
import { NotificationBell } from './NotificationBell'
import { GlobalSearch } from './GlobalSearch'
import styles from './TopBar.module.css'

interface TopBarProps {
  onMenuClick: () => void
}

export function TopBar({ onMenuClick }: TopBarProps) {
  const user = useAuthStore((s) => s.user)

  return (
    <header className={styles.topbar}>
      <div className={styles.left}>
        <button className={styles.menuBtn} onClick={onMenuClick} aria-label="Menu">
          <Menu size={22} />
        </button>
        <GlobalSearch />
      </div>

      <div className={styles.right}>
        <NotificationBell />

        <Link to="/profile" className={styles.profile} title="My Profile">
          <div className={styles.avatar}>
            {user?.firstName?.[0]}
            {user?.lastName?.[0]}
          </div>
          <div className={styles.profileInfo}>
            <span className={styles.profileName}>
              {user?.firstName} {user?.lastName}
            </span>
            <span className={styles.profileRole}>{user?.role}</span>
          </div>
        </Link>
      </div>
    </header>
  )
}
