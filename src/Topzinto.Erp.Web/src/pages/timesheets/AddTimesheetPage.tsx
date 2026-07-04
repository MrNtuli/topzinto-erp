import { useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { useQuery, useMutation } from '@tanstack/react-query'
import { getEmployees } from '@/api/employees'
import { getProjects } from '@/api/projects'
import { createTimesheet } from '@/api/timesheets'
import styles from '../site-reports/AddSiteReportPage.module.css'

export function AddTimesheetPage() {
  const navigate = useNavigate()
  const { data: employees } = useQuery({ queryKey: ['employees'], queryFn: () => getEmployees({ status: 'Active' }) })
  const { data: projects } = useQuery({ queryKey: ['projects'], queryFn: () => getProjects() })

  const [employeeId, setEmployeeId] = useState('')
  const [projectId, setProjectId] = useState('')
  const [workDate, setWorkDate] = useState(new Date().toISOString().slice(0, 10))
  const [hours, setHours] = useState('8')
  const [status, setStatus] = useState('Submitted')
  const [description, setDescription] = useState('')
  const [notes, setNotes] = useState('')

  const mutation = useMutation({
    mutationFn: () =>
      createTimesheet({
        employeeId,
        projectId,
        workDate,
        hours: parseFloat(hours) || 0,
        status,
        description: description || undefined,
        notes: notes || undefined,
      }),
    onSuccess: () => navigate('/timesheets'),
  })

  function handleSubmit(e: React.FormEvent) {
    e.preventDefault()
    if (!employeeId || !projectId || !workDate) return
    mutation.mutate()
  }

  return (
    <div className={styles.page}>
      <nav className={styles.breadcrumb}>Home &gt; Timesheets &gt; Log Hours</nav>
      <h1>Log Timesheet Hours</h1>

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
            Project *
            <select value={projectId} onChange={(e) => setProjectId(e.target.value)} required>
              <option value="">Select project</option>
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
            Hours *
            <input type="number" min="0.5" max="24" step="0.5" value={hours} onChange={(e) => setHours(e.target.value)} required />
          </label>
          <label>
            Status
            <select value={status} onChange={(e) => setStatus(e.target.value)}>
              <option value="Draft">Draft</option>
              <option value="Submitted">Submitted</option>
              <option value="Approved">Approved</option>
            </select>
          </label>
        </div>

        <label>
          Work Description
          <input value={description} onChange={(e) => setDescription(e.target.value)} placeholder="e.g. Bricklaying - north wing" />
        </label>

        <label>
          Notes
          <textarea rows={2} value={notes} onChange={(e) => setNotes(e.target.value)} />
        </label>

        <div className={styles.actions}>
          <button type="button" className={styles.draft} onClick={() => navigate('/timesheets')}>Cancel</button>
          <button type="submit" className={styles.submit} disabled={mutation.isPending}>
            {mutation.isPending ? 'Saving...' : 'Save Timesheet'}
          </button>
        </div>
      </form>
    </div>
  )
}
