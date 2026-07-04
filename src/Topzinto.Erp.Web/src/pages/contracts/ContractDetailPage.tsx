import { useQuery } from '@tanstack/react-query'
import { useParams, Link } from 'react-router-dom'
import { getContract, formatCurrency } from '@/api/contracts'
import styles from '../clients/ClientDetailPage.module.css'

export function ContractDetailPage() {
  const { id } = useParams<{ id: string }>()
  const { data, isLoading, isError } = useQuery({
    queryKey: ['contract', id],
    queryFn: () => getContract(id!),
    enabled: !!id,
  })

  if (isLoading) return <p>Loading...</p>
  if (isError || !data) return <p>Contract not found.</p>

  return (
    <div className={styles.page}>
      <nav className={styles.breadcrumb}><Link to="/contracts">Contracts</Link> &gt; {data.contractNumber}</nav>
      <header className={styles.header}>
        <h1>{data.title}</h1>
        <span className={styles.badge}>{data.status}</span>
        <div className={styles.headerActions}>
          <Link to={`/contracts/${id}/edit`} className={styles.editBtn}>Edit</Link>
        </div>
      </header>
      <div className={styles.grid}>
        <div className={styles.card}>
          <h3>Contract Details</h3>
          <dl>
            <dt>Contract Number</dt><dd>{data.contractNumber}</dd>
            <dt>Client</dt><dd><Link to={`/clients/${data.clientId}`}>{data.clientName}</Link></dd>
            <dt>Project</dt><dd>{data.projectName ? <Link to={`/projects/${data.projectId}`}>{data.projectName}</Link> : '—'}</dd>
            <dt>Value</dt><dd>{formatCurrency(data.value)}</dd>
            <dt>Retention</dt><dd>{data.retentionPercent}%</dd>
            <dt>Start Date</dt><dd>{data.startDate ?? '—'}</dd>
            <dt>End Date</dt><dd>{data.endDate ?? '—'}</dd>
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
