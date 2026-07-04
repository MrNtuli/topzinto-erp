import { useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { useQuery, useMutation } from '@tanstack/react-query'
import { getClients } from '@/api/clients'
import { getProjects } from '@/api/projects'
import { createContract } from '@/api/contracts'
import styles from '../site-reports/AddSiteReportPage.module.css'

export function AddContractPage() {
  const navigate = useNavigate()
  const { data: clients } = useQuery({ queryKey: ['clients'], queryFn: () => getClients() })
  const { data: projects } = useQuery({ queryKey: ['projects'], queryFn: () => getProjects() })

  const [contractNumber, setContractNumber] = useState('')
  const [title, setTitle] = useState('')
  const [clientId, setClientId] = useState('')
  const [projectId, setProjectId] = useState('')
  const [value, setValue] = useState('')
  const [retentionPercent, setRetentionPercent] = useState('5')
  const [status, setStatus] = useState('Draft')
  const [startDate, setStartDate] = useState('')
  const [endDate, setEndDate] = useState('')
  const [notes, setNotes] = useState('')

  const mutation = useMutation({
    mutationFn: () =>
      createContract({
        contractNumber,
        title,
        clientId,
        projectId: projectId || undefined,
        value: Number(value) || 0,
        retentionPercent: Number(retentionPercent) || 0,
        status,
        startDate: startDate || undefined,
        endDate: endDate || undefined,
        notes: notes || undefined,
      }),
    onSuccess: () => navigate('/contracts'),
  })

  function handleSubmit(e: React.FormEvent) {
    e.preventDefault()
    if (!contractNumber.trim() || !title.trim() || !clientId) return
    mutation.mutate()
  }

  return (
    <div className={styles.page}>
      <nav className={styles.breadcrumb}>Home &gt; Contracts &gt; New</nav>
      <h1>New Contract</h1>

      <form className={styles.form} onSubmit={handleSubmit}>
        <div className={styles.row}>
          <label>
            Contract Number *
            <input value={contractNumber} onChange={(e) => setContractNumber(e.target.value)} placeholder="CNT-2024-003" required />
          </label>
          <label>
            Status
            <select value={status} onChange={(e) => setStatus(e.target.value)}>
              <option value="Draft">Draft</option>
              <option value="Active">Active</option>
              <option value="Completed">Completed</option>
              <option value="Terminated">Terminated</option>
            </select>
          </label>
        </div>

        <label>
          Title *
          <input value={title} onChange={(e) => setTitle(e.target.value)} required />
        </label>

        <div className={styles.row}>
          <label>
            Client *
            <select value={clientId} onChange={(e) => setClientId(e.target.value)} required>
              <option value="">Select client</option>
              {clients?.map((c) => (
                <option key={c.id} value={c.id}>{c.name}</option>
              ))}
            </select>
          </label>
          <label>
            Linked Project
            <select value={projectId} onChange={(e) => setProjectId(e.target.value)}>
              <option value="">None</option>
              {projects?.map((p) => (
                <option key={p.id} value={p.id}>{p.code} — {p.name}</option>
              ))}
            </select>
          </label>
        </div>

        <div className={styles.row}>
          <label>
            Contract Value (ZAR)
            <input type="number" min={0} value={value} onChange={(e) => setValue(e.target.value)} />
          </label>
          <label>
            Retention %
            <input type="number" min={0} max={100} step="0.1" value={retentionPercent} onChange={(e) => setRetentionPercent(e.target.value)} />
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
          Notes
          <textarea rows={2} value={notes} onChange={(e) => setNotes(e.target.value)} />
        </label>

        <div className={styles.actions}>
          <button type="button" className={styles.draft} onClick={() => navigate('/contracts')}>Cancel</button>
          <button type="submit" className={styles.submit} disabled={mutation.isPending}>
            {mutation.isPending ? 'Saving...' : 'Create Contract'}
          </button>
        </div>
      </form>
    </div>
  )
}
