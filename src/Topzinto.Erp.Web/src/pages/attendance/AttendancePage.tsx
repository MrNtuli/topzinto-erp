import { useState } from 'react'
import { useQuery } from '@tanstack/react-query'
import { Link } from 'react-router-dom'
import { Plus } from 'lucide-react'
import { getAttendanceRecords, ATTENDANCE_STATUSES } from '@/api/attendance'
import { getProjects } from '@/api/projects'
import { getEmployees } from '@/api/employees'
import styles from '../projects/ProjectsPage.module.css'
import localStyles from './AttendancePage.module.css'

function statusClass(status: string) {
  switch (status) {
    case 'Absent': return localStyles.statusAbsent
    case 'Late': return localStyles.statusLate
    case 'HalfDay': return localStyles.statusHalfDay
    case 'OnLeave': return localStyles.statusOnLeave
    case 'PublicHoliday': return localStyles.statusPublicHoliday
    default: return localStyles.statusPresent
  }
}

function formatStatusLabel(status: string) {
  switch (status) {
    case 'HalfDay': return 'Half Day'
    case 'OnLeave': return 'On Leave'
    case 'PublicHoliday': return 'Public Holiday'
    default: return status
  }
}

export function AttendancePage() {
  const [projectId, setProjectId] = useState('')
  const [employeeId, setEmployeeId] = useState('')
  const [status, setStatus] = useState('')
  const [fromDate, setFromDate] = useState('')
  const [toDate, setToDate] = useState('')

  const { data: projects } = useQuery({ queryKey: ['projects'], queryFn: () => getProjects(), retry: false })
  const { data: employees } = useQuery({
    queryKey: ['employees'],
    queryFn: () => getEmployees({ status: 'Active' }),
    retry: false,
  })

  const { data, isLoading, isError } = useQuery({
    queryKey: ['attendance', projectId, employeeId, status, fromDate, toDate],
    queryFn: () => getAttendanceRecords({
      projectId: projectId || undefined,
      employeeId: employeeId || undefined,
      status: status || undefined,
      fromDate: fromDate || undefined,
      toDate: toDate || undefined,
    }),
    retry: false,
  })

  const present = data?.filter((r) => r.status === 'Present').length ?? 0
  const absent = data?.filter((r) => r.status === 'Absent').length ?? 0
  const onLeave = data?.filter((r) => r.status === 'OnLeave').length ?? 0

  return (
    <div className={styles.page}>
      <nav className={styles.breadcrumb}>Home &gt; Attendance</nav>
      <header className={styles.header}>
        <h1>Attendance</h1>
        <Link to="/attendance/new" className={styles.newBtn}>
          <Plus size={18} />
          Record Attendance
        </Link>
      </header>

      <div className={styles.filters}>
        <select value={projectId} onChange={(e) => setProjectId(e.target.value)}>
          <option value="">All Projects</option>
          {projects?.map((p) => (
            <option key={p.id} value={p.id}>{p.code} — {p.name}</option>
          ))}
        </select>
        <select value={employeeId} onChange={(e) => setEmployeeId(e.target.value)}>
          <option value="">All Employees</option>
          {employees?.map((e) => (
            <option key={e.id} value={e.id}>{e.fullName}</option>
          ))}
        </select>
        <select value={status} onChange={(e) => setStatus(e.target.value)}>
          <option value="">All Status</option>
          {ATTENDANCE_STATUSES.map((s) => (
            <option key={s} value={s}>{formatStatusLabel(s)}</option>
          ))}
        </select>
        <input
          type="date"
          value={fromDate}
          onChange={(e) => setFromDate(e.target.value)}
          aria-label="From date"
        />
        <input
          type="date"
          value={toDate}
          onChange={(e) => setToDate(e.target.value)}
          aria-label="To date"
        />
      </div>

      <div className={styles.tableWrap}>
        {isLoading && <p className={styles.loading}>Loading attendance records...</p>}
        {!isLoading && !isError && data?.length === 0 && (
          <p className={localStyles.empty}>No attendance records found.</p>
        )}

        <table className={`${styles.table} ${localStyles.desktopTable}`}>
          <thead>
            <tr>
              <th>Date</th>
              <th>Employee</th>
              <th>Project</th>
              <th>Check In</th>
              <th>Check Out</th>
              <th>Hours</th>
              <th>Status</th>
            </tr>
          </thead>
          <tbody>
            {data?.map((r) => (
              <tr key={r.id}>
                <td>
                  <Link to={`/attendance/${r.id}/edit`} className={localStyles.link}>{r.workDate}</Link>
                </td>
                <td>{r.employeeName}</td>
                <td>{r.projectName ?? '—'}</td>
                <td>{r.checkInTime ?? '—'}</td>
                <td>{r.checkOutTime ?? '—'}</td>
                <td>{r.hoursWorked ?? '—'}</td>
                <td>
                  <span className={`${styles.badge} ${statusClass(r.status)}`}>
                    {formatStatusLabel(r.status)}
                  </span>
                </td>
              </tr>
            ))}
          </tbody>
        </table>

        <div className={localStyles.mobileCards}>
          {data?.map((r) => (
            <article key={r.id} className={localStyles.mobileCard}>
              <div className={localStyles.mobileCardHeader}>
                <Link to={`/attendance/${r.id}/edit`} className={localStyles.link}>
                  <strong>{r.employeeName}</strong>
                </Link>
                <span className={`${styles.badge} ${statusClass(r.status)}`}>
                  {formatStatusLabel(r.status)}
                </span>
              </div>
              <p className={localStyles.mobileMeta}>{r.workDate} · {r.projectName ?? 'No project'}</p>
              <p className={localStyles.mobileMeta}>
                {r.checkInTime ? `In ${r.checkInTime}` : '—'}
                {r.checkOutTime ? ` · Out ${r.checkOutTime}` : ''}
                {r.hoursWorked != null ? ` · ${r.hoursWorked}h` : ''}
              </p>
            </article>
          ))}
        </div>

        {isError && <p className={styles.hint}>Start the API to load attendance records.</p>}
      </div>

      {data && data.length > 0 && (
        <p className={styles.hint} style={{ marginTop: 16 }}>
          Summary (filtered): {present} present · {absent} absent · {onLeave} on leave · {data.length} total
        </p>
      )}
    </div>
  )
}
