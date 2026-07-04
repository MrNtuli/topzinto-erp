import { useQuery } from '@tanstack/react-query'
import { Link } from 'react-router-dom'
import { Download } from 'lucide-react'
import { getAuditLogs, exportAuditLogsCsv } from '@/api/admin'
import styles from '../projects/ProjectsPage.module.css'

export function AuditLogsPage() {
  const { data, isLoading, isError } = useQuery({
    queryKey: ['audit-logs-full'],
    queryFn: () => getAuditLogs(200),
    retry: false,
  })

  return (
    <div className={styles.page}>
      <nav className={styles.breadcrumb}>
        <Link to="/settings">Settings</Link> &gt; Audit Log
      </nav>
      <header className={styles.header}>
        <h1>Audit Log</h1>
        <button
          type="button"
          className={styles.newBtn}
          onClick={() => exportAuditLogsCsv(1000).catch(() => {})}
        >
          <Download size={18} />
          Export CSV
        </button>
      </header>

      <div className={styles.tableWrap}>
        {isLoading && <p className={styles.loading}>Loading audit log...</p>}
        <table className={styles.table}>
          <thead>
            <tr>
              <th>Timestamp</th>
              <th>User</th>
              <th>Action</th>
              <th>Module</th>
              <th>Entity Type</th>
              <th>Details</th>
            </tr>
          </thead>
          <tbody>
            {data?.map((log) => (
              <tr key={log.id}>
                <td>{log.createdAt}</td>
                <td>{log.userEmail || '—'}</td>
                <td>{log.action}</td>
                <td>{log.module}</td>
                <td>{log.entityType}</td>
                <td>{log.newValues ?? log.entityId}</td>
              </tr>
            ))}
          </tbody>
        </table>
        {isError && <p className={styles.hint}>Start the API to load audit logs.</p>}
      </div>
    </div>
  )
}
