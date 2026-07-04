import { useQuery } from '@tanstack/react-query'
import { Link } from 'react-router-dom'
import { Plus } from 'lucide-react'
import { getSiteReports } from '@/api/siteReports'
import styles from '../projects/ProjectsPage.module.css'

export function SiteReportsPage() {
  const { data, isLoading, isError } = useQuery({
    queryKey: ['site-reports'],
    queryFn: () => getSiteReports(),
    retry: false,
  })

  return (
    <div className={styles.page}>
      <nav className={styles.breadcrumb}>Home &gt; Site Reports</nav>
      <header className={styles.header}>
        <h1>Site Reports</h1>
        <Link to="/site-reports/new" className={styles.newBtn}>
          <Plus size={18} />
          Add Site Report
        </Link>
      </header>

      <div className={styles.tableWrap}>
        {isLoading && <p className={styles.loading}>Loading...</p>}
        <table className={styles.table}>
          <thead>
            <tr>
              <th>Date</th>
              <th>Project</th>
              <th>Weather</th>
              <th>Status</th>
              <th>Submitted By</th>
              <th>Summary</th>
            </tr>
          </thead>
          <tbody>
            {data?.map((r) => (
              <tr key={r.id}>
                <td><Link to={`/site-reports/${r.id}`} className={styles.name}>{r.reportDate}</Link></td>
                <td className={styles.name}>{r.projectName}</td>
                <td>{r.weather ?? '—'}</td>
                <td>
                  <span className={`${styles.badge} ${r.status === 'Submitted' ? styles.active : styles.completed}`}>
                    {r.status}
                  </span>
                </td>
                <td>{r.submittedByName ?? '—'}</td>
                <td>{r.workCompletedPreview}</td>
              </tr>
            ))}
          </tbody>
        </table>
        {isError && <p className={styles.hint}>Start the API to load site reports.</p>}
      </div>
    </div>
  )
}
