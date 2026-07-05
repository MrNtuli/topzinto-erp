import { NavLink } from 'react-router-dom'
import styles from './ScheduleSubNav.module.css'

export function ScheduleSubNav() {
  return (
    <nav className={styles.subNav} aria-label="Schedule views">
      <NavLink
        to="/schedule"
        end
        className={({ isActive }) => (isActive ? styles.linkActive : styles.link)}
      >
        Timeline
      </NavLink>
      <NavLink
        to="/schedule/gantt"
        className={({ isActive }) => (isActive ? styles.linkActive : styles.link)}
      >
        Gantt
      </NavLink>
    </nav>
  )
}
