import { useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { useQuery, useMutation } from '@tanstack/react-query'
import { getClients } from '@/api/clients'
import { createProject } from '@/api/projects'
import styles from '../site-reports/AddSiteReportPage.module.css'

export function AddProjectPage() {
  const navigate = useNavigate()
  const { data: clients } = useQuery({ queryKey: ['clients'], queryFn: () => getClients() })

  const [code, setCode] = useState('')
  const [name, setName] = useState('')
  const [clientId, setClientId] = useState('')
  const [status, setStatus] = useState('Planned')
  const [progress, setProgress] = useState(0)
  const [startDate, setStartDate] = useState('')
  const [endDate, setEndDate] = useState('')
  const [contractValue, setContractValue] = useState('')
  const [budget, setBudget] = useState('')
  const [siteLocation, setSiteLocation] = useState('')
  const [description, setDescription] = useState('')

  const mutation = useMutation({
    mutationFn: () =>
      createProject({
        code,
        name,
        clientId,
        status,
        progress,
        startDate: startDate || undefined,
        endDate: endDate || undefined,
        contractValue: Number(contractValue) || 0,
        budget: Number(budget) || 0,
        siteLocation: siteLocation || undefined,
        description: description || undefined,
      }),
    onSuccess: (project) => navigate(`/projects/${project.id}`),
  })

  function handleSubmit(e: React.FormEvent) {
    e.preventDefault()
    if (!code.trim() || !name.trim() || !clientId) return
    mutation.mutate()
  }

  return (
    <div className={styles.page}>
      <nav className={styles.breadcrumb}>Home &gt; Projects &gt; New</nav>
      <h1>New Project</h1>

      <form className={styles.form} onSubmit={handleSubmit}>
        <div className={styles.row}>
          <label>
            Project Code *
            <input value={code} onChange={(e) => setCode(e.target.value)} placeholder="PRJ-006" required />
          </label>
          <label>
            Status
            <select value={status} onChange={(e) => setStatus(e.target.value)}>
              <option value="Planned">Planned</option>
              <option value="In Progress">In Progress</option>
              <option value="On Hold">On Hold</option>
              <option value="Completed">Completed</option>
            </select>
          </label>
          <label>
            Progress %
            <input type="number" min={0} max={100} value={progress} onChange={(e) => setProgress(+e.target.value)} />
          </label>
        </div>

        <label>
          Project Name *
          <input value={name} onChange={(e) => setName(e.target.value)} required />
        </label>

        <label>
          Client *
          <select value={clientId} onChange={(e) => setClientId(e.target.value)} required>
            <option value="">Select client</option>
            {clients?.map((c) => (
              <option key={c.id} value={c.id}>{c.name}</option>
            ))}
          </select>
        </label>

        <div className={styles.row}>
          <label>
            Contract Value (ZAR)
            <input type="number" min={0} value={contractValue} onChange={(e) => setContractValue(e.target.value)} />
          </label>
          <label>
            Budget (ZAR)
            <input type="number" min={0} value={budget} onChange={(e) => setBudget(e.target.value)} />
          </label>
          <label>
            Start Date
            <input type="date" value={startDate} onChange={(e) => setStartDate(e.target.value)} />
          </label>
          <label>
            End Date
            <input type="date" value={endDate} onChange={(e) => setEndDate(e.target.value)} />
          </label>
        </div>

        <label>
          Site Location
          <input value={siteLocation} onChange={(e) => setSiteLocation(e.target.value)} />
        </label>

        <label>
          Description
          <textarea rows={3} value={description} onChange={(e) => setDescription(e.target.value)} />
        </label>

        <div className={styles.actions}>
          <button type="button" className={styles.draft} onClick={() => navigate('/projects')}>Cancel</button>
          <button type="submit" className={styles.submit} disabled={mutation.isPending}>
            {mutation.isPending ? 'Saving...' : 'Create Project'}
          </button>
        </div>
      </form>
    </div>
  )
}
