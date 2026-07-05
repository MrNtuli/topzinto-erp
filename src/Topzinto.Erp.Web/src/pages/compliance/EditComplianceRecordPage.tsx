import { useEffect, useState } from 'react'
import { useNavigate, useParams } from 'react-router-dom'
import { useQuery, useMutation } from '@tanstack/react-query'
import { getProjects } from '@/api/projects'
import {
  getComplianceRecord,
  updateComplianceRecord,
  COMPLIANCE_TYPES,
  COMPLIANCE_STATUSES,
} from '@/api/compliance'
import styles from '../site-reports/AddSiteReportPage.module.css'

export function EditComplianceRecordPage() {
  const { id } = useParams<{ id: string }>()
  const navigate = useNavigate()

  const { data, isLoading, isError } = useQuery({
    queryKey: ['compliance-record', id],
    queryFn: () => getComplianceRecord(id!),
    enabled: !!id,
  })

  const { data: projects } = useQuery({ queryKey: ['projects'], queryFn: () => getProjects() })

  const [title, setTitle] = useState('')
  const [type, setType] = useState('Certificate')
  const [entityType, setEntityType] = useState('')
  const [projectId, setProjectId] = useState('')
  const [issueDate, setIssueDate] = useState('')
  const [expiryDate, setExpiryDate] = useState('')
  const [status, setStatus] = useState('Valid')
  const [responsiblePerson, setResponsiblePerson] = useState('')
  const [notes, setNotes] = useState('')

  useEffect(() => {
    if (!data) return
    setTitle(data.title)
    setType(data.type)
    setEntityType(data.entityType ?? '')
    setProjectId(data.projectId ?? '')
    setIssueDate(parseDisplayDate(data.issueDate))
    setExpiryDate(data.expiryDate ? parseDisplayDate(data.expiryDate) : '')
    setStatus(data.status)
    setResponsiblePerson(data.responsiblePerson ?? '')
    setNotes(data.notes ?? '')
  }, [data])

  const mutation = useMutation({
    mutationFn: () =>
      updateComplianceRecord(id!, {
        title,
        type,
        entityType: entityType || undefined,
        projectId: projectId || undefined,
        issueDate,
        expiryDate: expiryDate || undefined,
        status,
        responsiblePerson: responsiblePerson || undefined,
        notes: notes || undefined,
      }),
    onSuccess: () => navigate(`/compliance/${id}`),
  })

  if (isLoading) return <p>Loading...</p>
  if (isError || !data) return <p>Compliance record not found.</p>

  return (
    <div className={styles.page}>
      <nav className={styles.breadcrumb}>Home &gt; Compliance &gt; Edit Record</nav>
      <h1>Edit Compliance Record</h1>

      <form className={styles.form} onSubmit={(e) => { e.preventDefault(); mutation.mutate() }}>
        <label>
          Title *
          <input value={title} onChange={(e) => setTitle(e.target.value)} required />
        </label>

        <div className={styles.row}>
          <label>
            Type *
            <select value={type} onChange={(e) => setType(e.target.value)} required>
              {COMPLIANCE_TYPES.map((t) => (
                <option key={t} value={t}>{t}</option>
              ))}
            </select>
          </label>
          <label>
            Status
            <select value={status} onChange={(e) => setStatus(e.target.value)}>
              {COMPLIANCE_STATUSES.map((s) => (
                <option key={s} value={s}>{s}</option>
              ))}
            </select>
          </label>
        </div>

        <div className={styles.row}>
          <label>
            Entity Type
            <input value={entityType} onChange={(e) => setEntityType(e.target.value)} />
          </label>
          <label>
            Project
            <select value={projectId} onChange={(e) => setProjectId(e.target.value)}>
              <option value="">None / Company-wide</option>
              {projects?.map((p) => (
                <option key={p.id} value={p.id}>{p.name}</option>
              ))}
            </select>
          </label>
        </div>

        <div className={styles.row}>
          <label>
            Issue Date *
            <input type="date" value={issueDate} onChange={(e) => setIssueDate(e.target.value)} required />
          </label>
          <label>
            Expiry Date
            <input type="date" value={expiryDate} onChange={(e) => setExpiryDate(e.target.value)} />
          </label>
        </div>

        <label>
          Responsible Person
          <input value={responsiblePerson} onChange={(e) => setResponsiblePerson(e.target.value)} />
        </label>

        <label>
          Notes
          <textarea rows={3} value={notes} onChange={(e) => setNotes(e.target.value)} />
        </label>

        <div className={styles.actions}>
          <button type="button" className={styles.draft} onClick={() => navigate(`/compliance/${id}`)}>Cancel</button>
          <button type="submit" className={styles.submit} disabled={mutation.isPending}>
            {mutation.isPending ? 'Saving...' : 'Save Changes'}
          </button>
        </div>
      </form>
    </div>
  )
}

/** Convert API display date (e.g. "05 Jul 2026") to yyyy-MM-dd for date input. */
function parseDisplayDate(display: string): string {
  const parsed = new Date(display)
  if (!Number.isNaN(parsed.getTime())) return parsed.toISOString().slice(0, 10)
  return new Date().toISOString().slice(0, 10)
}
