import { useQuery } from '@tanstack/react-query'
import { useParams, Link } from 'react-router-dom'
import { getEmployee } from '@/api/employees'
import styles from '../clients/ClientDetailPage.module.css'

export function EmployeeDetailPage() {
  const { id } = useParams<{ id: string }>()
  const { data, isLoading, isError } = useQuery({
    queryKey: ['employee', id],
    queryFn: () => getEmployee(id!),
    enabled: !!id,
    retry: false,
  })

  if (isLoading) return <p>Loading...</p>
  if (isError || !data) return <p>Employee not found.</p>

  return (
    <div className={styles.page}>
      <nav className={styles.breadcrumb}>
        <Link to="/employees">Employees</Link> &gt; {data.firstName} {data.lastName}
      </nav>
      <header className={styles.header}>
        <h1>{data.firstName} {data.lastName}</h1>
        <span className={styles.badge}>{data.status}</span>
        <div className={styles.headerActions}>
          <Link to={`/employees/${id}/edit`} className={styles.editBtn}>Edit</Link>
        </div>
      </header>

      <div className={styles.grid}>
        <div className={styles.card}>
          <h3>Employment</h3>
          <dl>
            <dt>Employee Number</dt><dd>{data.employeeNumber}</dd>
            <dt>Job Title</dt><dd>{data.jobTitle}</dd>
            <dt>Department</dt><dd>{data.department}</dd>
            <dt>Trade</dt><dd>{data.trade}</dd>
            <dt>Hire Date</dt><dd>{data.hireDate}</dd>
            {data.terminationDate && (
              <>
                <dt>Termination Date</dt><dd>{data.terminationDate}</dd>
              </>
            )}
            {data.hourlyRate != null && (
              <>
                <dt>Hourly Rate</dt><dd>R {data.hourlyRate.toFixed(2)}</dd>
              </>
            )}
          </dl>
        </div>
        <div className={styles.card}>
          <h3>Contact</h3>
          <dl>
            <dt>Phone</dt><dd>{data.phone ?? '—'}</dd>
            <dt>Email</dt><dd>{data.email ?? '—'}</dd>
            <dt>ID Number</dt><dd>{data.idNumber ?? '—'}</dd>
          </dl>
        </div>
        <div className={styles.card}>
          <h3>Assignment</h3>
          <dl>
            <dt>Project</dt>
            <dd>
              {data.assignedProjectId ? (
                <Link to={`/projects/${data.assignedProjectId}`}>{data.assignedProjectName}</Link>
              ) : '—'}
            </dd>
          </dl>
          {data.notes && (
            <>
              <h3>Notes</h3>
              <p>{data.notes}</p>
            </>
          )}
        </div>
      </div>
    </div>
  )
}
