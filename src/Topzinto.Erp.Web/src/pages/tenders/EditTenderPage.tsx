import { useEffect, useState } from 'react'
import { useNavigate, useParams } from 'react-router-dom'
import { useQuery, useMutation } from '@tanstack/react-query'
import { getClients } from '@/api/clients'
import { getTender, updateTender } from '@/api/tenders'
import { parseDisplayDate } from '@/lib/parseDisplayDate'
import styles from '../site-reports/AddSiteReportPage.module.css'

export function EditTenderPage() {
  const { id } = useParams<{ id: string }>()
  const navigate = useNavigate()
  const { data, isLoading } = useQuery({ queryKey: ['tender', id], queryFn: () => getTender(id!), enabled: !!id })
  const { data: clients } = useQuery({ queryKey: ['clients'], queryFn: () => getClients() })

  const [referenceNumber, setReferenceNumber] = useState('')
  const [title, setTitle] = useState('')
  const [clientId, setClientId] = useState('')
  const [closingDate, setClosingDate] = useState('')
  const [status, setStatus] = useState('Identified')
  const [estimatedValue, setEstimatedValue] = useState('')
  const [notes, setNotes] = useState('')

  useEffect(() => {
    if (!data) return
    setReferenceNumber(data.referenceNumber)
    setTitle(data.title)
    setClientId(data.clientId)
    setClosingDate(parseDisplayDate(data.closingDate))
    setStatus(data.status)
    setEstimatedValue(String(data.estimatedValue))
    setNotes(data.notes ?? '')
  }, [data])

  const mutation = useMutation({
    mutationFn: () =>
      updateTender(id!, {
        referenceNumber,
        title,
        clientId,
        closingDate: closingDate ? new Date(closingDate).toISOString() : new Date().toISOString(),
        status,
        estimatedValue: Number(estimatedValue) || 0,
        notes: notes || undefined,
      }),
    onSuccess: () => navigate(`/tenders/${id}`),
  })

  function handleSubmit(e: React.FormEvent) {
    e.preventDefault()
    if (!referenceNumber.trim() || !title.trim() || !clientId || !closingDate) return
    mutation.mutate()
  }

  if (isLoading) return <p>Loading...</p>

  return (
    <div className={styles.page}>
      <nav className={styles.breadcrumb}>Home &gt; Tenders &gt; Edit</nav>
      <h1>Edit Tender</h1>

      <form className={styles.form} onSubmit={handleSubmit}>
        <div className={styles.row}>
          <label>
            Reference Number *
            <input value={referenceNumber} onChange={(e) => setReferenceNumber(e.target.value)} required />
          </label>
          <label>
            Status
            <select value={status} onChange={(e) => setStatus(e.target.value)}>
              <option value="Identified">Identified</option>
              <option value="Preparing">Preparing</option>
              <option value="Submitted">Submitted</option>
              <option value="Won">Won</option>
              <option value="Lost">Lost</option>
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
            Closing Date *
            <input type="date" value={closingDate} onChange={(e) => setClosingDate(e.target.value)} required />
          </label>
          <label>
            Estimated Value (ZAR)
            <input type="number" min={0} value={estimatedValue} onChange={(e) => setEstimatedValue(e.target.value)} />
          </label>
        </div>

        <label>
          Notes
          <textarea rows={3} value={notes} onChange={(e) => setNotes(e.target.value)} />
        </label>

        <div className={styles.actions}>
          <button type="button" className={styles.draft} onClick={() => navigate(`/tenders/${id}`)}>Cancel</button>
          <button type="submit" className={styles.submit} disabled={mutation.isPending}>
            {mutation.isPending ? 'Saving...' : 'Save Changes'}
          </button>
        </div>
      </form>
    </div>
  )
}
