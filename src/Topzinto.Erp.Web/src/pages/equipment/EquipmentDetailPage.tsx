import { useQuery } from '@tanstack/react-query'
import { useParams, Link } from 'react-router-dom'
import { getEquipment } from '@/api/equipment'
import styles from '../clients/ClientDetailPage.module.css'

export function EquipmentDetailPage() {
  const { id } = useParams<{ id: string }>()
  const { data, isLoading, isError } = useQuery({
    queryKey: ['equipment-detail', id],
    queryFn: () => getEquipment(id!),
    enabled: !!id,
    retry: false,
  })

  if (isLoading) return <p>Loading...</p>
  if (isError || !data) return <p>Equipment not found.</p>

  return (
    <div className={styles.page}>
      <nav className={styles.breadcrumb}><Link to="/equipment">Equipment</Link> &gt; {data.assetTag}</nav>
      <header className={styles.header}>
        <h1>{data.name}</h1>
        <span className={styles.badge}>{data.status}</span>
        <div className={styles.headerActions}>
          <Link to={`/equipment/${id}/edit`} className={styles.editBtn}>Edit</Link>
        </div>
      </header>

      <div className={styles.grid}>
        <div className={styles.card}>
          <h3>Asset Details</h3>
          <dl>
            <dt>Asset Tag</dt><dd>{data.assetTag}</dd>
            <dt>Category</dt><dd>{data.category}</dd>
            <dt>Make / Model</dt><dd>{data.makeModel ?? '—'}</dd>
            <dt>Serial No.</dt><dd>{data.serialNumber ?? '—'}</dd>
            <dt>Operator</dt><dd>{data.operatorName ?? '—'}</dd>
            <dt>Project</dt><dd>{data.assignedProjectName ?? '—'}</dd>
          </dl>
        </div>
        <div className={styles.card}>
          <h3>Service & Inspection</h3>
          <dl>
            <dt>Last Inspection</dt><dd>{data.lastInspectionDate ?? '—'}</dd>
            <dt>Next Inspection</dt><dd>{data.nextInspectionDue ?? '—'}</dd>
            <dt>Last Service</dt><dd>{data.lastServiceDate ?? '—'}</dd>
            <dt>Next Service</dt><dd>{data.nextServiceDue ?? '—'}</dd>
          </dl>
        </div>
        <div className={styles.card}>
          <h3>Bookings</h3>
          {data.bookings.length === 0 ? (
            <p className={styles.empty}>No bookings.</p>
          ) : (
            <ul className={styles.contacts}>
              {data.bookings.map((b) => (
                <li key={b.id}>
                  <strong>{b.projectName}</strong>
                  <div>{b.startDate} → {b.endDate}</div>
                </li>
              ))}
            </ul>
          )}
        </div>
        <div className={styles.card}>
          <h3>Inspections</h3>
          {data.inspections.length === 0 ? (
            <p className={styles.empty}>No inspections recorded.</p>
          ) : (
            <ul className={styles.contacts}>
              {data.inspections.map((i) => (
                <li key={i.id}>
                  <strong>{i.inspectionDate}</strong> — {i.result}
                  <div>Inspector: {i.inspectorName ?? '—'}</div>
                </li>
              ))}
            </ul>
          )}
        </div>
      </div>
    </div>
  )
}
