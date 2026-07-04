import { useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { useQuery, useMutation } from '@tanstack/react-query'
import { getProjects } from '@/api/projects'
import { createClaim } from '@/api/boq'
import styles from '../site-reports/AddSiteReportPage.module.css'

export function AddClaimPage() {
  const navigate = useNavigate()
  const { data: projects } = useQuery({ queryKey: ['projects'], queryFn: () => getProjects() })

  const [projectId, setProjectId] = useState('')
  const [claimNumber, setClaimNumber] = useState('')
  const [title, setTitle] = useState('')
  const [claimDate, setClaimDate] = useState(new Date().toISOString().slice(0, 10))
  const [periodFrom, setPeriodFrom] = useState('')
  const [periodTo, setPeriodTo] = useState('')
  const [amount, setAmount] = useState('')
  const [status, setStatus] = useState('Draft')
  const [submittedByName, setSubmittedByName] = useState('')
  const [notes, setNotes] = useState('')

  const mutation = useMutation({
    mutationFn: () =>
      createClaim({
        projectId,
        claimNumber,
        title,
        claimDate,
        periodFrom: periodFrom || undefined,
        periodTo: periodTo || undefined,
        amount: parseFloat(amount) || 0,
        status,
        submittedByName: submittedByName || undefined,
        notes: notes || undefined,
      }),
    onSuccess: () => navigate('/boq'),
  })

  function handleSubmit(e: React.FormEvent) {
    e.preventDefault()
    if (!projectId || !claimNumber.trim() || !title.trim()) return
    mutation.mutate()
  }

  return (
    <div className={styles.page}>
      <nav className={styles.breadcrumb}>Home &gt; BOQ &gt; New Claim</nav>
      <h1>New Progress Claim</h1>

      <form className={styles.form} onSubmit={handleSubmit}>
        <div className={styles.row}>
          <label>
            Claim Number *
            <input value={claimNumber} onChange={(e) => setClaimNumber(e.target.value)} placeholder="CLM-2024-004" required />
          </label>
          <label>
            Status
            <select value={status} onChange={(e) => setStatus(e.target.value)}>
              <option value="Draft">Draft</option>
              <option value="Submitted">Submitted</option>
              <option value="Approved">Approved</option>
              <option value="Paid">Paid</option>
              <option value="Rejected">Rejected</option>
            </select>
          </label>
          <label>
            Claim Date *
            <input type="date" value={claimDate} onChange={(e) => setClaimDate(e.target.value)} required />
          </label>
        </div>

        <label>
          Title *
          <input value={title} onChange={(e) => setTitle(e.target.value)} required />
        </label>

        <label>
          Project *
          <select value={projectId} onChange={(e) => setProjectId(e.target.value)} required>
            <option value="">Select project</option>
            {projects?.map((p) => (
              <option key={p.id} value={p.id}>{p.code} — {p.name}</option>
            ))}
          </select>
        </label>

        <div className={styles.row}>
          <label>
            Period From
            <input type="date" value={periodFrom} onChange={(e) => setPeriodFrom(e.target.value)} />
          </label>
          <label>
            Period To
            <input type="date" value={periodTo} onChange={(e) => setPeriodTo(e.target.value)} />
          </label>
          <label>
            Amount (ZAR) *
            <input type="number" min="0" value={amount} onChange={(e) => setAmount(e.target.value)} required />
          </label>
        </div>

        <label>
          Submitted By
          <input value={submittedByName} onChange={(e) => setSubmittedByName(e.target.value)} placeholder="QS / PM name" />
        </label>

        <label>
          Notes
          <textarea rows={2} value={notes} onChange={(e) => setNotes(e.target.value)} />
        </label>

        <div className={styles.actions}>
          <button type="button" className={styles.draft} onClick={() => navigate('/boq')}>Cancel</button>
          <button type="submit" className={styles.submit} disabled={mutation.isPending}>
            {mutation.isPending ? 'Saving...' : 'Create Claim'}
          </button>
        </div>
      </form>
    </div>
  )
}
