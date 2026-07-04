import { useQuery } from '@tanstack/react-query'
import { useParams, Link } from 'react-router-dom'
import { getPurchaseOrder, formatCurrency } from '@/api/procurement'
import styles from '../clients/ClientDetailPage.module.css'
import tableStyles from '../projects/ProjectsPage.module.css'

export function PurchaseOrderDetailPage() {
  const { id } = useParams<{ id: string }>()
  const { data, isLoading, isError } = useQuery({
    queryKey: ['purchase-order', id],
    queryFn: () => getPurchaseOrder(id!),
    enabled: !!id,
    retry: false,
  })

  if (isLoading) return <p>Loading...</p>
  if (isError || !data) return <p>Purchase order not found.</p>

  return (
    <div className={styles.page}>
      <nav className={styles.breadcrumb}><Link to="/procurement">Procurement</Link> &gt; {data.poNumber}</nav>
      <header className={styles.header}>
        <h1>{data.title}</h1>
        <span className={styles.badge}>{data.status}</span>
        <div className={styles.headerActions}>
          <Link to={`/procurement/${id}/edit`} className={styles.editBtn}>Edit</Link>
        </div>
      </header>

      <div className={styles.grid}>
        <div className={styles.card}>
          <h3>Order Details</h3>
          <dl>
            <dt>PO Number</dt><dd>{data.poNumber}</dd>
            <dt>Supplier</dt><dd><Link to={`/suppliers/${data.supplierId}`}>{data.supplierName}</Link></dd>
            <dt>Project</dt><dd>{data.projectName ?? '—'}</dd>
            <dt>Order Date</dt><dd>{data.orderDate}</dd>
            <dt>Required Date</dt><dd>{data.requiredDate ?? '—'}</dd>
            <dt>Total</dt><dd>{formatCurrency(data.totalAmount)}</dd>
          </dl>
        </div>
        <div className={styles.card}>
          <h3>Approval</h3>
          <dl>
            <dt>Requested By</dt><dd>{data.requestedByName ?? '—'}</dd>
            <dt>Approved By</dt><dd>{data.approvedByName ?? '—'}</dd>
            <dt>Notes</dt><dd>{data.notes ?? '—'}</dd>
          </dl>
        </div>
        <div className={styles.card} style={{ gridColumn: '1 / -1' }}>
          <h3>Line Items</h3>
          <div className={tableStyles.tableWrap}>
            <table className={tableStyles.table}>
              <thead>
                <tr>
                  <th>Description</th>
                  <th>Qty</th>
                  <th>Unit</th>
                  <th>Unit Price</th>
                  <th>Line Total</th>
                </tr>
              </thead>
              <tbody>
                {data.lines.map((line) => (
                  <tr key={line.id}>
                    <td>{line.description}</td>
                    <td>{line.quantity}</td>
                    <td>{line.unit}</td>
                    <td>{formatCurrency(line.unitPrice)}</td>
                    <td className={tableStyles.value}>{formatCurrency(line.lineTotal)}</td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </div>
      </div>
    </div>
  )
}
