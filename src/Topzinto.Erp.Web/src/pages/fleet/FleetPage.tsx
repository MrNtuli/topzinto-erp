import { useQuery } from '@tanstack/react-query'
import { useState } from 'react'
import { Link } from 'react-router-dom'
import { ChevronRight, Plus } from 'lucide-react'
import { getFleetSummary, getVehicles } from '@/api/fleet'
import styles from '../projects/ProjectsPage.module.css'
import fleetStyles from './FleetPage.module.css'

export function FleetPage() {
  const [search, setSearch] = useState('')
  const [status, setStatus] = useState('')

  const { data: summary } = useQuery({ queryKey: ['fleet-summary'], queryFn: getFleetSummary, retry: false })
  const { data, isLoading, isError } = useQuery({
    queryKey: ['fleet', search, status],
    queryFn: () => getVehicles({ search: search || undefined, status: status || undefined }),
    retry: false,
  })

  return (
    <div className={styles.page}>
      <nav className={styles.breadcrumb}>Home &gt; Fleet</nav>
      <header className={styles.header}>
        <h1>Fleet Management</h1>
        <Link to="/fleet/new" className={styles.newBtn}>
          <Plus size={18} />
          Register Vehicle
        </Link>
      </header>

      <div className={fleetStyles.kpiRow}>
        <div className={fleetStyles.kpi}><span>{summary?.total ?? '—'}</span><label>Total Vehicles</label></div>
        <div className={fleetStyles.kpi}><span className={fleetStyles.green}>{summary?.available ?? '—'}</span><label>Available</label></div>
        <div className={fleetStyles.kpi}><span className={fleetStyles.orange}>{summary?.inUse ?? '—'}</span><label>In Use</label></div>
        <div className={fleetStyles.kpi}><span className={fleetStyles.yellow}>{summary?.maintenance ?? '—'}</span><label>Maintenance</label></div>
        <div className={fleetStyles.kpi}><span className={fleetStyles.red}>{summary?.expiringSoon ?? '—'}</span><label>Expiring Soon</label></div>
      </div>

      <div className={styles.filters}>
        <input type="search" placeholder="Search vehicles..." value={search} onChange={(e) => setSearch(e.target.value)} />
        <select value={status} onChange={(e) => setStatus(e.target.value)}>
          <option value="">All Status</option>
          <option value="Available">Available</option>
          <option value="In Use">In Use</option>
          <option value="Maintenance">Maintenance</option>
        </select>
      </div>

      <div className={styles.tableWrap}>
        {isLoading && <p className={styles.loading}>Loading fleet...</p>}
        <table className={styles.table}>
          <thead>
            <tr>
              <th>Registration</th>
              <th>Vehicle</th>
              <th>Type</th>
              <th>Driver</th>
              <th>Status</th>
              <th>Location</th>
              <th>Project</th>
              <th></th>
            </tr>
          </thead>
          <tbody>
            {data?.map((v) => (
              <tr key={v.id} className={v.isExpiringSoon ? fleetStyles.expiring : undefined}>
                <td className={styles.name}>{v.registrationNumber}</td>
                <td>{v.makeModel}</td>
                <td>{v.type}</td>
                <td>{v.driverName ?? '—'}</td>
                <td>
                  <span className={`${styles.badge} ${v.status === 'In Use' ? styles.active : v.status === 'Maintenance' ? fleetStyles.maintBadge : styles.completed}`}>
                    {v.status}
                  </span>
                </td>
                <td>{v.currentLocation ?? '—'}</td>
                <td>{v.assignedProjectName ?? '—'}</td>
                <td>
                  <Link to={`/fleet/${v.id}`} className={styles.action}><ChevronRight size={18} /></Link>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
        {isError && <p className={styles.hint}>Start the API to load fleet data.</p>}
      </div>
    </div>
  )
}
