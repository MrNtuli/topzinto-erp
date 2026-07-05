import { useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { useQuery, useMutation } from '@tanstack/react-query'
import { getProjects } from '@/api/projects'
import { createSafetyIncident, SAFETY_SEVERITIES, SAFETY_STATUSES } from '@/api/safety'
import styles from '../site-reports/AddSiteReportPage.module.css'

export function AddSafetyIncidentPage() {
  const navigate = useNavigate()
  const { data: projects } = useQuery({ queryKey: ['projects'], queryFn: () => getProjects() })

  const [projectId, setProjectId] = useState('')
  const [incidentDate, setIncidentDate] = useState(new Date().toISOString().slice(0, 10))
  const [title, setTitle] = useState('')
  const [description, setDescription] = useState('')
  const [severity, setSeverity] = useState('Medium')
  const [status, setStatus] = useState('Reported')
  const [location, setLocation] = useState('')
  const [reportedByName, setReportedByName] = useState('')
  const [correctiveAction, setCorrectiveAction] = useState('')

  const mutation = useMutation({
    mutationFn: () =>
      createSafetyIncident({
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
    onSuccess: (incident) => navigate(`/safety/${incident.id}`),
  })

  function handleSubmit(e: React.FormEvent) {
    e.preventDefault()
    if (!projectId || !title.trim() || !description.trim()) return
    mutation.mutate()
  }

  return (
    <div className={styles.page}>
      <nav className={styles.breadcrumb}>Home &gt; Safety &gt; Report Incident</nav>
      <h1>Report Safety Incident</h1>

      <form className={styles.form} onSubmit={handleSubmit}>
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
          <input value={title} onChange={(e) => setTitle(e.target.value)} placeholder="Brief incident summary" required />
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
            <input value={location} onChange={(e) => setLocation(e.target.value)} placeholder="Site area or zone" />
          </label>
        </div>

        <label>
          Reported By
          <input value={reportedByName} onChange={(e) => setReportedByName(e.target.value)} placeholder="Name of reporter" />
        </label>

        <label>
          Corrective Action
          <textarea rows={3} value={correctiveAction} onChange={(e) => setCorrectiveAction(e.target.value)} placeholder="Immediate actions taken or planned" />
        </label>

        <div className={styles.actions}>
          <button type="button" className={styles.draft} onClick={() => navigate('/safety')}>Cancel</button>
          <button type="submit" className={styles.submit} disabled={mutation.isPending}>
            {mutation.isPending ? 'Saving...' : 'Report Incident'}
          </button>
        </div>
      </form>
    </div>
  )
}
