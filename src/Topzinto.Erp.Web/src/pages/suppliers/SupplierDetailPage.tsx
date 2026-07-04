import { useQuery } from '@tanstack/react-query'
import { useParams, Link } from 'react-router-dom'
import { getSupplier } from '@/api/suppliers'
import { formatCurrency } from '@/api/procurement'
import styles from '../clients/ClientDetailPage.module.css'

export function SupplierDetailPage() {
  const { id } = useParams<{ id: string }>()
  const { data, isLoading, isError } = useQuery({
    queryKey: ['supplier', id],
    queryFn: () => getSupplier(id!),
    enabled: !!id,
    retry: false,
  })

  if (isLoading) return <p>Loading...</p>
  if (isError || !data) return <p>Supplier not found.</p>

  return (
    <div className={styles.page}>
      <nav className={styles.breadcrumb}><Link to="/suppliers">Suppliers</Link> &gt; {data.name}</nav>
      <header className={styles.header}>
        <h1>{data.name}</h1>
        <span className={styles.badge}>{data.status}</span>
        <div className={styles.headerActions}>
          <Link to={`/suppliers/${id}/edit`} className={styles.editBtn}>Edit</Link>
        </div>
      </header>

      <div className={styles.grid}>
        <div className={styles.card}>
          <h3>Supplier Details</h3>
          <dl>
            <dt>Code</dt><dd>{data.code}</dd>
            <dt>Category</dt><dd>{data.category}</dd>
            <dt>Contact</dt><dd>{data.contactPerson ?? '—'}</dd>
            <dt>Phone</dt><dd>{data.phone ?? '—'}</dd>
            <dt>Email</dt><dd>{data.email ?? '—'}</dd>
            <dt>VAT Number</dt><dd>{data.vatNumber ?? '—'}</dd>
          </dl>
        </div>
        <div className={styles.card}>
          <h3>Address</h3>
          <dl>
            <dt>Address</dt><dd>{data.address ?? '—'}</dd>
            <dt>City</dt><dd>{data.city ?? '—'}</dd>
            <dt>Province</dt><dd>{data.province ?? '—'}</dd>
          </dl>
        </div>
        <div className={styles.card}>
          <h3>Recent Purchase Orders</h3>
          {data.recentOrders.length === 0 ? (
            <p className={styles.empty}>No purchase orders.</p>
          ) : (
            <ul className={styles.contacts}>
              {data.recentOrders.map((po) => (
                <li key={po.id}>
                  <Link to={`/procurement/${po.id}`}><strong>{po.poNumber}</strong></Link> — {po.title}
                  <div>{po.status} · {formatCurrency(po.totalAmount)} · {po.orderDate}</div>
                </li>
              ))}
            </ul>
          )}
        </div>
      </div>
    </div>
  )
}
