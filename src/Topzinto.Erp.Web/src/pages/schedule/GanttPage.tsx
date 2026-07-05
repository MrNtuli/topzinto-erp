import { useMemo, useState } from 'react'
import { useQuery } from '@tanstack/react-query'
import { Link } from 'react-router-dom'
import { BarChart3, Plus } from 'lucide-react'
import { getGanttData, type GanttMilestone, type GanttTask } from '@/api/schedule'
import { getProjects } from '@/api/projects'
import { ScheduleSubNav } from './ScheduleSubNav'
import styles from './GanttPage.module.css'
import pageStyles from '../projects/ProjectsPage.module.css'

const STATUS_COLORS: Record<string, string> = {
  'Not Started': '#94a3b8',
  'In Progress': '#f26522',
  Completed: '#16a34a',
  Overdue: '#dc2626',
  Pending: '#2563eb',
  Achieved: '#16a34a',
  Missed: '#dc2626',
}

type GanttRow = {
  id: string
  kind: 'task' | 'milestone'
  label: string
  projectName: string
  start: Date
  end: Date
  status: string
  meta: string
}

const DAY_MS = 86_400_000

function parseDate(value: string | null | undefined, fallback?: Date): Date {
  if (value) {
    const parsed = new Date(`${value}T00:00:00`)
    if (!Number.isNaN(parsed.getTime())) return parsed
  }
  return fallback ?? new Date()
}

function addDays(date: Date, days: number) {
  return new Date(date.getTime() + days * DAY_MS)
}

function buildRows(tasks: GanttTask[], milestones: GanttMilestone[]): GanttRow[] {
  const milestoneRows = milestones.map((m) => {
    const end = parseDate(m.endDate)
    const start = m.startDate ? parseDate(m.startDate) : addDays(end, -7)
    return {
      id: `milestone-${m.id}`,
      kind: 'milestone' as const,
      label: m.name,
      projectName: m.projectName,
      start: start <= end ? start : end,
      end,
      status: m.status,
      meta: `Milestone · ${m.progress}% · ${m.status}`,
    }
  })

  const taskRows = tasks.map((t) => {
    const end = t.endDate ? parseDate(t.endDate) : parseDate(t.startDate)
    const start = t.startDate ? parseDate(t.startDate) : addDays(end, -1)
    return {
      id: `task-${t.id}`,
      kind: 'task' as const,
      label: t.title,
      projectName: t.projectName,
      start: start <= end ? start : end,
      end,
      status: t.status,
      meta: `Task · ${t.priority} · ${t.status}`,
    }
  })

  return [...milestoneRows, ...taskRows].sort(
    (a, b) => a.start.getTime() - b.start.getTime() || a.label.localeCompare(b.label),
  )
}

function computeRange(rows: GanttRow[]) {
  if (!rows.length) return null

  const min = rows.reduce((acc, row) => (row.start < acc ? row.start : acc), rows[0].start)
  const max = rows.reduce((acc, row) => (row.end > acc ? row.end : acc), rows[0].end)
  const paddedStart = addDays(min, -3)
  const paddedEnd = addDays(max, 3)
  const duration = Math.max(paddedEnd.getTime() - paddedStart.getTime(), DAY_MS)

  return { start: paddedStart, end: paddedEnd, duration }
}

function barStyle(row: GanttRow, range: { start: Date; duration: number }) {
  const left = ((row.start.getTime() - range.start.getTime()) / range.duration) * 100
  const width = Math.max(((row.end.getTime() - row.start.getTime()) / range.duration) * 100, 1.5)
  return {
    left: `${Math.max(0, Math.min(left, 99))}%`,
    width: `${Math.min(width, 100 - left)}%`,
    background: STATUS_COLORS[row.status] ?? (row.kind === 'milestone' ? '#2563eb' : '#f26522'),
  }
}

function formatShortDate(date: Date) {
  return date.toLocaleDateString('en-ZA', { day: 'numeric', month: 'short' })
}

function buildMonthMarkers(range: { start: Date; end: Date; duration: number }) {
  const markers: { label: string; left: number }[] = []
  const cursor = new Date(range.start.getFullYear(), range.start.getMonth(), 1)
  if (cursor < range.start) cursor.setMonth(cursor.getMonth() + 1)

  while (cursor <= range.end) {
    const left = ((cursor.getTime() - range.start.getTime()) / range.duration) * 100
    if (left >= 0 && left <= 100) {
      markers.push({
        label: cursor.toLocaleDateString('en-ZA', { month: 'short', year: '2-digit' }),
        left,
      })
    }
    cursor.setMonth(cursor.getMonth() + 1)
  }

  return markers
}

export function GanttPage() {
  const [projectId, setProjectId] = useState('')

  const { data: projects } = useQuery({
    queryKey: ['projects'],
    queryFn: () => getProjects(),
  })

  const { data, isLoading, isError } = useQuery({
    queryKey: ['schedule-gantt', projectId || 'all'],
    queryFn: () => getGanttData(projectId || undefined),
    retry: false,
  })

  const rows = useMemo(
    () => buildRows(data?.tasks ?? [], data?.milestones ?? []),
    [data],
  )
  const range = useMemo(() => computeRange(rows), [rows])
  const monthMarkers = useMemo(() => (range ? buildMonthMarkers(range) : []), [range])

  return (
    <div className={styles.page}>
      <nav className={styles.breadcrumb}>Home &gt; Schedule &gt; Gantt</nav>
      <header className={styles.header}>
        <div>
          <h1>Programme Gantt</h1>
          <p className={styles.subtitle}>Tasks and milestones across active projects</p>
        </div>
        <div className={styles.headerActions}>
          <label className={styles.filter}>
            <span>Project</span>
            <select
              value={projectId}
              onChange={(e) => setProjectId(e.target.value)}
              aria-label="Filter by project"
            >
              <option value="">All projects</option>
              {projects?.map((p) => (
                <option key={p.id} value={p.id}>{p.name}</option>
              ))}
            </select>
          </label>
          <Link to="/schedule/new-task" className={pageStyles.newBtn}>
            <Plus size={18} /> Add Task
          </Link>
        </div>
      </header>

      <ScheduleSubNav />

      {isLoading && (
        <div className={styles.stateCard}>
          <p>Loading Gantt data...</p>
        </div>
      )}

      {isError && (
        <div className={styles.stateCard}>
          <p className={styles.error}>Could not load Gantt data. Start the API or try again later.</p>
        </div>
      )}

      {!isLoading && !isError && rows.length === 0 && (
        <div className={styles.stateCard}>
          <div className={styles.empty}>
            <BarChart3 size={36} strokeWidth={1.5} />
            <p>No schedule items to display.</p>
            <span>Add tasks or milestones to projects to build the programme chart.</span>
          </div>
        </div>
      )}

      {!isLoading && !isError && rows.length > 0 && range && (
        <>
          <div className={styles.legend}>
            {Object.entries(STATUS_COLORS).map(([status, color]) => (
              <span key={status}><i style={{ background: color }} /> {status}</span>
            ))}
            <span><i className={styles.milestoneDot} /> Milestone</span>
          </div>

          <div className={styles.mobileList}>
            {rows.map((row) => (
              <article key={row.id} className={styles.mobileCard}>
                <div className={styles.mobileCardHeader}>
                  <strong>{row.label}</strong>
                  <span
                    className={styles.statusBadge}
                    style={{ background: `${STATUS_COLORS[row.status] ?? '#f26522'}22`, color: STATUS_COLORS[row.status] ?? '#f26522' }}
                  >
                    {row.status}
                  </span>
                </div>
                <span className={styles.mobileMeta}>{row.projectName}</span>
                <span className={styles.mobileDates}>
                  {formatShortDate(row.start)} – {formatShortDate(row.end)}
                </span>
                <span className={styles.mobileMeta}>{row.meta}</span>
              </article>
            ))}
          </div>

          <div className={styles.chartWrap}>
            <div className={styles.chartScroll}>
              <div className={styles.chart}>
                <div className={styles.chartHeader}>
                  <div className={styles.labelCol}>Item</div>
                  <div className={styles.timelineCol}>
                    <div className={styles.monthRow}>
                      {monthMarkers.map((marker) => (
                        <span
                          key={`${marker.label}-${marker.left}`}
                          className={styles.monthLabel}
                          style={{ left: `${marker.left}%` }}
                        >
                          {marker.label}
                        </span>
                      ))}
                    </div>
                  </div>
                </div>

                {rows.map((row) => {
                  const style = barStyle(row, range)
                  return (
                    <div key={row.id} className={styles.row}>
                      <div className={styles.labelCol}>
                        <strong>{row.label}</strong>
                        <span>{row.projectName}</span>
                        <span className={styles.rowMeta}>{row.meta}</span>
                      </div>
                      <div className={styles.timelineCol}>
                        <div
                          className={row.kind === 'milestone' ? styles.barMilestone : styles.bar}
                          style={style}
                          title={`${row.label}: ${formatShortDate(row.start)} – ${formatShortDate(row.end)}`}
                        >
                          <span className={styles.barLabel}>{row.status}</span>
                        </div>
                      </div>
                    </div>
                  )
                })}
              </div>
            </div>
          </div>
        </>
      )}
    </div>
  )
}
