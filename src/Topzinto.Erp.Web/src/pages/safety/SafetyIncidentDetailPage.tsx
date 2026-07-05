import { useState } from 'react'
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { useParams, Link, useNavigate } from 'react-router-dom'
import { deleteSafetyIncident, getSafetyIncident } from '@/api/safety'
import styles from '../clients/ClientDetailPage.module.css'
import listStyles from './SafetyIncidentsPage.module.css'

function severityClass(severity: string) {
  switch (severity) {
    case 'Low': return listStyles.severityLow
    case 'High': return listStyles.severityHigh
    case 'Critical': return listStyles.severityCritical
    default: return listStyles.severityMedium
  }
}

function statusClass(status: string) {
  switch (status) {
    case 'Reported': return listStyles.statusReported
    case 'Investigating': return listStyles.statusInvestigating
    case 'Resolved': return listStyles.statusResolved
    case 'Closed': return listStyles.statusClosed
    default: return listStyles.statusReported
  }
}

export function SafetyIncidentDetailPage() {
  const { id } = useParams<{ id: string }>()
  const navigate = useNavigate()
  const queryClient = useQueryClient()
  const [confirmDelete, setConfirmDelete] = useState(false)

  const { data, isLoading, isError } = useQuery({
    queryKey: ['safety-incident', id],
    queryFn: () => getSafetyIncident(id!),
    enabled: !!id,
    retry: false,
  })

  const deleteMutation = useMutation({
    mutationFn: () => deleteSafetyIncident(id!),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['safety-incidents'] })
      navigate('/safety')
    },
  })

  if (isLoading) return <p>Loading...</p>
  if (isError || !data) return <p>Incident not found.</p>

  return (
    <div className={styles.page}>
      <nav className={styles.breadcrumb}>
        <Link to="/safety">Safety Incidents</Link> &gt; {data.title}
      </nav>
      <header className={styles.header}>
        <h1>{data.title}</h1>
        <span className={`${styles.badge} ${severityClass(data.severity)}`}>{data.severity}</span>
        <span className={`${styles.badge} ${statusClass(data.status)}`}>{data.status}</span>
        <div className={styles.headerActions}>
          <Link to={`/safety/${id}/edit`} className={styles.editBtn}>Edit</Link>
          {!confirmDelete ? (
            <button type="button" className={styles.editBtn} onClick={() => setConfirmDelete(true)}>Delete</button>
          ) : (
            <button
              type="button"
              className={styles.pdfBtn}
              disabled={deleteMutation.isPending}
              onClick={() => deleteMutation.mutate()}
            >
              {deleteMutation.isPending ? 'Deleting...' : 'Confirm Delete'}
            </button>
          )}
        </div>
      </header>

      <div className={styles.grid}>
        <div className={styles.card}>
          <h3>Incident Details</h3>
          <dl>
            <dt>Project</dt><dd>{data.projectName}</dd>
            <dt>Date</dt><dd>{data.incidentDate}</dd>
            <dt>Location</dt><dd>{data.location ?? '—'}</dd>
            <dt>Reported By</dt><dd>{data.reportedByName ?? '—'}</dd>
          </dl>
        </div>
        <div className={styles.card}>
          <h3>Description</h3>
          <p>{data.description}</p>
        </div>
        <div className={styles.card}>
          <h3>Corrective Action</h3>
          <p>{data.correctiveAction ?? 'No corrective action recorded.'}</p>
        </div>
      </div>
    </div>
  )
}
