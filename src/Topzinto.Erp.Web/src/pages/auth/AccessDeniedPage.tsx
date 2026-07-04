import { Link } from 'react-router-dom'
import { ShieldOff } from 'lucide-react'
import styles from './AccessDeniedPage.module.css'

export function AccessDeniedPage() {
  return (
    <div className={styles.page}>
      <ShieldOff size={48} className={styles.icon} />
      <h1>Access Denied</h1>
      <p>You don&apos;t have permission to view this module. Contact your administrator if you need access.</p>
      <Link to="/" className={styles.link}>Back to Dashboard</Link>
    </div>
  )
}
