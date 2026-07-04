import { useState } from 'react'
import { Sidebar } from './Sidebar'
import { TopBar } from './TopBar'
import { IdleWarningBanner } from './IdleWarningBanner'
import { useIdleSession } from '@/lib/useIdleSession'
import styles from './AppShell.module.css'

interface AppShellProps {
  children: React.ReactNode
}

export function AppShell({ children }: AppShellProps) {
  const [collapsed, setCollapsed] = useState(false)
  const { showWarning, warnMinutes } = useIdleSession(true)

  return (
    <div className={styles.shell}>
      {showWarning && <IdleWarningBanner minutesLeft={warnMinutes} />}
      <Sidebar collapsed={collapsed} onToggle={() => setCollapsed((c) => !c)} />
      <div className={styles.main} data-collapsed={collapsed}>
        <TopBar onMenuClick={() => setCollapsed((c) => !c)} />
        <main className={styles.content}>{children}</main>
      </div>
    </div>
  )
}
