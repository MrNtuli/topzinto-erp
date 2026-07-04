import { useEffect, useState } from 'react'
import { useNavigate, useParams } from 'react-router-dom'
import { useQuery, useMutation } from '@tanstack/react-query'
import { getSiteReport, updateSiteReport } from '@/api/siteReports'
import styles from '../site-reports/AddSiteReportPage.module.css'

export function EditSiteReportPage() {
  const { id } = useParams<{ id: string }>()
  const navigate = useNavigate()
  const { data, isLoading } = useQuery({ queryKey: ['site-report', id], queryFn: () => getSiteReport(id!), enabled: !!id })

  const [weather, setWeather] = useState('')
  const [temperature, setTemperature] = useState('')
  const [windSpeed, setWindSpeed] = useState('')
  const [personnelCount, setPersonnelCount] = useState(0)
  const [workCompleted, setWorkCompleted] = useState('')
  const [workPlanned, setWorkPlanned] = useState('')
  const [delaysIssues, setDelaysIssues] = useState('')
  const [notes, setNotes] = useState('')

  useEffect(() => {
    if (!data) return
    setWeather(data.weather ?? '')
    setTemperature(data.temperature ?? '')
    setWindSpeed(data.windSpeed ?? '')
    setPersonnelCount(data.personnelCount ?? 0)
    setWorkCompleted(data.workCompleted)
    setWorkPlanned(data.workPlanned ?? '')
    setDelaysIssues(data.delaysIssues ?? '')
    setNotes(data.notes ?? '')
  }, [data])

  const mutation = useMutation({
    mutationFn: (submit: boolean) =>
      updateSiteReport(id!, {
        weather: weather || undefined,
        temperature: temperature || undefined,
        windSpeed: windSpeed || undefined,
        personnelCount,
        workCompleted,
        workPlanned: workPlanned || undefined,
        delaysIssues: delaysIssues || undefined,
        notes: notes || undefined,
        submit,
      }),
    onSuccess: () => navigate(`/site-reports/${id}`),
  })

  if (isLoading) return <p>Loading...</p>
  if (!data) return <p>Site report not found.</p>

  const isDraft = data.status === 'Draft'

  return (
    <div className={styles.page}>
      <nav className={styles.breadcrumb}>Home &gt; Site Reports &gt; Edit</nav>
      <h1>Edit Site Report — {data.projectName}</h1>
      <p style={{ marginBottom: 16, color: 'var(--color-text-muted)' }}>Date: {data.reportDate} · Status: {data.status}</p>

      <form className={styles.form}>
        <fieldset className={styles.fieldset}>
          <legend>Weather</legend>
          <div className={styles.row}>
            <label>Weather<input value={weather} onChange={(e) => setWeather(e.target.value)} /></label>
            <label>Temperature<input value={temperature} onChange={(e) => setTemperature(e.target.value)} /></label>
            <label>Wind<input value={windSpeed} onChange={(e) => setWindSpeed(e.target.value)} /></label>
            <label>Personnel<input type="number" value={personnelCount} onChange={(e) => setPersonnelCount(+e.target.value)} /></label>
          </div>
        </fieldset>
        <label>Work Completed *<textarea rows={4} value={workCompleted} onChange={(e) => setWorkCompleted(e.target.value)} required /></label>
        <label>Work Planned<textarea rows={3} value={workPlanned} onChange={(e) => setWorkPlanned(e.target.value)} /></label>
        <label>Delays / Issues<textarea rows={3} value={delaysIssues} onChange={(e) => setDelaysIssues(e.target.value)} /></label>
        <label>Notes<textarea rows={2} value={notes} onChange={(e) => setNotes(e.target.value)} /></label>
        <div className={styles.actions}>
          <button type="button" className={styles.draft} onClick={() => navigate(`/site-reports/${id}`)}>Cancel</button>
          {isDraft && (
            <button type="button" className={styles.draft} onClick={() => mutation.mutate(false)} disabled={mutation.isPending}>
              Save Draft
            </button>
          )}
          <button
            type="button"
            className={styles.submit}
            onClick={() => mutation.mutate(isDraft)}
            disabled={mutation.isPending || !workCompleted.trim()}
          >
            {isDraft ? 'Submit Report' : 'Save Changes'}
          </button>
        </div>
      </form>
    </div>
  )
}
