import { useQuery } from '@tanstack/react-query'
import { useParams, Link } from 'react-router-dom'
import { getVehicle } from '@/api/fleet'
import styles from '../clients/ClientDetailPage.module.css'

export function VehicleDetailPage() {
  const { id } = useParams<{ id: string }>()
  const { data, isLoading, isError } = useQuery({
    queryKey: ['vehicle', id],
    queryFn: () => getVehicle(id!),
    enabled: !!id,
    retry: false,
  })

  if (isLoading) return <p>Loading...</p>
  if (isError || !data) return <p>Vehicle not found.</p>

  return (
    <div className={styles.page}>
      <nav className={styles.breadcrumb}><Link to="/fleet">Fleet</Link> &gt; {data.registrationNumber}</nav>
      <header className={styles.header}>
        <h1>{data.makeModel}</h1>
        <span className={styles.badge}>{data.status}</span>
        <div className={styles.headerActions}>
          <Link to={`/fleet/${id}/edit`} className={styles.editBtn}>Edit</Link>
        </div>
      </header>

      <div className={styles.grid}>
        <div className={styles.card}>
          <h3>Vehicle Details</h3>
          <dl>
            <dt>Registration</dt><dd>{data.registrationNumber}</dd>
            <dt>Type</dt><dd>{data.type}</dd>
            <dt>Driver</dt><dd>{data.driverName ?? '—'}</dd>
            <dt>Location</dt><dd>{data.currentLocation ?? '—'}</dd>
            <dt>Project</dt><dd>{data.assignedProjectName ?? '—'}</dd>
          </dl>
        </div>
        <div className={styles.card}>
          <h3>Compliance</h3>
          <dl>
            <dt>License Expiry</dt><dd>{data.licenseExpiryDate ?? '—'}</dd>
            <dt>Insurance Expiry</dt><dd>{data.insuranceExpiryDate ?? '—'}</dd>
            <dt>Roadworthy Expiry</dt><dd>{data.roadworthyExpiryDate ?? '—'}</dd>
          </dl>
        </div>
        <div className={styles.card}>
          <h3>Maintenance History</h3>
          {data.maintenanceRecords.length === 0 ? (
            <p className={styles.empty}>No maintenance records.</p>
          ) : (
            <ul className={styles.contacts}>
              {data.maintenanceRecords.map((m) => (
                <li key={m.id}>
                  <strong>{m.serviceDate}</strong> — {m.description}
                  <div>R{m.cost.toLocaleString()} · Next: {m.nextServiceDue ?? '—'}</div>
                </li>
              ))}
            </ul>
          )}
        </div>
        <div className={styles.card}>
          <h3>Fuel Log</h3>
          {data.fuelLogs.length === 0 ? (
            <p className={styles.empty}>No fuel entries.</p>
          ) : (
            <ul className={styles.contacts}>
              {data.fuelLogs.map((f) => (
                <li key={f.id}>
                  <strong>{f.logDate}</strong> — {f.litres}L · R{f.cost.toLocaleString()}
                </li>
              ))}
            </ul>
          )}
        </div>
      </div>
    </div>
  )
}
