import { useMemo, useState } from 'react'
import { useQuery } from '@tanstack/react-query'
import { Link } from 'react-router-dom'
import { Plus } from 'lucide-react'
import { getProjects } from '@/api/projects'
import { getSafetyIncidents, SAFETY_STATUSES } from '@/api/safety'
import styles from '../projects/ProjectsPage.module.css'
import localStyles from './SafetyIncidentsPage.module.css'

const PAGE_SIZE = 10

function severityClass(severity: string) {
  switch (severity) {
    case 'Low': return localStyles.severityLow
    case 'High': return localStyles.severityHigh
    case 'Critical': return localStyles.severityCritical
    default: return localStyles.severityMedium
  }
}

function statusClass(status: string) {
  switch (status) {
    case 'Reported': return localStyles.statusReported
    case 'Investigating': return localStyles.statusInvestigating
    case 'Resolved': return localStyles.statusResolved
    case 'Closed': return localStyles.statusClosed
    default: return localStyles.statusReported
  }
}

export function SafetyIncidentsPage() {
  const [search, setSearch] = useState('')
  const [status, setStatus] = useState('')
  const [projectId, setProjectId] = useState('')
  const [page, setPage] = useState(1)

  const { data: projects } = useQuery({
    queryKey: ['projects'],
    queryFn: () => getProjects(),
    retry: false,
  })

  const { data, isLoading, isError } = useQuery({
    queryKey: ['safety-incidents', status, projectId],
    queryFn: () => getSafetyIncidents({
      status: status || undefined,
      projectId: projectId || undefined,
    }),
    retry: false,
  })

  const filtered = useMemo(() => {
    if (!data) return []
    const q = search.trim().toLowerCase()
    if (!q) return data
    return data.filter((i) =>
      i.title.toLowerCase().includes(q)
      || i.projectName.toLowerCase().includes(q)
      || i.location?.toLowerCase().includes(q)
      || i.reportedByName?.toLowerCase().includes(q),
    )
  }, [data, search])

  const totalPages = Math.max(1, Math.ceil(filtered.length / PAGE_SIZE))
  const currentPage = Math.min(page, totalPages)
  const pageItems = filtered.slice((currentPage - 1) * PAGE_SIZE, currentPage * PAGE_SIZE)

  return (
    <div className={styles.page}>
      <nav className={styles.breadcrumb}>Home &gt; Safety Incidents</nav>
      <header className={styles.header}>
        <h1>Safety Incidents</h1>
        <Link to="/safety/new" className={styles.newBtn}>
          <Plus size={18} />
          Report Incident
        </Link>
      </header>

      <div className={styles.filters}>
        <input
          type="search"
          placeholder="Search title, project, location..."
          value={search}
          onChange={(e) => { setSearch(e.target.value); setPage(1) }}
        />
        <select value={status} onChange={(e) => { setStatus(e.target.value); setPage(1) }}>
          <option value="">All Status</option>
          {SAFETY_STATUSES.map((s) => (
            <option key={s} value={s}>{s}</option>
          ))}
        </select>
        <select value={projectId} onChange={(e) => { setProjectId(e.target.value); setPage(1) }}>
          <option value="">All Projects</option>
          {projects?.map((p) => (
            <option key={p.id} value={p.id}>{p.name}</option>
          ))}
        </select>
      </div>

      <div className={styles.tableWrap}>
        {isLoading && <p className={styles.loading}>Loading safety incidents...</p>}
        {!isLoading && !isError && filtered.length === 0 && (
          <p className={localStyles.empty}>No safety incidents found.</p>
        )}

        <table className={`${styles.table} ${localStyles.desktopTable}`}>
          <thead>
            <tr>
              <th>Date</th>
              <th>Title</th>
              <th>Project</th>
              <th>Severity</th>
              <th>Status</th>
              <th>Location</th>
              <th>Reported By</th>
            </tr>
          </thead>
          <tbody>
            {pageItems.map((i) => (
              <tr key={i.id}>
                <td>
                  <Link to={`/safety/${i.id}`} className={localStyles.link}>{i.incidentDate}</Link>
                </td>
                <td>
                  <Link to={`/safety/${i.id}`} className={localStyles.link}>{i.title}</Link>
                </td>
                <td>{i.projectName}</td>
                <td>
                  <span className={`${styles.badge} ${severityClass(i.severity)}`}>{i.severity}</span>
                </td>
                <td>
                  <span className={`${styles.badge} ${statusClass(i.status)}`}>{i.status}</span>
                </td>
                <td>{i.location ?? '—'}</td>
                <td>{i.reportedByName ?? '—'}</td>
              </tr>
            ))}
          </tbody>
        </table>

        <div className={localStyles.mobileCards}>
          {pageItems.map((i) => (
            <article key={i.id} className={localStyles.mobileCard}>
              <div className={localStyles.mobileCardHeader}>
                <Link to={`/safety/${i.id}`} className={localStyles.link}>
                  <strong>{i.title}</strong>
                </Link>
                <span className={`${styles.badge} ${severityClass(i.severity)}`}>{i.severity}</span>
              </div>
              <p className={localStyles.mobileMeta}>{i.incidentDate} · {i.projectName}</p>
              <p className={localStyles.mobileMeta}>
                <span className={`${styles.badge} ${statusClass(i.status)}`}>{i.status}</span>
                {i.location ? ` · ${i.location}` : ''}
              </p>
            </article>
          ))}
        </div>

        {filtered.length > PAGE_SIZE && (
          <div className={localStyles.pagination}>
            <span>
              Showing {(currentPage - 1) * PAGE_SIZE + 1}–{Math.min(currentPage * PAGE_SIZE, filtered.length)} of {filtered.length}
            </span>
            <div>
              <button type="button" disabled={currentPage <= 1} onClick={() => setPage((p) => p - 1)}>Previous</button>
              {' '}
              <button type="button" disabled={currentPage >= totalPages} onClick={() => setPage((p) => p + 1)}>Next</button>
            </div>
          </div>
        )}

        {isError && <p className={styles.hint}>Start the API to load safety incidents.</p>}
      </div>
    </div>
  )
}
