import { useQuery } from '@tanstack/react-query'
import { Link } from 'react-router-dom'
import { FolderKanban, FileText, ScrollText, Receipt, Truck, Wrench, Files, AlertTriangle, RefreshCw } from 'lucide-react'
import { KpiCard } from '@/components/ui/KpiCard'
import { getDashboard, formatCurrency } from '@/api/dashboard'
import { useAuthStore } from '@/stores/authStore'
import styles from './DashboardPage.module.css'

function greetingForHour(hour: number) {
  if (hour < 12) return 'Good Morning'
  if (hour < 17) return 'Good Afternoon'
  return 'Good Evening'
}

function formatUpdatedAt(timestamp: number) {
  return new Date(timestamp).toLocaleTimeString('en-ZA', { hour: '2-digit', minute: '2-digit' })
}

function buildDonutStyle(completed: number, inProgress: number, onHold: number, planned: number) {
  const total = completed + inProgress + onHold + planned || 1
  const c = (completed / total) * 360
  const p = c + (inProgress / total) * 360
  const h = p + (onHold / total) * 360
  return {
    background: `conic-gradient(#22c55e 0deg ${c}deg, #f26522 ${c}deg ${p}deg, #f59e0b ${p}deg ${h}deg, #94a3b8 ${h}deg 360deg)`,
  }
}

function barHeight(value: number, max: number) {
  if (max <= 0) return 4
  return Math.max(4, Math.round((value / max) * 100))
}

export function DashboardPage() {
  const user = useAuthStore((s) => s.user)
  const { data, isLoading, isError, isFetching, dataUpdatedAt, refetch } = useQuery({
    queryKey: ['dashboard'],
    queryFn: () => getDashboard(false),
    retry: false,
    staleTime: 60_000,
    refetchOnWindowFocus: true,
  })

  async function handleRefresh() {
    await getDashboard(true)
    await refetch()
  }

  const today = new Date().toLocaleDateString('en-ZA', {
    weekday: 'long',
    day: 'numeric',
    month: 'long',
    year: 'numeric',
  })

  const firstName = user?.firstName ?? 'User'
  const greeting = greetingForHour(new Date().getHours())
  const progress = data?.projectProgress
  const trend = data?.financialTrend ?? []
  const trendMax = Math.max(...trend.map((m) => Math.max(m.claimsAmount, m.procurementAmount)), 1)

  const kpis = data ? [
    { label: 'Active Projects', value: String(data.activeProjects), icon: FolderKanban, color: 'blue' as const, link: '/projects' },
    { label: 'Active Tenders', value: String(data.activeTenders), icon: FileText, color: 'orange' as const, link: '/tenders' },
    { label: 'Total Contracts', value: formatCurrency(data.totalContractValue), icon: ScrollText, color: 'blue' as const, link: '/contracts' },
    { label: 'Pending Claims', value: formatCurrency(data.pendingClaimsValue), icon: Receipt, color: 'orange' as const, link: '/boq' },
    { label: 'Fleet in Use', value: `${data.fleetInUse} / ${data.fleetTotal}`, icon: Truck, color: 'green' as const, link: '/fleet' },
    { label: 'Equipment in Use', value: `${data.equipmentInUse} / ${data.equipmentTotal}`, icon: Wrench, color: 'orange' as const, link: '/equipment' },
    { label: 'Docs Expiring', value: String(data.documentsExpiringSoon), icon: Files, color: 'red' as const, link: '/documents' },
    { label: 'Overdue Tasks', value: String(data.overdueTasks), icon: AlertTriangle, color: 'red' as const, link: '/schedule' },
  ] : []

  return (
    <div className={styles.page}>
      <header className={styles.header}>
        <div>
          <p className={styles.breadcrumb}>Dashboard</p>
          <h1>{greeting}, {firstName}</h1>
        </div>
        <div className={styles.headerActions}>
          {dataUpdatedAt > 0 && (
            <span className={styles.updatedAt}>
              Updated {formatUpdatedAt(dataUpdatedAt)}
            </span>
          )}
          <button
            type="button"
            className={styles.refreshBtn}
            onClick={() => handleRefresh()}
            disabled={isFetching}
            title="Refresh dashboard"
          >
            <RefreshCw size={16} className={isFetching ? styles.spinning : undefined} />
            {isFetching ? 'Refreshing...' : 'Refresh'}
          </button>
          <span className={styles.date}>{today}</span>
        </div>
      </header>

      {isLoading && <p>Loading dashboard...</p>}
      {isError && <p className={styles.hint}>Start the API to load live dashboard data.</p>}

      <div className={styles.kpiGrid}>
        {kpis.map((kpi) => (
          <KpiCard key={kpi.label} {...kpi} />
        ))}
      </div>

      <div className={styles.charts}>
        <div className={styles.chartCard}>
          <h3>Projects Progress Overview</h3>
          <div className={styles.chartPlaceholder}>
            <div
              className={styles.donut}
              style={progress ? buildDonutStyle(progress.completed, progress.inProgress, progress.onHold, progress.planned) : undefined}
            />
            <ul className={styles.legend}>
              <li><span className={styles.dot} style={{ background: '#22c55e' }} /> Completed ({progress?.completed ?? 0})</li>
              <li><span className={styles.dot} style={{ background: '#f26522' }} /> In Progress ({progress?.inProgress ?? 0})</li>
              <li><span className={styles.dot} style={{ background: '#f59e0b' }} /> On Hold ({progress?.onHold ?? 0})</li>
              <li><span className={styles.dot} style={{ background: '#94a3b8' }} /> Planned ({progress?.planned ?? 0})</li>
            </ul>
          </div>
        </div>
        <div className={styles.chartCard}>
          <h3>Latest Site Reports</h3>
          {data?.latestSiteReports.length === 0 ? (
            <p className={styles.empty}>No site reports yet.</p>
          ) : (
            <ul className={styles.reportList}>
              {data?.latestSiteReports.map((r) => (
                <li key={r.id}>
                  <Link to={`/site-reports/${r.id}`}>
                    <strong>{r.projectName}</strong>
                    <span>{r.reportDate} · {r.status}</span>
                  </Link>
                </li>
              ))}
            </ul>
          )}
          <Link to="/site-reports" className={styles.viewLink}>View all site reports</Link>
        </div>
      </div>

      <div className={styles.financialSection}>
        <div className={styles.chartCard}>
          <h3>Financial Activity (Last 6 Months)</h3>
          <p className={styles.chartSub}>Claims submitted vs procurement spend — live from ERP data</p>
          {trend.length === 0 ? (
            <p className={styles.empty}>No financial data yet.</p>
          ) : (
            <>
              <div className={styles.financialChart}>
                {trend.map((m) => (
                  <div key={m.label} className={styles.monthCol}>
                    <div className={styles.barPair}>
                      <div
                        className={styles.barClaims}
                        style={{ height: `${barHeight(m.claimsAmount, trendMax)}%` }}
                        title={`Claims: ${formatCurrency(m.claimsAmount)}`}
                      />
                      <div
                        className={styles.barProcurement}
                        style={{ height: `${barHeight(m.procurementAmount, trendMax)}%` }}
                        title={`Procurement: ${formatCurrency(m.procurementAmount)}`}
                      />
                    </div>
                    <span className={styles.monthLabel}>{m.label}</span>
                  </div>
                ))}
              </div>
              <ul className={styles.finLegend}>
                <li><span className={styles.dot} style={{ background: 'var(--color-orange)' }} /> Claims</li>
                <li><span className={styles.dot} style={{ background: 'var(--color-navy)' }} /> Procurement</li>
              </ul>
            </>
          )}
        </div>
      </div>
    </div>
  )
}
