import { useMemo } from 'react'
import { useQuery } from '@tanstack/react-query'
import { Activity } from 'lucide-react'
import { getProjectActivity, type ProjectActivityItem } from '@/api/projects'
import styles from './ProjectDetailPage.module.css'

const ACTION_COLORS: Record<string, string> = {
  Create: '#16a34a',
  Update: '#f26522',
  Delete: '#dc2626',
}

const ENTITY_LABELS: Record<string, string> = {
  Project: 'Project',
  ProjectTask: 'Task',
  ProjectMilestone: 'Milestone',
  SiteReport: 'Site Report',
  BoqItem: 'BOQ Item',
  Claim: 'Claim',
  DocumentRecord: 'Document',
  Document: 'Document',
}

function formatEntityType(entityType: string) {
  return ENTITY_LABELS[entityType] ?? entityType
}

function formatTimestamp(value: string) {
  const date = new Date(value)
  return {
    day: date.toLocaleDateString('en-ZA', { day: 'numeric', month: 'short' }),
    time: date.toLocaleTimeString('en-ZA', { hour: '2-digit', minute: '2-digit' }),
    monthKey: value.slice(0, 7),
  }
}

function groupByMonth(items: ProjectActivityItem[]) {
  return items.reduce<Record<string, ProjectActivityItem[]>>((acc, item) => {
    const month = item.createdAt.slice(0, 7)
    if (!acc[month]) acc[month] = []
    acc[month].push(item)
    return acc
  }, {})
}

interface ProjectActivityFeedProps {
  projectId: string
}

export function ProjectActivityFeed({ projectId }: ProjectActivityFeedProps) {
  const { data, isLoading, isError } = useQuery({
    queryKey: ['project-activity', projectId],
    queryFn: () => getProjectActivity(projectId),
    enabled: !!projectId,
    retry: false,
  })

  const grouped = useMemo(() => groupByMonth(data ?? []), [data])
  const months = useMemo(() => Object.keys(grouped).sort((a, b) => b.localeCompare(a)), [grouped])

  if (isLoading) {
    return (
      <div className={styles.activityCard}>
        <p className={styles.activityHint}>Loading project activity...</p>
      </div>
    )
  }

  if (isError) {
    return (
      <div className={styles.activityCard}>
        <p className={styles.activityError}>Could not load activity. Start the API or try again later.</p>
      </div>
    )
  }

  if (!data?.length) {
    return (
      <div className={styles.activityCard}>
        <div className={styles.activityEmpty}>
          <Activity size={32} strokeWidth={1.5} />
          <p>No activity recorded for this project yet.</p>
          <span>Changes to the project, schedule, BOQ, claims, documents and site reports will appear here.</span>
        </div>
      </div>
    )
  }

  return (
    <div className={styles.activityFeed}>
      <div className={styles.activityHeader}>
        <h3>Activity Timeline</h3>
        {data.length >= 100 && (
          <span className={styles.activityMeta}>Showing latest 100 events</span>
        )}
      </div>

      {months.map((month) => (
        <section key={month} className={styles.activityMonth}>
          <h4>
            {new Date(`${month}-01`).toLocaleDateString('en-ZA', { month: 'long', year: 'numeric' })}
          </h4>
          <div className={styles.activityList}>
            {grouped[month]?.map((item) => {
              const { day, time } = formatTimestamp(item.createdAt)
              const actionColor = ACTION_COLORS[item.action] ?? 'var(--color-navy)'

              return (
                <article
                  key={item.id}
                  className={styles.activityItem}
                  style={{ borderLeftColor: actionColor }}
                >
                  <div className={styles.activityWhen}>
                    <span className={styles.activityDay}>{day}</span>
                    <span className={styles.activityTime}>{time}</span>
                  </div>
                  <div className={styles.activityBody}>
                    <div className={styles.activityTitleRow}>
                      <span className={styles.activityAction} style={{ color: actionColor }}>
                        {item.action}
                      </span>
                      <span className={styles.activityEntity}>{formatEntityType(item.entityType)}</span>
                    </div>
                    <p className={styles.activitySummary}>{item.summary}</p>
                    <span className={styles.activityMeta}>
                      {item.module} · {item.userEmail || 'System'}
                    </span>
                  </div>
                </article>
              )
            })}
          </div>
        </section>
      ))}
    </div>
  )
}
