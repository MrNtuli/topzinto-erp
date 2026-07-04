import { useEffect, useState } from 'react'
import { useNavigate, useParams } from 'react-router-dom'
import { useQuery, useMutation } from '@tanstack/react-query'
import { getEmployee, updateEmployee } from '@/api/employees'
import { getProjects } from '@/api/projects'
import styles from '../site-reports/AddSiteReportPage.module.css'

export function EditEmployeePage() {
  const { id } = useParams<{ id: string }>()
  const navigate = useNavigate()
  const { data, isLoading } = useQuery({ queryKey: ['employee', id], queryFn: () => getEmployee(id!), enabled: !!id })
  const { data: projects } = useQuery({ queryKey: ['projects'], queryFn: () => getProjects() })

  const [employeeNumber, setEmployeeNumber] = useState('')
  const [firstName, setFirstName] = useState('')
  const [lastName, setLastName] = useState('')
  const [idNumber, setIdNumber] = useState('')
  const [jobTitle, setJobTitle] = useState('')
  const [department, setDepartment] = useState('Site')
  const [trade, setTrade] = useState('General')
  const [status, setStatus] = useState('Active')
  const [phone, setPhone] = useState('')
  const [email, setEmail] = useState('')
  const [hireDate, setHireDate] = useState('')
  const [terminationDate, setTerminationDate] = useState('')
  const [assignedProjectId, setAssignedProjectId] = useState('')
  const [hourlyRate, setHourlyRate] = useState('')
  const [notes, setNotes] = useState('')

  useEffect(() => {
    if (!data) return
    setEmployeeNumber(data.employeeNumber)
    setFirstName(data.firstName)
    setLastName(data.lastName)
    setIdNumber(data.idNumber ?? '')
    setJobTitle(data.jobTitle)
    setDepartment(data.department)
    setTrade(data.trade)
    setStatus(data.status)
    setPhone(data.phone ?? '')
    setEmail(data.email ?? '')
    setHireDate(data.hireDate)
    setTerminationDate(data.terminationDate ?? '')
    setAssignedProjectId(data.assignedProjectId ?? '')
    setHourlyRate(data.hourlyRate != null ? String(data.hourlyRate) : '')
    setNotes(data.notes ?? '')
  }, [data])

  const mutation = useMutation({
    mutationFn: () =>
      updateEmployee(id!, {
        employeeNumber,
        firstName,
        lastName,
        idNumber: idNumber || undefined,
        jobTitle,
        department,
        trade,
        status,
        phone: phone || undefined,
        email: email || undefined,
        hireDate,
        terminationDate: terminationDate || undefined,
        assignedProjectId: assignedProjectId || undefined,
        hourlyRate: hourlyRate ? parseFloat(hourlyRate) : undefined,
        notes: notes || undefined,
      }),
    onSuccess: () => navigate(`/employees/${id}`),
  })

  function handleSubmit(e: React.FormEvent) {
    e.preventDefault()
    if (!employeeNumber.trim() || !firstName.trim() || !lastName.trim() || !jobTitle.trim()) return
    mutation.mutate()
  }

  if (isLoading) return <p>Loading...</p>

  return (
    <div className={styles.page}>
      <nav className={styles.breadcrumb}>Home &gt; Employees &gt; Edit</nav>
      <h1>Edit Employee</h1>

      <form className={styles.form} onSubmit={handleSubmit}>
        <div className={styles.row}>
          <label>
            Employee Number *
            <input value={employeeNumber} onChange={(e) => setEmployeeNumber(e.target.value)} required />
          </label>
          <label>
            Hire Date *
            <input type="date" value={hireDate} onChange={(e) => setHireDate(e.target.value)} required />
          </label>
          <label>
            Status
            <select value={status} onChange={(e) => setStatus(e.target.value)}>
              <option value="Active">Active</option>
              <option value="On Leave">On Leave</option>
              <option value="Suspended">Suspended</option>
              <option value="Terminated">Terminated</option>
            </select>
          </label>
        </div>

        <div className={styles.row}>
          <label>
            First Name *
            <input value={firstName} onChange={(e) => setFirstName(e.target.value)} required />
          </label>
          <label>
            Last Name *
            <input value={lastName} onChange={(e) => setLastName(e.target.value)} required />
          </label>
          <label>
            ID Number
            <input value={idNumber} onChange={(e) => setIdNumber(e.target.value)} />
          </label>
        </div>

        <div className={styles.row}>
          <label>
            Job Title *
            <input value={jobTitle} onChange={(e) => setJobTitle(e.target.value)} required />
          </label>
          <label>
            Department
            <select value={department} onChange={(e) => setDepartment(e.target.value)}>
              <option value="Site">Site</option>
              <option value="Administration">Administration</option>
              <option value="Fleet">Fleet</option>
              <option value="Procurement">Procurement</option>
              <option value="Finance">Finance</option>
              <option value="Management">Management</option>
              <option value="Safety">Safety</option>
            </select>
          </label>
          <label>
            Trade
            <select value={trade} onChange={(e) => setTrade(e.target.value)}>
              <option value="General">General</option>
              <option value="Bricklayer">Bricklayer</option>
              <option value="Carpenter">Carpenter</option>
              <option value="Electrician">Electrician</option>
              <option value="Plumber">Plumber</option>
              <option value="Steel Fixer">Steel Fixer</option>
              <option value="Operator">Operator</option>
              <option value="Driver">Driver</option>
              <option value="Supervisor">Supervisor</option>
              <option value="Foreman">Foreman</option>
              <option value="Other">Other</option>
            </select>
          </label>
        </div>

        <div className={styles.row}>
          <label>
            Phone
            <input value={phone} onChange={(e) => setPhone(e.target.value)} />
          </label>
          <label>
            Email
            <input type="email" value={email} onChange={(e) => setEmail(e.target.value)} />
          </label>
          <label>
            Hourly Rate (R)
            <input type="number" min="0" step="0.01" value={hourlyRate} onChange={(e) => setHourlyRate(e.target.value)} />
          </label>
        </div>

        {status === 'Terminated' && (
          <label>
            Termination Date
            <input type="date" value={terminationDate} onChange={(e) => setTerminationDate(e.target.value)} />
          </label>
        )}

        <label>
          Assigned Project
          <select value={assignedProjectId} onChange={(e) => setAssignedProjectId(e.target.value)}>
            <option value="">— None —</option>
            {projects?.map((p) => (
              <option key={p.id} value={p.id}>{p.code} — {p.name}</option>
            ))}
          </select>
        </label>

        <label>
          Notes
          <textarea value={notes} onChange={(e) => setNotes(e.target.value)} rows={3} />
        </label>

        {mutation.isError && <p>Failed to update employee.</p>}

        <div className={styles.actions}>
          <button type="button" onClick={() => navigate(`/employees/${id}`)}>Cancel</button>
          <button type="submit" disabled={mutation.isPending}>
            {mutation.isPending ? 'Saving...' : 'Save Changes'}
          </button>
        </div>
      </form>
    </div>
  )
}
