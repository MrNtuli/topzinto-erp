import { useState } from 'react'
import { useQuery } from '@tanstack/react-query'
import { useParams, Link } from 'react-router-dom'
import { getProject, formatCurrency } from '@/api/projects'
import { getProjectMilestones, getProjectTasks } from '@/api/schedule'
import { getBoqItems, getClaims, getInvoices, formatCurrency as fmtBoq } from '@/api/boq'
import { getTimesheets } from '@/api/timesheets'
import { getSiteReports } from '@/api/siteReports'
import { getDocuments } from '@/api/documents'
import { ProjectActivityFeed } from './ProjectActivityFeed'
import styles from './ProjectDetailPage.module.css'
import tableStyles from '../projects/ProjectsPage.module.css'
import detailStyles from '../clients/ClientDetailPage.module.css'

const TABS = ['Overview', 'Schedule', 'BOQ', 'Site Reports', 'Financial', 'Labour', 'Documents', 'Activity'] as const
type Tab = (typeof TABS)[number]

export function ProjectDetailPage() {
  const { id } = useParams<{ id: string }>()
  const [tab, setTab] = useState<Tab>('Overview')

  const { data, isLoading, isError } = useQuery({
    queryKey: ['project', id],
    queryFn: () => getProject(id!),
    enabled: !!id,
  })

  const { data: milestones } = useQuery({
    queryKey: ['milestones', id],
    queryFn: () => getProjectMilestones(id!),
    enabled: !!id && tab === 'Schedule',
    retry: false,
  })

  const { data: tasks } = useQuery({
    queryKey: ['tasks', id],
    queryFn: () => getProjectTasks(id!),
    enabled: !!id && tab === 'Schedule',
    retry: false,
  })

  const { data: boqItems } = useQuery({
    queryKey: ['boq', id],
    queryFn: () => getBoqItems({ projectId: id }),
    enabled: !!id && tab === 'BOQ',
    retry: false,
  })

  const { data: siteReports } = useQuery({
    queryKey: ['site-reports', id],
    queryFn: () => getSiteReports(id),
    enabled: !!id && tab === 'Site Reports',
    retry: false,
  })

  const { data: claims } = useQuery({
    queryKey: ['claims', id],
    queryFn: () => getClaims({ projectId: id }),
    enabled: !!id && tab === 'Financial',
    retry: false,
  })

  const { data: invoices } = useQuery({
    queryKey: ['invoices', id],
    queryFn: () => getInvoices({ projectId: id }),
    enabled: !!id && tab === 'Financial',
    retry: false,
  })

  const { data: documents } = useQuery({
    queryKey: ['documents', id],
    queryFn: () => getDocuments({ parentType: 'Project', parentId: id }),
    enabled: !!id && tab === 'Documents',
    retry: false,
  })

  const { data: timesheets } = useQuery({
    queryKey: ['timesheets', id],
    queryFn: () => getTimesheets({ projectId: id }),
    enabled: !!id && (tab === 'Labour' || tab === 'Financial'),
    retry: false,
  })

  const labourHours = timesheets?.reduce((sum, t) => sum + t.hours, 0) ?? 0
  const labourCost = timesheets?.reduce((sum, t) => sum + (t.labourCost ?? 0), 0) ?? 0

  if (isLoading) return <p>Loading...</p>
  if (isError || !data) return <p>Project not found.</p>

  return (
    <div className={styles.page}>
      <nav className={styles.breadcrumb}>
        <Link to="/projects">Projects</Link> &gt; {data.name}
      </nav>
      <header className={styles.header}>
        <div>
          <h1>{data.name}</h1>
          <span className={styles.code}>{data.code}</span>
        </div>
        <span className={`${styles.status} ${data.status === 'Completed' ? styles.completed : styles.active}`}>
          {data.status}
        </span>
        <div className={detailStyles.headerActions}>
          <Link to={`/projects/${id}/edit`} className={detailStyles.editBtn}>Edit</Link>
        </div>
      </header>

      <div className={styles.tabs}>
        {TABS.map((t) => (
          <button
            key={t}
            type="button"
            className={tab === t ? styles.tabActive : styles.tab}
            onClick={() => setTab(t)}
          >
            {t}
          </button>
        ))}
      </div>

      {tab === 'Overview' && (
        <div className={styles.grid}>
          <div className={styles.card}>
            <h3>Project Summary</h3>
            <dl>
              <dt>Client</dt><dd>{data.clientName}</dd>
              <dt>Site</dt><dd>{data.siteLocation ?? '—'}</dd>
              <dt>Start</dt><dd>{data.startDate ?? '—'}</dd>
              <dt>End</dt><dd>{data.endDate ?? '—'}</dd>
              <dt>Progress</dt><dd>{data.progress}%</dd>
            </dl>
          </div>
          <div className={styles.card}>
            <h3>Financial Summary</h3>
            <dl>
              <dt>Contract Value</dt><dd className={styles.amount}>{formatCurrency(data.contractValue)}</dd>
              <dt>Budget</dt><dd className={styles.amount}>{formatCurrency(data.budget)}</dd>
            </dl>
          </div>
          <div className={`${styles.card} ${styles.wide}`}>
            <h3>Description</h3>
            <p>{data.description ?? 'No description provided.'}</p>
          </div>
        </div>
      )}

      {tab === 'Schedule' && (
        <div className={styles.grid}>
          <div className={`${styles.card} ${styles.wide}`}>
            <h3>Milestones</h3>
            <div className={tableStyles.tableWrap}>
              <table className={tableStyles.table}>
                <thead>
                  <tr><th>Name</th><th>Due</th><th>Status</th><th>Progress</th></tr>
                </thead>
                <tbody>
                  {milestones?.map((m) => (
                    <tr key={m.id}>
                      <td>{m.name}</td>
                      <td>{m.dueDate}</td>
                      <td>{m.status}</td>
                      <td>{m.progress}%</td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          </div>
          <div className={`${styles.card} ${styles.wide}`}>
            <h3>Tasks</h3>
            <div className={tableStyles.tableWrap}>
              <table className={tableStyles.table}>
                <thead>
                  <tr><th>Task</th><th>Assigned</th><th>Priority</th><th>Due</th><th>Status</th></tr>
                </thead>
                <tbody>
                  {tasks?.map((t) => (
                    <tr key={t.id}>
                      <td>{t.title}</td>
                      <td>{t.assignedToName ?? '—'}</td>
                      <td>{t.priority}</td>
                      <td>{t.dueDate ?? '—'}</td>
                      <td>{t.status}</td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          </div>
        </div>
      )}

      {tab === 'BOQ' && (
        <div className={tableStyles.tableWrap}>
          <table className={tableStyles.table}>
            <thead>
              <tr><th>Code</th><th>Description</th><th>Category</th><th>Qty</th><th>Rate</th><th>Amount</th></tr>
            </thead>
            <tbody>
              {boqItems?.map((item) => (
                <tr key={item.id}>
                  <td className={tableStyles.name}>
                    <Link to={`/boq/${item.id}/edit`}>{item.itemCode}</Link>
                  </td>
                  <td>{item.description}</td>
                  <td>{item.category}</td>
                  <td>{item.quantity} {item.unit}</td>
                  <td>{fmtBoq(item.rate)}</td>
                  <td className={tableStyles.value}>{fmtBoq(item.amount)}</td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}

      {tab === 'Site Reports' && (
        <div className={tableStyles.tableWrap}>
          <table className={tableStyles.table}>
            <thead>
              <tr><th>Date</th><th>Weather</th><th>Status</th><th>Submitted By</th><th>Summary</th></tr>
            </thead>
            <tbody>
              {siteReports?.map((r) => (
                <tr key={r.id}>
                  <td><Link to={`/site-reports/${r.id}`}>{r.reportDate}</Link></td>
                  <td>{r.weather ?? '—'}</td>
                  <td>{r.status}</td>
                  <td>{r.submittedByName ?? '—'}</td>
                  <td>{r.workCompletedPreview}</td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}

      {tab === 'Financial' && (
        <div className={styles.grid}>
          <div className={styles.card}>
            <h3>Labour Cost (logged)</h3>
            <dl>
              <dt>Total Hours</dt><dd>{labourHours.toFixed(1)}</dd>
              <dt>Labour Cost</dt><dd className={styles.amount}>{fmtBoq(labourCost)}</dd>
            </dl>
          </div>
          <div className={`${styles.card} ${styles.wide}`}>
            <h3>Claims</h3>
            <div className={tableStyles.tableWrap}>
              <table className={tableStyles.table}>
                <thead>
                  <tr><th>Claim No.</th><th>Title</th><th>Status</th><th>Amount</th><th>Date</th></tr>
                </thead>
                <tbody>
                  {claims?.map((c) => (
                    <tr key={c.id}>
                      <td>
                        <Link to={`/boq/claims/${c.id}/edit`}>{c.claimNumber}</Link>
                      </td>
                      <td>{c.title}</td>
                      <td>{c.status}</td>
                      <td className={tableStyles.value}>{fmtBoq(c.amount)}</td>
                      <td>{c.claimDate}</td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          </div>
          <div className={`${styles.card} ${styles.wide}`}>
            <h3>Invoices</h3>
            <div className={tableStyles.tableWrap}>
              <table className={tableStyles.table}>
                <thead>
                  <tr><th>Invoice No.</th><th>Status</th><th>Amount</th><th>Date</th><th>Due</th></tr>
                </thead>
                <tbody>
                  {invoices?.map((inv) => (
                    <tr key={inv.id}>
                      <td>{inv.invoiceNumber}</td>
                      <td>{inv.status}</td>
                      <td className={tableStyles.value}>{fmtBoq(inv.amount)}</td>
                      <td>{inv.invoiceDate}</td>
                      <td>{inv.dueDate ?? '—'}</td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          </div>
        </div>
      )}

      {tab === 'Labour' && (
        <div className={styles.grid}>
          <div className={styles.card}>
            <h3>Labour Summary</h3>
            <dl>
              <dt>Total Hours</dt><dd>{labourHours.toFixed(1)}</dd>
              <dt>Labour Cost</dt><dd className={styles.amount}>{fmtBoq(labourCost)}</dd>
              <dt>Entries</dt><dd>{timesheets?.length ?? 0}</dd>
            </dl>
            <p style={{ marginTop: 12 }}>
              <Link to="/timesheets/new">Log hours</Link>
            </p>
          </div>
          <div className={`${styles.card} ${styles.wide}`}>
            <h3>Timesheet Entries</h3>
            <div className={tableStyles.tableWrap}>
              <table className={tableStyles.table}>
                <thead>
                  <tr><th>Date</th><th>Employee</th><th>Hours</th><th>Description</th><th>Status</th><th>Cost</th></tr>
                </thead>
                <tbody>
                  {timesheets?.map((t) => (
                    <tr key={t.id}>
                      <td>{t.workDate}</td>
                      <td>{t.employeeName}</td>
                      <td>{t.hours}</td>
                      <td>{t.description ?? '—'}</td>
                      <td>{t.status}</td>
                      <td className={tableStyles.value}>{t.labourCost != null ? fmtBoq(t.labourCost) : '—'}</td>
                    </tr>
                  ))}
                </tbody>
              </table>
              {!timesheets?.length && <p className={tableStyles.loading}>No timesheet entries for this project.</p>}
            </div>
          </div>
        </div>
      )}

      {tab === 'Documents' && (
        <div className={tableStyles.tableWrap}>
          <table className={tableStyles.table}>
            <thead>
              <tr><th>Title</th><th>Category</th><th>File</th><th>Status</th><th>Expiry</th></tr>
            </thead>
            <tbody>
              {documents?.length === 0 && (
                <tr><td colSpan={5}>No documents linked to this project.</td></tr>
              )}
              {documents?.map((doc) => (
                <tr key={doc.id}>
                  <td className={tableStyles.name}>{doc.title}</td>
                  <td>{doc.category}</td>
                  <td>{doc.fileName ?? '—'}</td>
                  <td>{doc.status}</td>
                  <td>{doc.expiryDate ?? '—'}</td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}

      {tab === 'Activity' && id && <ProjectActivityFeed projectId={id} />}
    </div>
  )
}
