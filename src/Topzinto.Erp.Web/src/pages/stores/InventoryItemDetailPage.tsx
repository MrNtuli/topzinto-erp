import { useQuery } from '@tanstack/react-query'
import { useParams, Link } from 'react-router-dom'
import { getInventoryItem, formatCurrency } from '@/api/stores'
import styles from '../clients/ClientDetailPage.module.css'
import tableStyles from '../projects/ProjectsPage.module.css'

export function InventoryItemDetailPage() {
  const { id } = useParams<{ id: string }>()
  const { data, isLoading, isError } = useQuery({
    queryKey: ['inventory-item', id],
    queryFn: () => getInventoryItem(id!),
    enabled: !!id,
    retry: false,
  })

  if (isLoading) return <p>Loading...</p>
  if (isError || !data) return <p>Inventory item not found.</p>

  const isLowStock = data.quantityOnHand <= data.reorderLevel

  return (
    <div className={styles.page}>
      <nav className={styles.breadcrumb}>
        <Link to="/stores">Stores</Link> &gt; {data.itemCode}
      </nav>
      <header className={styles.header}>
        <h1>{data.name}</h1>
        {isLowStock && <span className={styles.badge}>Low Stock</span>}
        <div className={styles.headerActions}>
          <Link to={`/stores/${id}/edit`} className={styles.editBtn}>Edit</Link>
        </div>
      </header>

      <div className={styles.grid}>
        <div className={styles.card}>
          <h3>Item Details</h3>
          <dl>
            <dt>Item Code</dt><dd>{data.itemCode}</dd>
            <dt>Category</dt><dd>{data.category}</dd>
            <dt>Unit</dt><dd>{data.unit}</dd>
            <dt>Quantity on Hand</dt><dd>{data.quantityOnHand}</dd>
            <dt>Reorder Level</dt><dd>{data.reorderLevel}</dd>
            <dt>Location</dt><dd>{data.location ?? '—'}</dd>
            <dt>Unit Cost</dt><dd>{formatCurrency(data.unitCost)}</dd>
            <dt>Stock Value</dt><dd>{formatCurrency(data.quantityOnHand * data.unitCost)}</dd>
          </dl>
        </div>
        {data.notes && (
          <div className={styles.card}>
            <h3>Notes</h3>
            <p>{data.notes}</p>
          </div>
        )}
        <div className={styles.card} style={{ gridColumn: '1 / -1' }}>
          <h3>Recent Transactions</h3>
          {data.recentTransactions.length === 0 ? (
            <p className={styles.empty}>No transactions recorded.</p>
          ) : (
            <div className={tableStyles.tableWrap}>
              <table className={tableStyles.table}>
                <thead>
                  <tr>
                    <th>Date</th>
                    <th>Type</th>
                    <th>Qty</th>
                    <th>Reference</th>
                    <th>Project</th>
                    <th>By</th>
                  </tr>
                </thead>
                <tbody>
                  {data.recentTransactions.map((tx) => (
                    <tr key={tx.id}>
                      <td>{tx.transactionDate}</td>
                      <td>{tx.transactionType}</td>
                      <td>{tx.quantity}</td>
                      <td>{tx.reference ?? '—'}</td>
                      <td>{tx.projectName ?? '—'}</td>
                      <td>{tx.recordedByName ?? '—'}</td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          )}
          <p style={{ marginTop: 12 }}>
            <Link to="/stores/transactions/new">Record stock transaction</Link>
          </p>
        </div>
      </div>
    </div>
  )
}
