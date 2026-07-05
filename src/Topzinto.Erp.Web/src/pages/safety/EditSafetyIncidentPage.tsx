import { useEffect, useState } from 'react'
import { useNavigate, useParams } from 'react-router-dom'
import { useQuery, useMutation } from '@tanstack/react-query'
import { getProjects } from '@/api/projects'
import {
  getSafetyIncident,
  updateSafetyIncident,
  SAFETY_SEVERITIES,
  SAFETY_STATUSES,
} from '@/api/safety'
import styles from '../site-reports/AddSiteReportPage.module.css'

export function EditSafetyIncidentPage() {
  const { id } = useParams<{ id: string }>()
  const navigate = useNavigate()

  const { data, isLoading, isError } = useQuery({
    queryKey: ['safety-incident', id],
    queryFn: () => getSafetyIncident(id!),
    enabled: !!id,
  })

  const { data: projects } = useQuery({ queryKey: ['projects'], queryFn: () => getProjects() })

  const [projectId, setProjectId] = useState('')
  const [incidentDate, setIncidentDate] = useState('')
  const [title, setTitle] = useState('')
  const [description, setDescription] = useState('')
  const [severity, setSeverity] = useState('Medium')
  const [status, setStatus] = useState('Reported')
  const [location, setLocation] = useState('')
  const [reportedByName, setReportedByName] = useState('')
  const [correctiveAction, setCorrectiveAction] = useState('')

  useEffect(() => {
    if (!data) return
    setProjectId(data.projectId)
    setIncidentDate(parseIncidentDate(data.incidentDate))
    setTitle(data.title)
    setDescription(data.description)
    setSeverity(data.severity)
    setStatus(data.status)
    setLocation(data.location ?? '')
    setReportedByName(data.reportedByName ?? '')
    setCorrectiveAction(data.correctiveAction ?? '')
  }, [data])

  const mutation = useMutation({
    mutationFn: () =>
      updateSafetyIncident(id!, {
        projectId,
        incidentDate,
        title,
        description,
        severity,
        status,
        location: location || undefined,
        reportedByName: reportedByName || undefined,
        correctiveAction: correctiveAction || undefined,
      }),
    onSuccess: () => navigate(`/safety/${id}`),
  })

  if (isLoading) return <p>Loading...</p>
  if (isError || !data) return <p>Incident not found.</p>

  return (
    <div className={styles.page}>
      <nav className={styles.breadcrumb}>Home &gt; Safety &gt; Edit Incident</nav>
      <h1>Edit Safety Incident</h1>

      <form className={styles.form} onSubmit={(e) => { e.preventDefault(); mutation.mutate() }}>
        <div className={styles.row}>
          <label>
            Project *
            <select value={projectId} onChange={(e) => setProjectId(e.target.value)} required>
              <option value="">Select project</option>
              {projects?.map((p) => (
                <option key={p.id} value={p.id}>{p.name}</option>
              ))}
            </select>
          </label>
          <label>
            Incident Date *
            <input type="date" value={incidentDate} onChange={(e) => setIncidentDate(e.target.value)} required />
          </label>
        </div>

        <label>
          Title *
          <input value={title} onChange={(e) => setTitle(e.target.value)} required />
        </label>

        <label>
          Description *
          <textarea rows={4} value={description} onChange={(e) => setDescription(e.target.value)} required />
        </label>

        <div className={styles.row}>
          <label>
            Severity
            <select value={severity} onChange={(e) => setSeverity(e.target.value)}>
              {SAFETY_SEVERITIES.map((s) => (
                <option key={s} value={s}>{s}</option>
              ))}
            </select>
          </label>
          <label>
            Status
            <select value={status} onChange={(e) => setStatus(e.target.value)}>
              {SAFETY_STATUSES.map((s) => (
                <option key={s} value={s}>{s}</option>
              ))}
            </select>
          </label>
          <label>
            Location
            <input value={location} onChange={(e) => setLocation(e.target.value)} />
          </label>
        </div>

        <label>
          Reported By
          <input value={reportedByName} onChange={(e) => setReportedByName(e.target.value)} />
        </label>

        <label>
          Corrective Action
          <textarea rows={3} value={correctiveAction} onChange={(e) => setCorrectiveAction(e.target.value)} />
        </label>

        <div className={styles.actions}>
          <button type="button" className={styles.draft} onClick={() => navigate(`/safety/${id}`)}>Cancel</button>
          <button type="submit" className={styles.submit} disabled={mutation.isPending}>
            {mutation.isPending ? 'Saving...' : 'Save Changes'}
          </button>
        </div>
      </form>
    </div>
  )
}

/** Convert API display date (e.g. "05 Jul 2026") to yyyy-MM-dd for date input. */
function parseIncidentDate(display: string): string {
  const parsed = new Date(display)
  if (!Number.isNaN(parsed.getTime())) return parsed.toISOString().slice(0, 10)
  return new Date().toISOString().slice(0, 10)
}
