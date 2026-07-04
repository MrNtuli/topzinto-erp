import { useQuery } from '@tanstack/react-query'
import { Link } from 'react-router-dom'
import { Plus } from 'lucide-react'
import { getScheduleEvents } from '@/api/schedule'
import styles from './SchedulePage.module.css'
import pageStyles from '../projects/ProjectsPage.module.css'

const TYPE_COLORS: Record<string, string> = {
  Milestone: '#2563eb',
  Task: '#f26522',
  Tender: '#22c55e',
}

export function SchedulePage() {
  const { data, isLoading, isError } = useQuery({
    queryKey: ['schedule'],
    queryFn: getScheduleEvents,
    retry: false,
  })

  const grouped = data?.reduce<Record<string, typeof data>>((acc, e) => {
    const month = e.date.slice(0, 7)
    if (!acc[month]) acc[month] = []
    acc[month].push(e)
    return acc
  }, {}) ?? {}

  return (
    <div className={styles.page}>
      <nav className={styles.breadcrumb}>Home &gt; Schedule</nav>
      <header className={styles.header}>
        <h1>Schedule</h1>
        <div style={{ display: 'flex', alignItems: 'center', gap: 16 }}>
          <div className={styles.legend}>
            {Object.entries(TYPE_COLORS).map(([type, color]) => (
              <span key={type}><i style={{ background: color }} /> {type}</span>
            ))}
          </div>
          <Link to="/schedule/new-task" className={pageStyles.newBtn}>
            <Plus size={18} /> Add Task
          </Link>
        </div>
      </header>

      {isLoading && <p>Loading schedule...</p>}
      {isError && <p className={styles.hint}>Start the API to load schedule events.</p>}

      <div className={styles.timeline}>
        {Object.entries(grouped).map(([month, events]) => (
          <section key={month} className={styles.month}>
            <h2>{new Date(month + '-01').toLocaleDateString('en-ZA', { month: 'long', year: 'numeric' })}</h2>
            <div className={styles.events}>
              {events?.map((e) => (
                <div key={`${e.type}-${e.id}`} className={styles.event} style={{ borderLeftColor: TYPE_COLORS[e.type] ?? '#94a3b8' }}>
                  <span className={styles.date}>{new Date(e.date).toLocaleDateString('en-ZA', { day: 'numeric', month: 'short' })}</span>
                  <div>
                    <strong>{e.title}</strong>
                    <span className={styles.meta}>{e.type} · {e.projectName ?? '—'} · {e.status}</span>
                  </div>
                </div>
              ))}
            </div>
          </section>
        ))}
      </div>
    </div>
  )
}
