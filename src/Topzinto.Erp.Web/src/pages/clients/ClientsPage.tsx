import { useQuery } from '@tanstack/react-query'
import { useState } from 'react'
import { Link } from 'react-router-dom'
import { Plus } from 'lucide-react'
import { getClients } from '@/api/clients'
import styles from '../projects/ProjectsPage.module.css'
import localStyles from './ClientsPage.module.css'

export function ClientsPage() {
  const [search, setSearch] = useState('')

  const { data, isLoading, isError } = useQuery({
    queryKey: ['clients', search],
    queryFn: () => getClients(search || undefined),
    retry: false,
  })

  return (
    <div className={styles.page}>
      <nav className={styles.breadcrumb}>Home &gt; Clients</nav>
      <header className={styles.header}>
        <h1>Clients</h1>
        <Link to="/clients/new" className={styles.newBtn}>
          <Plus size={18} />
          New Client
        </Link>
      </header>

      <div className={styles.filters}>
        <input
          type="search"
          placeholder="Search clients..."
          value={search}
          onChange={(e) => setSearch(e.target.value)}
        />
      </div>

      <div className={styles.tableWrap}>
        {isLoading && <p className={styles.loading}>Loading clients...</p>}
        <table className={styles.table}>
          <thead>
            <tr>
              <th>Name</th>
              <th>Type</th>
              <th>City</th>
              <th>Province</th>
              <th>Projects</th>
              <th>Primary Contact</th>
            </tr>
          </thead>
          <tbody>
            {data?.map((c) => (
              <tr key={c.id}>
                <td>
                  <Link to={`/clients/${c.id}`} className={localStyles.link}>
                    {c.name}
                  </Link>
                </td>
                <td><span className={localStyles.typeBadge}>{c.type}</span></td>
                <td>{c.city ?? '—'}</td>
                <td>{c.province ?? '—'}</td>
                <td>{c.projectCount}</td>
                <td>{c.primaryContact ?? '—'}</td>
              </tr>
            ))}
          </tbody>
        </table>
        {isError && <p className={styles.hint}>Start the API to load clients.</p>}
      </div>
    </div>
  )
}
