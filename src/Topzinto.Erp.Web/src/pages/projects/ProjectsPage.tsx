import { useQuery } from '@tanstack/react-query'
import { useState } from 'react'
import { Link } from 'react-router-dom'
import { ChevronRight, Plus } from 'lucide-react'
import { getProjects, formatCurrency } from '@/api/projects'
import styles from './ProjectsPage.module.css'

const FALLBACK = [
  { id: '1', name: 'Ridgeview Mall Construction', client: 'Private', status: 'In Progress', progress: 68, endDate: '15 Dec 2024', value: 'R45,600,000' },
]

export function ProjectsPage() {
  const [search, setSearch] = useState('')
  const [status, setStatus] = useState('')

  const { data, isLoading, isError } = useQuery({
    queryKey: ['projects', search, status],
    queryFn: () => getProjects({ search: search || undefined, status: status || undefined }),
    retry: false,
  })

  const projects = isError || !data?.length ? null : data

  return (
    <div className={styles.page}>
      <nav className={styles.breadcrumb}>Home &gt; Projects</nav>
      <header className={styles.header}>
        <h1>Projects</h1>
        <Link to="/projects/new" className={styles.newBtn}>
          <Plus size={18} />
          New Project
        </Link>
      </header>

      <div className={styles.filters}>
        <input
          type="search"
          placeholder="Search projects..."
          value={search}
          onChange={(e) => setSearch(e.target.value)}
        />
        <select value={status} onChange={(e) => setStatus(e.target.value)}>
          <option value="">All Status</option>
          <option value="In Progress">In Progress</option>
          <option value="Completed">Completed</option>
          <option value="On Hold">On Hold</option>
        </select>
      </div>

      <div className={styles.tableWrap}>
        {isLoading && <p className={styles.loading}>Loading projects...</p>}
        <table className={styles.table}>
          <thead>
            <tr>
              <th>Project Name</th>
              <th>Client</th>
              <th>Status</th>
              <th>Progress</th>
              <th>End Date</th>
              <th>Contract Value</th>
              <th></th>
            </tr>
          </thead>
          <tbody>
            {projects
              ? projects.map((p) => (
                  <tr key={p.id}>
                    <td className={styles.name}>{p.name}</td>
                    <td>{p.clientName}</td>
                    <td>
                      <span className={`${styles.badge} ${p.status === 'Completed' ? styles.completed : styles.active}`}>
                        {p.status}
                      </span>
                    </td>
                    <td>
                      <div className={styles.progress}>
                        <div className={styles.progressBar}>
                          <div style={{ width: `${p.progress}%` }} />
                        </div>
                        <span>{p.progress}%</span>
                      </div>
                    </td>
                    <td>{p.endDate ?? '—'}</td>
                    <td className={styles.value}>{formatCurrency(p.contractValue)}</td>
                    <td>
                      <Link to={`/projects/${p.id}`} className={styles.action} aria-label="View project">
                        <ChevronRight size={18} />
                      </Link>
                    </td>
                  </tr>
                ))
              : FALLBACK.map((p) => (
                  <tr key={p.name}>
                    <td className={styles.name}>{p.name}</td>
                    <td>{p.client}</td>
                    <td>
                      <span className={`${styles.badge} ${p.status === 'Completed' ? styles.completed : styles.active}`}>
                        {p.status}
                      </span>
                    </td>
                    <td>
                      <div className={styles.progress}>
                        <div className={styles.progressBar}>
                          <div style={{ width: `${p.progress}%` }} />
                        </div>
                        <span>{p.progress}%</span>
                      </div>
                    </td>
                    <td>{p.endDate}</td>
                    <td className={styles.value}>{p.value}</td>
                    <td><ChevronRight size={18} className={styles.action} /></td>
                  </tr>
                ))}
          </tbody>
        </table>
        {isError && <p className={styles.hint}>Showing sample data — start API to load live projects.</p>}
      </div>
    </div>
  )
}
