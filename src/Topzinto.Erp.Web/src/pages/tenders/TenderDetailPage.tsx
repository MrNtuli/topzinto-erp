import { useQuery } from '@tanstack/react-query'
import { useParams, Link } from 'react-router-dom'
import { getTender, formatCurrency } from '@/api/tenders'
import styles from '../clients/ClientDetailPage.module.css'

export function TenderDetailPage() {
  const { id } = useParams<{ id: string }>()
  const { data, isLoading, isError } = useQuery({
    queryKey: ['tender', id],
    queryFn: () => getTender(id!),
    enabled: !!id,
  })

  if (isLoading) return <p>Loading...</p>
  if (isError || !data) return <p>Tender not found.</p>

  return (
    <div className={styles.page}>
      <nav className={styles.breadcrumb}><Link to="/tenders">Tenders</Link> &gt; {data.referenceNumber}</nav>
      <header className={styles.header}>
        <h1>{data.title}</h1>
        <span className={styles.badge}>{data.status}</span>
        <div className={styles.headerActions}>
          <Link to={`/tenders/${id}/edit`} className={styles.editBtn}>Edit</Link>
        </div>
      </header>
      <div className={styles.grid}>
        <div className={styles.card}>
          <h3>Tender Details</h3>
          <dl>
            <dt>Reference</dt><dd>{data.referenceNumber}</dd>
            <dt>Client</dt><dd><Link to={`/clients/${data.clientId}`}>{data.clientName}</Link></dd>
            <dt>Closing Date</dt><dd>{data.closingDate}</dd>
            <dt>Estimated Value</dt><dd>{formatCurrency(data.estimatedValue)}</dd>
          </dl>
        </div>
        {data.notes && (
          <div className={styles.card}>
            <h3>Notes</h3>
            <p>{data.notes}</p>
          </div>
        )}
      </div>
    </div>
  )
}
