import { useQuery } from '@tanstack/react-query'
import { useState } from 'react'
import { Link } from 'react-router-dom'
import { Plus } from 'lucide-react'
import { getBoqSummary, getFinancialSummary, getBoqItems, getClaims, getInvoices, formatCurrency } from '@/api/boq'
import styles from '../projects/ProjectsPage.module.css'
import kpiStyles from '../fleet/FleetPage.module.css'
import localStyles from './BoqPage.module.css'

type Tab = 'boq' | 'claims' | 'invoices'

export function BoqPage() {
  const [tab, setTab] = useState<Tab>('boq')
  const [search, setSearch] = useState('')

  const { data: boqSummary } = useQuery({ queryKey: ['boq-summary'], queryFn: getBoqSummary, retry: false })
  const { data: finSummary } = useQuery({ queryKey: ['financial-summary'], queryFn: getFinancialSummary, retry: false })
  const { data: boqItems, isLoading: boqLoading, isError: boqError } = useQuery({
    queryKey: ['boq-items', search],
    queryFn: () => getBoqItems({ search: search || undefined }),
    enabled: tab === 'boq',
    retry: false,
  })
  const { data: claims, isLoading: claimsLoading, isError: claimsError } = useQuery({
    queryKey: ['claims'],
    queryFn: () => getClaims(),
    enabled: tab === 'claims',
    retry: false,
  })
  const { data: invoices, isLoading: invLoading, isError: invError } = useQuery({
    queryKey: ['invoices'],
    queryFn: () => getInvoices(),
    enabled: tab === 'invoices',
    retry: false,
  })

  return (
    <div className={styles.page}>
      <nav className={styles.breadcrumb}>Home &gt; BOQ &amp; Costing</nav>
      <header className={styles.header}>
        <h1>BOQ &amp; Costing</h1>
        {tab === 'boq' && (
          <Link to="/boq/new" className={styles.newBtn}>
            <Plus size={18} />
            New BOQ Item
          </Link>
        )}
        {tab === 'claims' && (
          <Link to="/boq/claims/new" className={styles.newBtn}>
            <Plus size={18} />
            New Claim
          </Link>
        )}
      </header>

      <div className={kpiStyles.kpiRow}>
        <div className={kpiStyles.kpi}><span>{boqSummary ? formatCurrency(boqSummary.totalValue) : '—'}</span><label>BOQ Total</label></div>
        <div className={kpiStyles.kpi}><span className={kpiStyles.orange}>{finSummary ? formatCurrency(finSummary.pendingClaims) : '—'}</span><label>Pending Claims</label></div>
        <div className={kpiStyles.kpi}><span className={kpiStyles.green}>{finSummary ? formatCurrency(finSummary.paidClaims) : '—'}</span><label>Paid Claims</label></div>
        <div className={kpiStyles.kpi}><span className={kpiStyles.yellow}>{finSummary ? formatCurrency(finSummary.outstandingInvoices) : '—'}</span><label>Outstanding Invoices</label></div>
        <div className={kpiStyles.kpi}><span>{boqSummary?.itemCount ?? '—'}</span><label>BOQ Line Items</label></div>
      </div>

      <div className={localStyles.tabs}>
        {(['boq', 'claims', 'invoices'] as Tab[]).map((t) => (
          <button key={t} className={tab === t ? localStyles.activeTab : undefined} onClick={() => setTab(t)}>
            {t === 'boq' ? 'BOQ Items' : t === 'claims' ? 'Claims' : 'Invoices'}
          </button>
        ))}
      </div>

      {tab === 'boq' && (
        <>
          <div className={styles.filters}>
            <input type="search" placeholder="Search BOQ items..." value={search} onChange={(e) => setSearch(e.target.value)} />
          </div>
          <div className={styles.tableWrap}>
            {boqLoading && <p className={styles.loading}>Loading BOQ...</p>}
            <table className={styles.table}>
              <thead>
                <tr>
                  <th>Code</th><th>Description</th><th>Project</th><th>Category</th>
                  <th>Qty</th><th>Unit</th><th>Rate</th><th>Amount</th>
                </tr>
              </thead>
              <tbody>
                {boqItems?.map((item) => (
                  <tr key={item.id}>
                    <td className={styles.name}>{item.itemCode}</td>
                    <td>{item.description}</td>
                    <td>{item.projectName}</td>
                    <td>{item.category}</td>
                    <td>{item.quantity}</td>
                    <td>{item.unit}</td>
                    <td>{formatCurrency(item.rate)}</td>
                    <td className={styles.value}>{formatCurrency(item.amount)}</td>
                  </tr>
                ))}
              </tbody>
            </table>
            {boqError && <p className={styles.hint}>Start the API to load BOQ data.</p>}
          </div>
        </>
      )}

      {tab === 'claims' && (
        <div className={styles.tableWrap}>
          {claimsLoading && <p className={styles.loading}>Loading claims...</p>}
          <table className={styles.table}>
            <thead>
              <tr>
                <th>Claim No.</th><th>Title</th><th>Project</th><th>Status</th>
                <th>Amount</th><th>Date</th><th>Period</th>
              </tr>
            </thead>
            <tbody>
              {claims?.map((c) => (
                <tr key={c.id}>
                  <td className={styles.name}>{c.claimNumber}</td>
                  <td>{c.title}</td>
                  <td>{c.projectName}</td>
                  <td><span className={`${styles.badge} ${c.status === 'Paid' ? styles.completed : styles.active}`}>{c.status}</span></td>
                  <td className={styles.value}>{formatCurrency(c.amount)}</td>
                  <td>{c.claimDate}</td>
                  <td>{c.periodFrom && c.periodTo ? `${c.periodFrom} – ${c.periodTo}` : '—'}</td>
                </tr>
              ))}
            </tbody>
          </table>
          {claimsError && <p className={styles.hint}>Start the API to load claims.</p>}
        </div>
      )}

      {tab === 'invoices' && (
        <div className={styles.tableWrap}>
          {invLoading && <p className={styles.loading}>Loading invoices...</p>}
          <table className={styles.table}>
            <thead>
              <tr>
                <th>Invoice No.</th><th>Project</th><th>Client</th><th>Status</th>
                <th>Amount</th><th>Invoice Date</th><th>Due Date</th>
              </tr>
            </thead>
            <tbody>
              {invoices?.map((inv) => (
                <tr key={inv.id}>
                  <td className={styles.name}>{inv.invoiceNumber}</td>
                  <td>{inv.projectName}</td>
                  <td>{inv.clientName}</td>
                  <td><span className={`${styles.badge} ${inv.status === 'Paid' ? styles.completed : inv.status === 'Overdue' ? kpiStyles.maintBadge : styles.active}`}>{inv.status}</span></td>
                  <td className={styles.value}>{formatCurrency(inv.amount)}</td>
                  <td>{inv.invoiceDate}</td>
                  <td>{inv.dueDate ?? '—'}</td>
                </tr>
              ))}
            </tbody>
          </table>
          {invError && <p className={styles.hint}>Start the API to load invoices.</p>}
        </div>
      )}
    </div>
  )
}
