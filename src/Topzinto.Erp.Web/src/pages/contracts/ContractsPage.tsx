import { useQuery } from '@tanstack/react-query'
import { useState } from 'react'
import { Link } from 'react-router-dom'
import { Plus } from 'lucide-react'
import { getContracts, formatCurrency } from '@/api/contracts'
import styles from '../projects/ProjectsPage.module.css'
import localStyles from '../clients/ClientsPage.module.css'

export function ContractsPage() {
  const [search, setSearch] = useState('')

  const { data, isLoading, isError } = useQuery({
    queryKey: ['contracts', search],
    queryFn: () => getContracts({ search: search || undefined }),
    retry: false,
  })

  return (
    <div className={styles.page}>
      <nav className={styles.breadcrumb}>Home &gt; Contracts</nav>
      <header className={styles.header}>
        <h1>Contracts</h1>
        <Link to="/contracts/new" className={styles.newBtn}>
          <Plus size={18} />
          New Contract
        </Link>
      </header>
      <div className={styles.filters}>
        <input type="search" placeholder="Search contracts..." value={search} onChange={(e) => setSearch(e.target.value)} />
      </div>
      <div className={styles.tableWrap}>
        {isLoading && <p className={styles.loading}>Loading...</p>}
        <table className={styles.table}>
          <thead>
            <tr>
              <th>Contract No.</th>
              <th>Title</th>
              <th>Client</th>
              <th>Project</th>
              <th>Status</th>
              <th>Value</th>
              <th>End Date</th>
            </tr>
          </thead>
          <tbody>
            {data?.map((c) => (
              <tr key={c.id}>
                <td className={styles.name}>
                  <Link to={`/contracts/${c.id}`} className={localStyles.link}>{c.contractNumber}</Link>
                </td>
                <td>{c.title}</td>
                <td>{c.clientName}</td>
                <td>{c.projectName ?? '—'}</td>
                <td><span className={`${styles.badge} ${styles.active}`}>{c.status}</span></td>
                <td className={styles.value}>{formatCurrency(c.value)}</td>
                <td>{c.endDate ?? '—'}</td>
              </tr>
            ))}
          </tbody>
        </table>
        {isError && <p className={styles.hint}>Start the API to load contracts.</p>}
      </div>
    </div>
  )
}
