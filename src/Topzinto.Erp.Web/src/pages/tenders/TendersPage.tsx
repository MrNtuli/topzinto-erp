import { useQuery } from '@tanstack/react-query'
import { useState } from 'react'
import { Link } from 'react-router-dom'
import { Plus } from 'lucide-react'
import { getTenders, formatCurrency } from '@/api/tenders'
import styles from '../projects/ProjectsPage.module.css'
import localStyles from '../clients/ClientsPage.module.css'

export function TendersPage() {
  const [search, setSearch] = useState('')

  const { data, isLoading, isError } = useQuery({
    queryKey: ['tenders', search],
    queryFn: () => getTenders({ search: search || undefined }),
    retry: false,
  })

  return (
    <div className={styles.page}>
      <nav className={styles.breadcrumb}>Home &gt; Tenders</nav>
      <header className={styles.header}>
        <h1>Tenders</h1>
        <Link to="/tenders/new" className={styles.newBtn}>
          <Plus size={18} />
          New Tender
        </Link>
      </header>
      <div className={styles.filters}>
        <input type="search" placeholder="Search tenders..." value={search} onChange={(e) => setSearch(e.target.value)} />
      </div>
      <div className={styles.tableWrap}>
        {isLoading && <p className={styles.loading}>Loading...</p>}
        <table className={styles.table}>
          <thead>
            <tr>
              <th>Reference</th>
              <th>Title</th>
              <th>Client</th>
              <th>Status</th>
              <th>Closing Date</th>
              <th>Est. Value</th>
            </tr>
          </thead>
          <tbody>
            {data?.map((t) => (
              <tr key={t.id}>
                <td className={styles.name}>
                  <Link to={`/tenders/${t.id}`} className={localStyles.link}>{t.referenceNumber}</Link>
                </td>
                <td>{t.title}</td>
                <td>{t.clientName}</td>
                <td><span className={`${styles.badge} ${styles.active}`}>{t.status}</span></td>
                <td>{t.closingDate}</td>
                <td className={styles.value}>{formatCurrency(t.estimatedValue)}</td>
              </tr>
            ))}
          </tbody>
        </table>
        {isError && <p className={styles.hint}>Start the API to load tenders.</p>}
      </div>
    </div>
  )
}
