import { useQuery } from '@tanstack/react-query'
import { useState } from 'react'
import { Link } from 'react-router-dom'
import { ChevronRight, Plus } from 'lucide-react'
import { getProcurementSummary, getPurchaseOrders, formatCurrency } from '@/api/procurement'
import styles from '../projects/ProjectsPage.module.css'
import kpiStyles from '../fleet/FleetPage.module.css'

export function ProcurementPage() {
  const [search, setSearch] = useState('')
  const [status, setStatus] = useState('')

  const { data: summary } = useQuery({ queryKey: ['procurement-summary'], queryFn: getProcurementSummary, retry: false })
  const { data, isLoading, isError } = useQuery({
    queryKey: ['procurement', search, status],
    queryFn: () => getPurchaseOrders({ search: search || undefined, status: status || undefined }),
    retry: false,
  })

  return (
    <div className={styles.page}>
      <nav className={styles.breadcrumb}>Home &gt; Procurement</nav>
      <header className={styles.header}>
        <h1>Purchase Orders</h1>
        <Link to="/procurement/new" className={styles.newBtn}>
          <Plus size={18} />
          New PO
        </Link>
      </header>

      <div className={kpiStyles.kpiRow}>
        <div className={kpiStyles.kpi}><span>{summary?.totalOrders ?? '—'}</span><label>Total POs</label></div>
        <div className={kpiStyles.kpi}><span className={kpiStyles.yellow}>{summary?.pendingApproval ?? '—'}</span><label>Pending Approval</label></div>
        <div className={kpiStyles.kpi}><span className={kpiStyles.green}>{summary?.approved ?? '—'}</span><label>Approved / Ordered</label></div>
        <div className={kpiStyles.kpi}><span>{summary?.delivered ?? '—'}</span><label>Delivered</label></div>
        <div className={kpiStyles.kpi}><span className={kpiStyles.orange}>{summary ? formatCurrency(summary.totalValue) : '—'}</span><label>Total Value</label></div>
      </div>

      <div className={styles.filters}>
        <input type="search" placeholder="Search POs..." value={search} onChange={(e) => setSearch(e.target.value)} />
        <select value={status} onChange={(e) => setStatus(e.target.value)}>
          <option value="">All Status</option>
          <option value="Draft">Draft</option>
          <option value="Pending Approval">Pending Approval</option>
          <option value="Approved">Approved</option>
          <option value="Ordered">Ordered</option>
          <option value="Delivered">Delivered</option>
        </select>
      </div>

      <div className={styles.tableWrap}>
        {isLoading && <p className={styles.loading}>Loading purchase orders...</p>}
        <table className={styles.table}>
          <thead>
            <tr>
              <th>PO Number</th>
              <th>Title</th>
              <th>Supplier</th>
              <th>Project</th>
              <th>Status</th>
              <th>Value</th>
              <th>Order Date</th>
              <th></th>
            </tr>
          </thead>
          <tbody>
            {data?.map((po) => (
              <tr key={po.id}>
                <td className={styles.name}>{po.poNumber}</td>
                <td>{po.title}</td>
                <td>{po.supplierName}</td>
                <td>{po.projectName ?? '—'}</td>
                <td>
                  <span className={`${styles.badge} ${po.status === 'Delivered' ? styles.completed : po.status === 'Pending Approval' ? kpiStyles.maintBadge : styles.active}`}>
                    {po.status}
                  </span>
                </td>
                <td className={styles.value}>{formatCurrency(po.totalAmount)}</td>
                <td>{po.orderDate}</td>
                <td>
                  <Link to={`/procurement/${po.id}`} className={styles.action}><ChevronRight size={18} /></Link>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
        {isError && <p className={styles.hint}>Start the API to load procurement data.</p>}
      </div>
    </div>
  )
}
