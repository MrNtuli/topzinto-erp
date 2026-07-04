import { useQuery } from '@tanstack/react-query'
import { useState } from 'react'
import { Link } from 'react-router-dom'
import { Plus } from 'lucide-react'
import { getDocumentsSummary, getDocuments, downloadDocumentFile } from '@/api/documents'
import styles from '../projects/ProjectsPage.module.css'
import kpiStyles from '../fleet/FleetPage.module.css'

export function DocumentsPage() {
  const [search, setSearch] = useState('')
  const [expiringOnly, setExpiringOnly] = useState(false)

  const { data: summary } = useQuery({ queryKey: ['documents-summary'], queryFn: getDocumentsSummary, retry: false })
  const { data, isLoading, isError } = useQuery({
    queryKey: ['documents', search, expiringOnly],
    queryFn: () => getDocuments({ search: search || undefined, expiringOnly: expiringOnly || undefined }),
    retry: false,
  })

  return (
    <div className={styles.page}>
      <nav className={styles.breadcrumb}>Home &gt; Documents</nav>
      <header className={styles.header}>
        <h1>Document Library</h1>
        <Link to="/documents/new" className={styles.newBtn}>
          <Plus size={18} />
          Register Document
        </Link>
      </header>

      <div className={kpiStyles.kpiRow} style={{ gridTemplateColumns: 'repeat(3, 1fr)' }}>
        <div className={kpiStyles.kpi}><span>{summary?.total ?? '—'}</span><label>Total Documents</label></div>
        <div className={kpiStyles.kpi}><span className={kpiStyles.yellow}>{summary?.expiringSoon ?? '—'}</span><label>Expiring Soon</label></div>
        <div className={kpiStyles.kpi}><span className={kpiStyles.red}>{summary?.expired ?? '—'}</span><label>Expired</label></div>
      </div>

      <p className={styles.hint} style={{ marginBottom: 16 }}>
        Document register with optional file attachments (local storage in dev).
      </p>

      <div className={styles.filters}>
        <input type="search" placeholder="Search documents..." value={search} onChange={(e) => setSearch(e.target.value)} />
        <label style={{ display: 'flex', alignItems: 'center', gap: 8 }}>
          <input type="checkbox" checked={expiringOnly} onChange={(e) => setExpiringOnly(e.target.checked)} />
          Expiring within 30 days
        </label>
      </div>

      <div className={styles.tableWrap}>
        {isLoading && <p className={styles.loading}>Loading documents...</p>}
        <table className={styles.table}>
          <thead>
            <tr>
              <th>Title</th>
              <th>Category</th>
              <th>Linked To</th>
              <th>File</th>
              <th>Version</th>
              <th>Status</th>
              <th>Expiry</th>
            </tr>
          </thead>
          <tbody>
            {data?.map((doc) => (
              <tr key={doc.id} className={doc.isExpiringSoon ? kpiStyles.expiring : undefined}>
                <td className={styles.name}>{doc.title}</td>
                <td>{doc.category}</td>
                <td>{doc.parentType}{doc.parentName ? ` · ${doc.parentName}` : ''}</td>
                <td>
                  {doc.hasFile ? (
                    <button
                      type="button"
                      className={styles.linkBtn}
                      onClick={() => downloadDocumentFile(doc.id, doc.fileName)}
                    >
                      {doc.fileName}
                    </button>
                  ) : (doc.fileName ?? '—')}
                </td>
                <td>v{doc.version}</td>
                <td>
                  <span className={`${styles.badge} ${doc.status === 'Expired' ? kpiStyles.maintBadge : styles.active}`}>
                    {doc.status}
                  </span>
                </td>
                <td>{doc.expiryDate ?? '—'}</td>
              </tr>
            ))}
          </tbody>
        </table>
        {isError && <p className={styles.hint}>Start the API to load documents.</p>}
      </div>
    </div>
  )
}
