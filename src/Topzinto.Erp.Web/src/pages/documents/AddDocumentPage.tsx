import { useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { useQuery, useMutation } from '@tanstack/react-query'
import { getProjects } from '@/api/projects'
import { createDocument, uploadDocumentFile } from '@/api/documents'
import styles from '../site-reports/AddSiteReportPage.module.css'

export function AddDocumentPage() {
  const navigate = useNavigate()
  const { data: projects } = useQuery({ queryKey: ['projects'], queryFn: () => getProjects() })

  const [title, setTitle] = useState('')
  const [category, setCategory] = useState('General')
  const [parentType, setParentType] = useState('Project')
  const [projectId, setProjectId] = useState('')
  const [fileName, setFileName] = useState('')
  const [issueDate, setIssueDate] = useState('')
  const [expiryDate, setExpiryDate] = useState('')
  const [status, setStatus] = useState('Approved')
  const [notes, setNotes] = useState('')
  const [file, setFile] = useState<File | null>(null)

  const selectedProject = projects?.find((p) => p.id === projectId)

  const mutation = useMutation({
    mutationFn: async () => {
      const doc = await createDocument({
        title,
        category,
        parentType,
        parentId: parentType === 'Project' ? projectId || undefined : undefined,
        parentName: parentType === 'Project' ? selectedProject?.name : parentType === 'Company' ? 'TopZinto CC' : undefined,
        fileName: (file?.name ?? fileName) || undefined,
        issueDate: issueDate || undefined,
        expiryDate: expiryDate || undefined,
        status,
        notes: notes || undefined,
      })
      if (file) await uploadDocumentFile(doc.id, file)
      return doc
    },
    onSuccess: () => navigate('/documents'),
  })

  function handleSubmit(e: React.FormEvent) {
    e.preventDefault()
    if (!title) return
    mutation.mutate()
  }

  return (
    <div className={styles.page}>
      <nav className={styles.breadcrumb}>Home &gt; Documents &gt; Register Document</nav>
      <h1>Register Document</h1>
      <p className={styles.hint}>Attach a file (optional, max 50MB) — stored locally in dev; MinIO for production.</p>

      <form className={styles.form} onSubmit={handleSubmit}>
        <label>
          Title *
          <input value={title} onChange={(e) => setTitle(e.target.value)} required />
        </label>
        <label>
          Category
          <select value={category} onChange={(e) => setCategory(e.target.value)}>
            <option>General</option>
            <option>Contract</option>
            <option>Compliance</option>
            <option>Insurance</option>
            <option>H&S</option>
            <option>Drawings</option>
            <option>Tender</option>
          </select>
        </label>
        <label>
          Linked To
          <select value={parentType} onChange={(e) => setParentType(e.target.value)}>
            <option value="Project">Project</option>
            <option value="Company">Company</option>
            <option value="Contract">Contract</option>
            <option value="Tender">Tender</option>
          </select>
        </label>
        {parentType === 'Project' && (
          <label>
            Project
            <select value={projectId} onChange={(e) => setProjectId(e.target.value)}>
              <option value="">Select project...</option>
              {projects?.map((p) => (
                <option key={p.id} value={p.id}>{p.name}</option>
              ))}
            </select>
          </label>
        )}
        <label>
          Attach file
          <input type="file" onChange={(e) => setFile(e.target.files?.[0] ?? null)} />
        </label>
        {!file && (
          <label>
            File name (reference only)
            <input value={fileName} onChange={(e) => setFileName(e.target.value)} placeholder="e.g. contract-signed.pdf" />
          </label>
        )}
        <div className={styles.row}>
          <label>
            Issue date
            <input type="date" value={issueDate} onChange={(e) => setIssueDate(e.target.value)} />
          </label>
          <label>
            Expiry date
            <input type="date" value={expiryDate} onChange={(e) => setExpiryDate(e.target.value)} />
          </label>
        </div>
        <label>
          Status
          <select value={status} onChange={(e) => setStatus(e.target.value)}>
            <option>Draft</option>
            <option>Pending Approval</option>
            <option>Approved</option>
          </select>
        </label>
        <label>
          Notes
          <textarea value={notes} onChange={(e) => setNotes(e.target.value)} rows={3} />
        </label>
        <div className={styles.actions}>
          <button type="button" onClick={() => navigate('/documents')}>Cancel</button>
          <button type="submit" className={styles.primary} disabled={mutation.isPending}>
            {mutation.isPending ? 'Saving...' : 'Register Document'}
          </button>
        </div>
      </form>
    </div>
  )
}
