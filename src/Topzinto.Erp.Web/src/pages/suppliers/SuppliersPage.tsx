import { useQuery } from '@tanstack/react-query'
import { useState } from 'react'
import { Link } from 'react-router-dom'
import { Plus } from 'lucide-react'
import { getSuppliers } from '@/api/suppliers'
import styles from '../projects/ProjectsPage.module.css'
import localStyles from '../clients/ClientsPage.module.css'

export function SuppliersPage() {
  const [search, setSearch] = useState('')
  const [status, setStatus] = useState('')

  const { data, isLoading, isError } = useQuery({
    queryKey: ['suppliers', search, status],
    queryFn: () => getSuppliers({ search: search || undefined, status: status || undefined }),
    retry: false,
  })

  return (
    <div className={styles.page}>
      <nav className={styles.breadcrumb}>Home &gt; Suppliers</nav>
      <header className={styles.header}>
        <h1>Suppliers</h1>
        <Link to="/suppliers/new" className={styles.newBtn}>
          <Plus size={18} />
          New Supplier
        </Link>
      </header>

      <div className={styles.filters}>
        <input type="search" placeholder="Search suppliers..." value={search} onChange={(e) => setSearch(e.target.value)} />
        <select value={status} onChange={(e) => setStatus(e.target.value)}>
          <option value="">All Status</option>
          <option value="Active">Active</option>
          <option value="Inactive">Inactive</option>
          <option value="Blacklisted">Blacklisted</option>
        </select>
      </div>

      <div className={styles.tableWrap}>
        {isLoading && <p className={styles.loading}>Loading suppliers...</p>}
        <table className={styles.table}>
          <thead>
            <tr>
              <th>Code</th>
              <th>Name</th>
              <th>Category</th>
              <th>Contact</th>
              <th>Phone</th>
              <th>City</th>
              <th>Status</th>
              <th>POs</th>
            </tr>
          </thead>
          <tbody>
            {data?.map((s) => (
              <tr key={s.id}>
                <td className={styles.name}>{s.code}</td>
                <td>
                  <Link to={`/suppliers/${s.id}`} className={localStyles.link}>{s.name}</Link>
                </td>
                <td><span className={localStyles.typeBadge}>{s.category}</span></td>
                <td>{s.contactPerson ?? '—'}</td>
                <td>{s.phone ?? '—'}</td>
                <td>{s.city ?? '—'}</td>
                <td>
                  <span className={`${styles.badge} ${s.status === 'Active' ? styles.active : styles.completed}`}>
                    {s.status}
                  </span>
                </td>
                <td>{s.purchaseOrderCount}</td>
              </tr>
            ))}
          </tbody>
        </table>
        {isError && <p className={styles.hint}>Start the API to load suppliers.</p>}
      </div>
    </div>
  )
}
