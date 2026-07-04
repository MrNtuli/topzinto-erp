import { Link } from 'react-router-dom'
import type { LucideIcon } from 'lucide-react'
import styles from './KpiCard.module.css'

interface KpiCardProps {
  label: string
  value: string
  icon: LucideIcon
  color: 'blue' | 'orange' | 'green' | 'red'
  link?: string
}

const colorMap = {
  blue: styles.blue,
  orange: styles.orange,
  green: styles.green,
  red: styles.red,
}

export function KpiCard({ label, value, icon: Icon, color, link }: KpiCardProps) {
  return (
    <div className={styles.card}>
      <div className={`${styles.icon} ${colorMap[color]}`}>
        <Icon size={22} />
      </div>
      <div className={styles.content}>
        <span className={styles.value}>{value}</span>
        <span className={styles.label}>{label}</span>
        {link && (
          <Link to={link} className={styles.link}>
            View all
          </Link>
        )}
      </div>
    </div>
  )
}
