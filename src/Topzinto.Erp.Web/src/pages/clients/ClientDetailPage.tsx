import { useQuery } from '@tanstack/react-query'
import { useParams, Link } from 'react-router-dom'
import { getClient } from '@/api/clients'
import styles from './ClientDetailPage.module.css'

export function ClientDetailPage() {
  const { id } = useParams<{ id: string }>()
  const { data, isLoading, isError } = useQuery({
    queryKey: ['client', id],
    queryFn: () => getClient(id!),
    enabled: !!id,
  })

  if (isLoading) return <p>Loading...</p>
  if (isError || !data) return <p>Client not found.</p>

  return (
    <div className={styles.page}>
      <nav className={styles.breadcrumb}>
        <Link to="/clients">Clients</Link> &gt; {data.name}
      </nav>
      <header className={styles.header}>
        <h1>{data.name}</h1>
        <span className={styles.badge}>{data.type}</span>
        <div className={styles.headerActions}>
          <Link to={`/clients/${id}/edit`} className={styles.editBtn}>Edit</Link>
        </div>
      </header>

      <div className={styles.grid}>
        <div className={styles.card}>
          <h3>Details</h3>
          <dl>
            <dt>Registration</dt><dd>{data.registrationNumber ?? '—'}</dd>
            <dt>Address</dt><dd>{data.address ?? '—'}</dd>
            <dt>City</dt><dd>{data.city ?? '—'}</dd>
            <dt>Province</dt><dd>{data.province ?? '—'}</dd>
          </dl>
        </div>
        <div className={styles.card}>
          <h3>Contacts</h3>
          {data.contacts.length === 0 ? (
            <p className={styles.empty}>No contacts recorded.</p>
          ) : (
            <ul className={styles.contacts}>
              {data.contacts.map((c) => (
                <li key={c.id}>
                  <strong>{c.name}</strong>
                  {c.isPrimary && <span className={styles.primary}>Primary</span>}
                  <div>{c.title}</div>
                  <div>{c.phone} · {c.email}</div>
                </li>
              ))}
            </ul>
          )}
        </div>
      </div>
    </div>
  )
}
