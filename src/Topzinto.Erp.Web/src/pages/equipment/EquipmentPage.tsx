import { useQuery } from '@tanstack/react-query'
import { useState } from 'react'
import { Link } from 'react-router-dom'
import { ChevronRight, Plus } from 'lucide-react'
import { getEquipmentSummary, getEquipmentList } from '@/api/equipment'
import styles from '../projects/ProjectsPage.module.css'
import fleetStyles from '../fleet/FleetPage.module.css'

export function EquipmentPage() {
  const [search, setSearch] = useState('')
  const [status, setStatus] = useState('')

  const { data: summary } = useQuery({ queryKey: ['equipment-summary'], queryFn: getEquipmentSummary, retry: false })
  const { data, isLoading, isError } = useQuery({
    queryKey: ['equipment', search, status],
    queryFn: () => getEquipmentList({ search: search || undefined, status: status || undefined }),
    retry: false,
  })

  return (
    <div className={styles.page}>
      <nav className={styles.breadcrumb}>Home &gt; Equipment</nav>
      <header className={styles.header}>
        <h1>Equipment Management</h1>
        <Link to="/equipment/new" className={styles.newBtn}>
          <Plus size={18} />
          Register Asset
        </Link>
      </header>

      <div className={fleetStyles.kpiRow}>
        <div className={fleetStyles.kpi}><span>{summary?.total ?? '—'}</span><label>Total Assets</label></div>
        <div className={fleetStyles.kpi}><span className={fleetStyles.green}>{summary?.available ?? '—'}</span><label>Available</label></div>
        <div className={fleetStyles.kpi}><span className={fleetStyles.orange}>{summary?.inUse ?? '—'}</span><label>In Use</label></div>
        <div className={fleetStyles.kpi}><span className={fleetStyles.yellow}>{summary?.maintenance ?? '—'}</span><label>Maintenance</label></div>
        <div className={fleetStyles.kpi}><span className={fleetStyles.red}>{summary?.inspectionDue ?? '—'}</span><label>Inspection Due</label></div>
      </div>

      <div className={styles.filters}>
        <input type="search" placeholder="Search equipment..." value={search} onChange={(e) => setSearch(e.target.value)} />
        <select value={status} onChange={(e) => setStatus(e.target.value)}>
          <option value="">All Status</option>
          <option value="Available">Available</option>
          <option value="In Use">In Use</option>
          <option value="Maintenance">Maintenance</option>
        </select>
      </div>

      <div className={styles.tableWrap}>
        {isLoading && <p className={styles.loading}>Loading equipment...</p>}
        <table className={styles.table}>
          <thead>
            <tr>
              <th>Asset Tag</th>
              <th>Name</th>
              <th>Category</th>
              <th>Operator</th>
              <th>Status</th>
              <th>Project</th>
              <th>Next Service</th>
              <th></th>
            </tr>
          </thead>
          <tbody>
            {data?.map((e) => (
              <tr key={e.id} className={e.isInspectionDue ? fleetStyles.expiring : undefined}>
                <td className={styles.name}>{e.assetTag}</td>
                <td>{e.name}</td>
                <td>{e.category}</td>
                <td>{e.operatorName ?? '—'}</td>
                <td>
                  <span className={`${styles.badge} ${e.status === 'In Use' ? styles.active : e.status === 'Maintenance' ? fleetStyles.maintBadge : styles.completed}`}>
                    {e.status}
                  </span>
                </td>
                <td>{e.assignedProjectName ?? '—'}</td>
                <td>{e.nextServiceDue ?? '—'}</td>
                <td>
                  <Link to={`/equipment/${e.id}`} className={styles.action}><ChevronRight size={18} /></Link>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
        {isError && <p className={styles.hint}>Start the API to load equipment data.</p>}
      </div>
    </div>
  )
}
