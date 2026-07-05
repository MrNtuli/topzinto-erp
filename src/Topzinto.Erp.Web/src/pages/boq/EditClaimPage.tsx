import { useEffect, useState } from 'react'
import { useNavigate, useParams } from 'react-router-dom'
import { useMutation, useQuery } from '@tanstack/react-query'
import { getClaim, updateClaim } from '@/api/boq'
import { parseDisplayDate } from '@/lib/parseDisplayDate'
import styles from '../site-reports/AddSiteReportPage.module.css'

export function EditClaimPage() {
  const { id } = useParams<{ id: string }>()
  const navigate = useNavigate()
  const { data, isLoading, isError } = useQuery({
    queryKey: ['claim', id],
    queryFn: () => getClaim(id!),
    enabled: !!id,
  })

  const [claimNumber, setClaimNumber] = useState('')
  const [title, setTitle] = useState('')
  const [claimDate, setClaimDate] = useState('')
  const [periodFrom, setPeriodFrom] = useState('')
  const [periodTo, setPeriodTo] = useState('')
  const [amount, setAmount] = useState('')
  const [status, setStatus] = useState('Draft')
  const [submittedByName, setSubmittedByName] = useState('')
  const [notes, setNotes] = useState('')

  useEffect(() => {
    if (!data) return
    setClaimNumber(data.claimNumber)
    setTitle(data.title)
    setClaimDate(parseDisplayDate(data.claimDate))
    setPeriodFrom(data.periodFrom ? parseDisplayDate(data.periodFrom) : '')
    setPeriodTo(data.periodTo ? parseDisplayDate(data.periodTo) : '')
    setAmount(String(data.amount))
    setStatus(data.status)
    setSubmittedByName(data.submittedByName ?? '')
    setNotes(data.notes ?? '')
  }, [data])

  const mutation = useMutation({
    mutationFn: () =>
      updateClaim(id!, {
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
    if (!claimNumber.trim() || !title.trim() || !claimDate) return
    mutation.mutate()
  }

  if (isLoading) return <p className={styles.page}>Loading claim...</p>
  if (isError || !data) return <p className={styles.page}>Claim not found.</p>

  return (
    <div className={styles.page}>
      <nav className={styles.breadcrumb}>Home &gt; BOQ &gt; Edit Claim</nav>
      <h1>Edit Progress Claim</h1>

      <form className={styles.form} onSubmit={handleSubmit}>
        <label>
          Project (read-only)
          <input value={data.projectName} disabled />
        </label>

        <div className={styles.row}>
          <label>
            Claim Number *
            <input value={claimNumber} onChange={(e) => setClaimNumber(e.target.value)} required />
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
          <input value={submittedByName} onChange={(e) => setSubmittedByName(e.target.value)} />
        </label>

        <label>
          Notes
          <textarea rows={2} value={notes} onChange={(e) => setNotes(e.target.value)} />
        </label>

        {mutation.isError && (
          <p style={{ color: 'var(--color-danger, #c0392b)', fontSize: 14 }}>
            Failed to save changes. Check your connection and try again.
          </p>
        )}

        <div className={styles.actions}>
          <button type="button" className={styles.draft} onClick={() => navigate('/boq')}>Cancel</button>
          <button type="submit" className={styles.submit} disabled={mutation.isPending}>
            {mutation.isPending ? 'Saving...' : 'Save Changes'}
          </button>
        </div>
      </form>
    </div>
  )
}
