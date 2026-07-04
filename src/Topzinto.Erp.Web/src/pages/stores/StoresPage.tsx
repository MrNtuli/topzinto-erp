import { useQuery } from '@tanstack/react-query'
import { useState } from 'react'
import { Link } from 'react-router-dom'
import { getStoresSummary, getInventoryItems, formatCurrency } from '@/api/stores'
import styles from '../projects/ProjectsPage.module.css'
import kpiStyles from '../fleet/FleetPage.module.css'
import localStyles from '../clients/ClientsPage.module.css'

export function StoresPage() {
  const [search, setSearch] = useState('')
  const [lowStockOnly, setLowStockOnly] = useState(false)

  const { data: summary } = useQuery({ queryKey: ['stores-summary'], queryFn: getStoresSummary, retry: false })
  const { data, isLoading, isError } = useQuery({
    queryKey: ['inventory', search, lowStockOnly],
    queryFn: () => getInventoryItems({ search: search || undefined, lowStockOnly: lowStockOnly || undefined }),
    retry: false,
  })

  return (
    <div className={styles.page}>
      <nav className={styles.breadcrumb}>Home &gt; Stores &gt; Inventory</nav>
      <header className={styles.header}>
        <h1>Inventory</h1>
        <div style={{ display: 'flex', gap: 10 }}>
          <Link to="/stores/new" className={styles.newBtn}>Add Item</Link>
          <Link to="/stores/transactions/new" className={styles.newBtn}>Record Transaction</Link>
          <Link to="/stores/transactions" className={styles.newBtn} style={{ background: 'var(--color-navy)' }}>Transactions</Link>
        </div>
      </header>

      <div className={kpiStyles.kpiRow}>
        <div className={kpiStyles.kpi}><span>{summary?.totalItems ?? '—'}</span><label>Total Items</label></div>
        <div className={kpiStyles.kpi}><span className={kpiStyles.red}>{summary?.lowStock ?? '—'}</span><label>Low Stock</label></div>
        <div className={kpiStyles.kpi}><span className={kpiStyles.orange}>{summary ? formatCurrency(summary.totalValue) : '—'}</span><label>Stock Value</label></div>
        <div className={kpiStyles.kpi}><span>{summary?.transactionsThisMonth ?? '—'}</span><label>Tx This Month</label></div>
      </div>

      <div className={styles.filters}>
        <input type="search" placeholder="Search inventory..." value={search} onChange={(e) => setSearch(e.target.value)} />
        <label style={{ display: 'flex', alignItems: 'center', gap: 8 }}>
          <input type="checkbox" checked={lowStockOnly} onChange={(e) => setLowStockOnly(e.target.checked)} />
          Low stock only
        </label>
      </div>

      <div className={styles.tableWrap}>
        {isLoading && <p className={styles.loading}>Loading inventory...</p>}
        <table className={styles.table}>
          <thead>
            <tr>
              <th>Item Code</th>
              <th>Name</th>
              <th>Category</th>
              <th>On Hand</th>
              <th>Unit</th>
              <th>Reorder Level</th>
              <th>Location</th>
              <th>Unit Cost</th>
            </tr>
          </thead>
          <tbody>
            {data?.map((item) => (
              <tr key={item.id} className={item.isLowStock ? kpiStyles.expiring : undefined}>
                <td className={styles.name}>
                  <Link to={`/stores/${item.id}`} className={localStyles.link}>{item.itemCode}</Link>
                </td>
                <td>{item.name}</td>
                <td>{item.category}</td>
                <td>{item.quantityOnHand}</td>
                <td>{item.unit}</td>
                <td>{item.reorderLevel}</td>
                <td>{item.location ?? '—'}</td>
                <td className={styles.value}>{formatCurrency(item.unitCost)}</td>
              </tr>
            ))}
          </tbody>
        </table>
        {isError && <p className={styles.hint}>Start the API to load inventory.</p>}
      </div>
    </div>
  )
}
