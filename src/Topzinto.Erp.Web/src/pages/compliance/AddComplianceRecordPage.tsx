import { useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { useQuery, useMutation } from '@tanstack/react-query'
import { getProjects } from '@/api/projects'
import { createComplianceRecord, COMPLIANCE_TYPES, COMPLIANCE_STATUSES } from '@/api/compliance'
import styles from '../site-reports/AddSiteReportPage.module.css'

export function AddComplianceRecordPage() {
  const navigate = useNavigate()
  const { data: projects } = useQuery({ queryKey: ['projects'], queryFn: () => getProjects() })

  const [title, setTitle] = useState('')
  const [type, setType] = useState('Certificate')
  const [entityType, setEntityType] = useState('')
  const [projectId, setProjectId] = useState('')
  const [issueDate, setIssueDate] = useState(new Date().toISOString().slice(0, 10))
  const [expiryDate, setExpiryDate] = useState('')
  const [status, setStatus] = useState('Valid')
  const [responsiblePerson, setResponsiblePerson] = useState('')
  const [notes, setNotes] = useState('')

  const mutation = useMutation({
    mutationFn: () =>
      createComplianceRecord({
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
    onSuccess: (record) => navigate(`/compliance/${record.id}`),
  })

  function handleSubmit(e: React.FormEvent) {
    e.preventDefault()
    if (!title.trim()) return
    mutation.mutate()
  }

  return (
    <div className={styles.page}>
      <nav className={styles.breadcrumb}>Home &gt; Compliance &gt; Add Record</nav>
      <h1>Add Compliance Record</h1>

      <form className={styles.form} onSubmit={handleSubmit}>
        <label>
          Title *
          <input value={title} onChange={(e) => setTitle(e.target.value)} placeholder="e.g. CIPC Annual Return, Vehicle Licence" required />
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
            <input value={entityType} onChange={(e) => setEntityType(e.target.value)} placeholder="Company, Vehicle, Employee..." />
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
          <input value={responsiblePerson} onChange={(e) => setResponsiblePerson(e.target.value)} placeholder="Person accountable for renewal" />
        </label>

        <label>
          Notes
          <textarea rows={3} value={notes} onChange={(e) => setNotes(e.target.value)} placeholder="Additional details, renewal reminders..." />
        </label>

        <div className={styles.actions}>
          <button type="button" className={styles.draft} onClick={() => navigate('/compliance')}>Cancel</button>
          <button type="submit" className={styles.submit} disabled={mutation.isPending}>
            {mutation.isPending ? 'Saving...' : 'Add Record'}
          </button>
        </div>
      </form>
    </div>
  )
}
