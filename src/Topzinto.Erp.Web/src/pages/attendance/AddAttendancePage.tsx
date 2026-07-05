import { useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { useQuery, useMutation } from '@tanstack/react-query'
import { getEmployees } from '@/api/employees'
import { getProjects } from '@/api/projects'
import { createAttendanceRecord, ATTENDANCE_STATUSES } from '@/api/attendance'
import styles from '../site-reports/AddSiteReportPage.module.css'

export function AddAttendancePage() {
  const navigate = useNavigate()
  const { data: employees } = useQuery({ queryKey: ['employees'], queryFn: () => getEmployees({ status: 'Active' }) })
  const { data: projects } = useQuery({ queryKey: ['projects'], queryFn: () => getProjects() })

  const [employeeId, setEmployeeId] = useState('')
  const [projectId, setProjectId] = useState('')
  const [workDate, setWorkDate] = useState(new Date().toISOString().slice(0, 10))
  const [status, setStatus] = useState('Present')
  const [checkInTime, setCheckInTime] = useState('07:00')
  const [checkOutTime, setCheckOutTime] = useState('16:00')
  const [hoursWorked, setHoursWorked] = useState('8')
  const [notes, setNotes] = useState('')
  const [error, setError] = useState('')

  const mutation = useMutation({
    mutationFn: () =>
      createAttendanceRecord({
        employeeId,
        projectId: projectId || undefined,
        workDate,
        status,
        checkInTime: checkInTime || undefined,
        checkOutTime: checkOutTime || undefined,
        hoursWorked: hoursWorked ? parseFloat(hoursWorked) : undefined,
        notes: notes || undefined,
      }),
    onSuccess: () => navigate('/attendance'),
    onError: (err: Error) => setError(err.message),
  })

  function handleSubmit(e: React.FormEvent) {
    e.preventDefault()
    if (!employeeId || !workDate) return
    setError('')
    mutation.mutate()
  }

  return (
    <div className={styles.page}>
      <nav className={styles.breadcrumb}>Home &gt; Attendance &gt; Record</nav>
      <h1>Record Attendance</h1>

      <form className={styles.form} onSubmit={handleSubmit}>
        <div className={styles.row}>
          <label>
            Employee *
            <select value={employeeId} onChange={(e) => setEmployeeId(e.target.value)} required>
              <option value="">Select employee</option>
              {employees?.map((e) => (
                <option key={e.id} value={e.id}>{e.fullName} — {e.jobTitle}</option>
              ))}
            </select>
          </label>
          <label>
            Project
            <select value={projectId} onChange={(e) => setProjectId(e.target.value)}>
              <option value="">No project</option>
              {projects?.map((p) => (
                <option key={p.id} value={p.id}>{p.code} — {p.name}</option>
              ))}
            </select>
          </label>
        </div>

        <div className={styles.row}>
          <label>
            Work Date *
            <input type="date" value={workDate} onChange={(e) => setWorkDate(e.target.value)} required />
          </label>
          <label>
            Status
            <select value={status} onChange={(e) => setStatus(e.target.value)}>
              {ATTENDANCE_STATUSES.map((s) => (
                <option key={s} value={s}>{s === 'HalfDay' ? 'Half Day' : s === 'OnLeave' ? 'On Leave' : s === 'PublicHoliday' ? 'Public Holiday' : s}</option>
              ))}
            </select>
          </label>
        </div>

        <div className={styles.row}>
          <label>
            Check In
            <input type="time" value={checkInTime} onChange={(e) => setCheckInTime(e.target.value)} />
          </label>
          <label>
            Check Out
            <input type="time" value={checkOutTime} onChange={(e) => setCheckOutTime(e.target.value)} />
          </label>
          <label>
            Hours Worked
            <input type="number" min="0" max="24" step="0.5" value={hoursWorked} onChange={(e) => setHoursWorked(e.target.value)} />
          </label>
        </div>

        <label>
          Notes
          <textarea rows={2} value={notes} onChange={(e) => setNotes(e.target.value)} />
        </label>

        {error && <p role="alert">{error}</p>}

        <div className={styles.actions}>
          <button type="button" className={styles.draft} onClick={() => navigate('/attendance')}>Cancel</button>
          <button type="submit" className={styles.submit} disabled={mutation.isPending}>
            {mutation.isPending ? 'Saving...' : 'Save Attendance'}
          </button>
        </div>
      </form>
    </div>
  )
}
