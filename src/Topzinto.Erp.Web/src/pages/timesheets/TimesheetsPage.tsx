import { useQuery } from '@tanstack/react-query'
import { useState } from 'react'
import { Link } from 'react-router-dom'
import { Plus } from 'lucide-react'
import { getTimesheets, getLabourSummary } from '@/api/timesheets'
import { getProjects } from '@/api/projects'
import { formatCurrency } from '@/api/boq'
import styles from '../projects/ProjectsPage.module.css'
import kpiStyles from '../fleet/FleetPage.module.css'
import localStyles from '../clients/ClientsPage.module.css'

export function TimesheetsPage() {
  const [projectId, setProjectId] = useState('')
  const [status, setStatus] = useState('')

  const { data: projects } = useQuery({ queryKey: ['projects'], queryFn: () => getProjects() })
  const { data: labour } = useQuery({
    queryKey: ['labour-summary', projectId],
    queryFn: () => getLabourSummary(projectId || undefined),
    retry: false,
  })
  const { data, isLoading, isError } = useQuery({
    queryKey: ['timesheets', projectId, status],
    queryFn: () => getTimesheets({
      projectId: projectId || undefined,
      status: status || undefined,
    }),
    retry: false,
  })

  const totalHours = data?.reduce((sum, t) => sum + t.hours, 0) ?? 0
  const totalCost = data?.reduce((sum, t) => sum + (t.labourCost ?? 0), 0) ?? 0
  const pending = data?.filter((t) => t.status === 'Submitted').length ?? 0

  return (
    <div className={styles.page}>
      <nav className={styles.breadcrumb}>Home &gt; Timesheets</nav>
      <header className={styles.header}>
        <h1>Timesheets &amp; Labour</h1>
        <Link to="/timesheets/new" className={styles.newBtn}>
          <Plus size={18} />
          Log Hours
        </Link>
      </header>

      <div className={kpiStyles.kpiRow}>
        <div className={kpiStyles.kpi}><span>{totalHours.toFixed(1)}</span><label>Total Hours (filtered)</label></div>
        <div className={kpiStyles.kpi}><span>{formatCurrency(totalCost)}</span><label>Labour Cost</label></div>
        <div className={kpiStyles.kpi}><span className={kpiStyles.orange}>{pending}</span><label>Pending Approval</label></div>
        <div className={kpiStyles.kpi}><span>{data?.length ?? '—'}</span><label>Entries</label></div>
      </div>

      <h2 className={styles.hint} style={{ marginBottom: 12, fontWeight: 600, color: 'var(--color-navy)' }}>Labour by Project</h2>
      <div className={styles.tableWrap} style={{ marginBottom: 24 }}>
        <table className={styles.table}>
          <thead>
            <tr>
              <th>Project</th>
              <th>Total Hours</th>
              <th>Labour Cost</th>
              <th>Entries</th>
            </tr>
          </thead>
          <tbody>
            {labour?.map((row) => (
              <tr key={row.projectId}>
                <td className={styles.name}>
                  <Link to={`/projects/${row.projectId}`} className={localStyles.link}>{row.projectName}</Link>
                </td>
                <td>{row.totalHours.toFixed(1)}</td>
                <td className={styles.value}>{formatCurrency(row.totalLabourCost)}</td>
                <td>{row.entryCount}</td>
              </tr>
            ))}
          </tbody>
        </table>
        {!labour?.length && <p className={styles.loading}>No labour data yet.</p>}
      </div>

      <div className={styles.filters}>
        <select value={projectId} onChange={(e) => setProjectId(e.target.value)}>
          <option value="">All Projects</option>
          {projects?.map((p) => (
            <option key={p.id} value={p.id}>{p.code} — {p.name}</option>
          ))}
        </select>
        <select value={status} onChange={(e) => setStatus(e.target.value)}>
          <option value="">All Status</option>
          <option value="Draft">Draft</option>
          <option value="Submitted">Submitted</option>
          <option value="Approved">Approved</option>
        </select>
      </div>

      <div className={styles.tableWrap}>
        {isLoading && <p className={styles.loading}>Loading timesheets...</p>}
        {isError && <p className={styles.loading}>Could not load timesheets.</p>}
        <table className={styles.table}>
          <thead>
            <tr>
              <th>Date</th>
              <th>Employee</th>
              <th>Project</th>
              <th>Hours</th>
              <th>Description</th>
              <th>Cost</th>
              <th>Status</th>
            </tr>
          </thead>
          <tbody>
            {data?.map((t) => (
              <tr key={t.id}>
                <td className={styles.name}>
                  <Link to={`/timesheets/${t.id}/edit`} className={localStyles.link}>{t.workDate}</Link>
                </td>
                <td className={styles.name}>{t.employeeName}</td>
                <td>{t.projectName}</td>
                <td>{t.hours}</td>
                <td>{t.description ?? '—'}</td>
                <td>{t.labourCost != null ? formatCurrency(t.labourCost) : '—'}</td>
                <td><span className={styles.badge}>{t.status}</span></td>
              </tr>
            ))}
          </tbody>
        </table>
        {!isLoading && !isError && data?.length === 0 && (
          <p className={styles.loading}>No timesheet entries found.</p>
        )}
      </div>
    </div>
  )
}
