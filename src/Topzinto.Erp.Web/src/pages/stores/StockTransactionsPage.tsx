import { useQuery } from '@tanstack/react-query'
import { useState } from 'react'
import { Link } from 'react-router-dom'
import { getInventoryTransactions } from '@/api/stores'
import styles from '../projects/ProjectsPage.module.css'

export function StockTransactionsPage() {
  const [search, setSearch] = useState('')

  const { data, isLoading, isError } = useQuery({
    queryKey: ['inventory-transactions', search],
    queryFn: () => getInventoryTransactions(search || undefined),
    retry: false,
  })

  return (
    <div className={styles.page}>
      <nav className={styles.breadcrumb}>
        <Link to="/stores">Stores</Link> &gt; Stock Transactions
      </nav>
      <header className={styles.header}><h1>Stock Transactions</h1></header>

      <div className={styles.filters}>
        <input type="search" placeholder="Search by item or reference..." value={search} onChange={(e) => setSearch(e.target.value)} />
      </div>

      <div className={styles.tableWrap}>
        {isLoading && <p className={styles.loading}>Loading transactions...</p>}
        <table className={styles.table}>
          <thead>
            <tr>
              <th>Date</th>
              <th>Item</th>
              <th>Type</th>
              <th>Quantity</th>
              <th>Reference</th>
              <th>Project</th>
              <th>Recorded By</th>
            </tr>
          </thead>
          <tbody>
            {data?.map((tx) => (
              <tr key={tx.id}>
                <td>{tx.transactionDate}</td>
                <td className={styles.name}>{tx.itemCode} — {tx.itemName}</td>
                <td>
                  <span className={`${styles.badge} ${tx.transactionType === 'Stock In' ? styles.active : tx.transactionType === 'Stock Out' ? styles.completed : ''}`}>
                    {tx.transactionType}
                  </span>
                </td>
                <td>{tx.quantity}</td>
                <td>{tx.reference ?? '—'}</td>
                <td>{tx.projectName ?? '—'}</td>
                <td>{tx.recordedByName ?? '—'}</td>
              </tr>
            ))}
          </tbody>
        </table>
        {isError && <p className={styles.hint}>Start the API to load transactions.</p>}
      </div>
    </div>
  )
}
