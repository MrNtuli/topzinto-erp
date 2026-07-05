import { useMemo, useState } from 'react'
import { useQuery } from '@tanstack/react-query'
import { Link } from 'react-router-dom'
import { Plus } from 'lucide-react'
import { getProjects } from '@/api/projects'
import { getComplianceRecords, COMPLIANCE_STATUSES } from '@/api/compliance'
import styles from '../projects/ProjectsPage.module.css'
import localStyles from './ComplianceRecordsPage.module.css'

const PAGE_SIZE = 10

function typeClass(type: string) {
  switch (type) {
    case 'Insurance': return localStyles.typeInsurance
    case 'License': return localStyles.typeLicense
    case 'Certificate': return localStyles.typeCertificate
    case 'Permit': return localStyles.typePermit
    case 'Inspection': return localStyles.typeInspection
    default: return localStyles.typeOther
  }
}

function statusClass(status: string) {
  switch (status) {
    case 'Valid': return localStyles.statusValid
    case 'Expiring Soon': return localStyles.statusExpiringSoon
    case 'Expired': return localStyles.statusExpired
    case 'Pending': return localStyles.statusPending
    case 'Revoked': return localStyles.statusRevoked
    default: return localStyles.statusValid
  }
}

function expiryClass(status: string) {
  if (status === 'Expired') return localStyles.expiryExpired
  if (status === 'Expiring Soon') return localStyles.expiryWarning
  return ''
}

export function ComplianceRecordsPage() {
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
    queryKey: ['compliance-records', status, projectId],
    queryFn: () => getComplianceRecords({
      status: status || undefined,
      projectId: projectId || undefined,
    }),
    retry: false,
  })

  const filtered = useMemo(() => {
    if (!data) return []
    const q = search.trim().toLowerCase()
    if (!q) return data
    return data.filter((r) =>
      r.title.toLowerCase().includes(q)
      || r.type.toLowerCase().includes(q)
      || r.projectName?.toLowerCase().includes(q)
      || r.responsiblePerson?.toLowerCase().includes(q)
      || r.entityType?.toLowerCase().includes(q),
    )
  }, [data, search])

  const totalPages = Math.max(1, Math.ceil(filtered.length / PAGE_SIZE))
  const currentPage = Math.min(page, totalPages)
  const pageItems = filtered.slice((currentPage - 1) * PAGE_SIZE, currentPage * PAGE_SIZE)

  return (
    <div className={styles.page}>
      <nav className={styles.breadcrumb}>Home &gt; Compliance Records</nav>
      <header className={styles.header}>
        <h1>Compliance Records</h1>
        <Link to="/compliance/new" className={styles.newBtn}>
          <Plus size={18} />
          Add Record
        </Link>
      </header>

      <div className={styles.filters}>
        <input
          type="search"
          placeholder="Search title, type, project, responsible person..."
          value={search}
          onChange={(e) => { setSearch(e.target.value); setPage(1) }}
        />
        <select value={status} onChange={(e) => { setStatus(e.target.value); setPage(1) }}>
          <option value="">All Status</option>
          {COMPLIANCE_STATUSES.map((s) => (
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
        {isLoading && <p className={styles.loading}>Loading compliance records...</p>}
        {!isLoading && !isError && filtered.length === 0 && (
          <p className={localStyles.empty}>No compliance records found.</p>
        )}

        <table className={`${styles.table} ${localStyles.desktopTable}`}>
          <thead>
            <tr>
              <th>Title</th>
              <th>Type</th>
              <th>Project</th>
              <th>Issue Date</th>
              <th>Expiry Date</th>
              <th>Status</th>
              <th>Responsible</th>
            </tr>
          </thead>
          <tbody>
            {pageItems.map((r) => (
              <tr key={r.id}>
                <td>
                  <Link to={`/compliance/${r.id}`} className={localStyles.link}>{r.title}</Link>
                </td>
                <td>
                  <span className={`${styles.badge} ${typeClass(r.type)}`}>{r.type}</span>
                </td>
                <td>{r.projectName ?? '—'}</td>
                <td>{r.issueDate}</td>
                <td className={expiryClass(r.status)}>{r.expiryDate ?? '—'}</td>
                <td>
                  <span className={`${styles.badge} ${statusClass(r.status)}`}>{r.status}</span>
                </td>
                <td>{r.responsiblePerson ?? '—'}</td>
              </tr>
            ))}
          </tbody>
        </table>

        <div className={localStyles.mobileCards}>
          {pageItems.map((r) => (
            <article key={r.id} className={localStyles.mobileCard}>
              <div className={localStyles.mobileCardHeader}>
                <Link to={`/compliance/${r.id}`} className={localStyles.link}>
                  <strong>{r.title}</strong>
                </Link>
                <span className={`${styles.badge} ${statusClass(r.status)}`}>{r.status}</span>
              </div>
              <p className={localStyles.mobileMeta}>
                <span className={`${styles.badge} ${typeClass(r.type)}`}>{r.type}</span>
                {r.projectName ? ` · ${r.projectName}` : ''}
              </p>
              <p className={localStyles.mobileMeta}>
                Issued {r.issueDate}
                {r.expiryDate ? ` · Expires ${r.expiryDate}` : ''}
              </p>
              {r.responsiblePerson && (
                <p className={localStyles.mobileMeta}>Responsible: {r.responsiblePerson}</p>
              )}
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

        {isError && <p className={styles.hint}>Start the API to load compliance records.</p>}
      </div>
    </div>
  )
}
