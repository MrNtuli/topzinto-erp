import { useQuery } from '@tanstack/react-query'
import { useState } from 'react'
import { Link } from 'react-router-dom'
import { Plus } from 'lucide-react'
import { getEmployees } from '@/api/employees'
import styles from '../projects/ProjectsPage.module.css'
import localStyles from '../clients/ClientsPage.module.css'

export function EmployeesPage() {
  const [search, setSearch] = useState('')
  const [status, setStatus] = useState('')
  const [department, setDepartment] = useState('')

  const { data, isLoading, isError } = useQuery({
    queryKey: ['employees', search, status, department],
    queryFn: () => getEmployees({
      search: search || undefined,
      status: status || undefined,
      department: department || undefined,
    }),
    retry: false,
  })

  const activeCount = data?.filter((e) => e.status === 'Active').length ?? 0

  return (
    <div className={styles.page}>
      <nav className={styles.breadcrumb}>Home &gt; Employees</nav>
      <header className={styles.header}>
        <h1>Employees</h1>
        <Link to="/employees/new" className={styles.newBtn}>
          <Plus size={18} />
          New Employee
        </Link>
      </header>

      <div className={styles.filters}>
        <input type="search" placeholder="Search employees..." value={search} onChange={(e) => setSearch(e.target.value)} />
        <select value={status} onChange={(e) => setStatus(e.target.value)}>
          <option value="">All Status</option>
          <option value="Active">Active</option>
          <option value="On Leave">On Leave</option>
          <option value="Suspended">Suspended</option>
          <option value="Terminated">Terminated</option>
        </select>
        <select value={department} onChange={(e) => setDepartment(e.target.value)}>
          <option value="">All Departments</option>
          <option value="Site">Site</option>
          <option value="Administration">Administration</option>
          <option value="Fleet">Fleet</option>
          <option value="Procurement">Procurement</option>
          <option value="Finance">Finance</option>
          <option value="Management">Management</option>
          <option value="Safety">Safety</option>
        </select>
      </div>

      {!isLoading && data && (
        <p className={styles.hint}>{data.length} employees · {activeCount} active</p>
      )}

      <div className={styles.tableWrap}>
        {isLoading && <p className={styles.loading}>Loading employees...</p>}
        {isError && <p className={styles.loading}>Could not load employees.</p>}
        <table className={styles.table}>
          <thead>
            <tr>
              <th>Number</th>
              <th>Name</th>
              <th>Job Title</th>
              <th>Department</th>
              <th>Trade</th>
              <th>Project</th>
              <th>Phone</th>
              <th>Status</th>
            </tr>
          </thead>
          <tbody>
            {data?.map((e) => (
              <tr key={e.id}>
                <td className={styles.name}>{e.employeeNumber}</td>
                <td>
                  <Link to={`/employees/${e.id}`} className={localStyles.link}>{e.fullName}</Link>
                </td>
                <td>{e.jobTitle}</td>
                <td>{e.department}</td>
                <td>{e.trade}</td>
                <td>{e.assignedProjectName ?? '—'}</td>
                <td>{e.phone ?? '—'}</td>
                <td><span className={styles.badge}>{e.status}</span></td>
              </tr>
            ))}
          </tbody>
        </table>
        {!isLoading && !isError && data?.length === 0 && (
          <p className={styles.loading}>No employees found.</p>
        )}
      </div>
    </div>
  )
}
