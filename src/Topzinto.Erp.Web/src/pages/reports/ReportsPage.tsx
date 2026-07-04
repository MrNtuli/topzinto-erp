import { useQuery } from '@tanstack/react-query'
import { Link } from 'react-router-dom'
import { BarChart3, ChevronRight, Download } from 'lucide-react'
import { getReportsHub } from '@/api/reports'
import {
  exportProjectsCsv,
  exportBoqCsv,
  exportClaimsCsv,
  exportSuppliersCsv,
  exportProcurementCsv,
  exportInvoicesCsv,
  exportFleetCsv,
  exportDocumentsCsv,
  exportEmployeesCsv,
  exportTimesheetsCsv,
  exportProjectsExcel,
  exportSuppliersExcel,
  exportProcurementExcel,
  exportEmployeesExcel,
} from '@/api/export'
import styles from './ReportsPage.module.css'
import pageStyles from '../projects/ProjectsPage.module.css'

const csvExports = [
  { label: 'Projects', fn: exportProjectsCsv },
  { label: 'BOQ Items', fn: exportBoqCsv },
  { label: 'Claims', fn: exportClaimsCsv },
  { label: 'Invoices', fn: exportInvoicesCsv },
  { label: 'Procurement', fn: exportProcurementCsv },
  { label: 'Suppliers', fn: exportSuppliersCsv },
  { label: 'Fleet', fn: exportFleetCsv },
  { label: 'Documents', fn: exportDocumentsCsv },
  { label: 'Employees', fn: exportEmployeesCsv },
  { label: 'Timesheets', fn: exportTimesheetsCsv },
]

const excelExports = [
  { label: 'Projects', fn: exportProjectsExcel },
  { label: 'Suppliers', fn: exportSuppliersExcel },
  { label: 'Procurement (PO + lines)', fn: exportProcurementExcel },
  { label: 'Employees', fn: exportEmployeesExcel },
]

export function ReportsPage() {
  const { data, isLoading, isError } = useQuery({
    queryKey: ['reports-hub'],
    queryFn: getReportsHub,
    retry: false,
  })

  return (
    <div className={pageStyles.page}>
      <nav className={pageStyles.breadcrumb}>Home &gt; Reports</nav>
      <header className={pageStyles.header}><h1>Reports Hub</h1></header>

      <p className={styles.intro}>
        Live summary reports across TopZinto operations. CSV and Excel exports available.
      </p>

      <div className={styles.exportRow}>
        <h3>Excel Exports</h3>
        <div className={styles.exportBtns}>
          {excelExports.map(({ label, fn }) => (
            <button key={label} type="button" className={styles.exportBtn} onClick={() => fn().catch(() => {})}>
              <Download size={16} /> {label}
            </button>
          ))}
        </div>
      </div>

      <div className={styles.exportRow}>
        <h3>CSV Exports</h3>
        <div className={styles.exportBtns}>
          {csvExports.map(({ label, fn }) => (
            <button key={label} type="button" className={styles.exportBtn} onClick={() => fn().catch(() => {})}>
              <Download size={16} /> {label}
            </button>
          ))}
        </div>
      </div>

      {isLoading && <p className={pageStyles.loading}>Loading reports...</p>}
      <div className={styles.grid}>
        {data?.reports.map((report) => (
          <Link key={report.id} to={report.link} className={styles.card}>
            <div className={styles.icon}><BarChart3 size={24} /></div>
            <div className={styles.content}>
              <h3>{report.title}</h3>
              <p>{report.description}</p>
              <span className={styles.value}>{report.value}</span>
            </div>
            <ChevronRight size={20} className={styles.arrow} />
          </Link>
        ))}
      </div>
      {isError && <p className={pageStyles.hint}>Start the API to load reports.</p>}
    </div>
  )
}
