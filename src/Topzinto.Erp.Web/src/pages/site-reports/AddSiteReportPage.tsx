import { useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { useQuery, useMutation } from '@tanstack/react-query'
import { getProjects } from '@/api/projects'
import { createSiteReport } from '@/api/siteReports'
import styles from './AddSiteReportPage.module.css'

export function AddSiteReportPage() {
  const navigate = useNavigate()
  const { data: projects } = useQuery({ queryKey: ['projects'], queryFn: () => getProjects() })

  const [projectId, setProjectId] = useState('')
  const [weather, setWeather] = useState('Sunny')
  const [temperature, setTemperature] = useState('28°C')
  const [windSpeed, setWindSpeed] = useState('12 km/h')
  const [personnelCount, setPersonnelCount] = useState(24)
  const [workCompleted, setWorkCompleted] = useState('')
  const [workPlanned, setWorkPlanned] = useState('')
  const [delaysIssues, setDelaysIssues] = useState('')
  const [notes, setNotes] = useState('')

  const mutation = useMutation({
    mutationFn: (submit: boolean) =>
      createSiteReport({
        projectId,
        reportDate: new Date().toISOString(),
        weather,
        temperature,
        windSpeed,
        personnelCount,
        workCompleted,
        workPlanned: workPlanned || undefined,
        delaysIssues: delaysIssues || undefined,
        notes: notes || undefined,
        submit,
      }),
    onSuccess: () => navigate('/site-reports'),
  })

  function handleSubmit(e: React.FormEvent, submit: boolean) {
    e.preventDefault()
    if (!projectId || !workCompleted) return
    mutation.mutate(submit)
  }

  return (
    <div className={styles.page}>
      <nav className={styles.breadcrumb}>Home &gt; Site Reports &gt; Add New</nav>
      <h1>Add Site Report</h1>

      <form className={styles.form}>
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
            Date
            <input type="date" defaultValue={new Date().toISOString().slice(0, 10)} readOnly />
          </label>
        </div>

        <fieldset className={styles.fieldset}>
          <legend>Weather Conditions</legend>
          <div className={styles.row}>
            <label>Weather<input value={weather} onChange={(e) => setWeather(e.target.value)} /></label>
            <label>Temperature<input value={temperature} onChange={(e) => setTemperature(e.target.value)} /></label>
            <label>Wind<input value={windSpeed} onChange={(e) => setWindSpeed(e.target.value)} /></label>
            <label>Personnel<input type="number" value={personnelCount} onChange={(e) => setPersonnelCount(+e.target.value)} /></label>
          </div>
        </fieldset>

        <label>
          Work Completed Today *
          <textarea rows={4} value={workCompleted} onChange={(e) => setWorkCompleted(e.target.value)} required />
        </label>
        <label>
          Work Planned for Tomorrow
          <textarea rows={3} value={workPlanned} onChange={(e) => setWorkPlanned(e.target.value)} />
        </label>
        <label>
          Delays / Issues
          <textarea rows={3} value={delaysIssues} onChange={(e) => setDelaysIssues(e.target.value)} />
        </label>
        <label>
          Notes
          <textarea rows={2} value={notes} onChange={(e) => setNotes(e.target.value)} />
        </label>

        <div className={styles.actions}>
          <button type="button" className={styles.draft} onClick={(e) => handleSubmit(e, false)} disabled={mutation.isPending}>
            Save Draft
          </button>
          <button type="button" className={styles.submit} onClick={(e) => handleSubmit(e, true)} disabled={mutation.isPending}>
            Submit Report
          </button>
        </div>
      </form>
    </div>
  )
}
