import { useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { useQuery, useMutation } from '@tanstack/react-query'
import { getProjects } from '@/api/projects'
import { createEquipment } from '@/api/equipment'
import styles from '../site-reports/AddSiteReportPage.module.css'

export function AddEquipmentPage() {
  const navigate = useNavigate()
  const { data: projects } = useQuery({ queryKey: ['projects'], queryFn: () => getProjects() })

  const [assetTag, setAssetTag] = useState('')
  const [name, setName] = useState('')
  const [category, setCategory] = useState('Excavator')
  const [status, setStatus] = useState('Available')
  const [makeModel, setMakeModel] = useState('')
  const [serialNumber, setSerialNumber] = useState('')
  const [operatorName, setOperatorName] = useState('')
  const [assignedProjectId, setAssignedProjectId] = useState('')
  const [nextInspectionDue, setNextInspectionDue] = useState('')
  const [nextServiceDue, setNextServiceDue] = useState('')
  const [notes, setNotes] = useState('')

  const mutation = useMutation({
    mutationFn: () =>
      createEquipment({
        assetTag,
        name,
        category,
        status,
        makeModel: makeModel || undefined,
        serialNumber: serialNumber || undefined,
        operatorName: operatorName || undefined,
        assignedProjectId: assignedProjectId || undefined,
        nextInspectionDue: nextInspectionDue || undefined,
        nextServiceDue: nextServiceDue || undefined,
        notes: notes || undefined,
      }),
    onSuccess: (eq) => navigate(`/equipment/${eq.id}`),
  })

  function handleSubmit(e: React.FormEvent) {
    e.preventDefault()
    if (!assetTag.trim() || !name.trim()) return
    mutation.mutate()
  }

  return (
    <div className={styles.page}>
      <nav className={styles.breadcrumb}>Home &gt; Equipment &gt; New Asset</nav>
      <h1>Register Equipment</h1>

      <form className={styles.form} onSubmit={handleSubmit}>
        <div className={styles.row}>
          <label>
            Asset Tag *
            <input value={assetTag} onChange={(e) => setAssetTag(e.target.value)} placeholder="EQ-006" required />
          </label>
          <label>
            Category
            <select value={category} onChange={(e) => setCategory(e.target.value)}>
              <option value="Excavator">Excavator</option>
              <option value="TLB">TLB</option>
              <option value="Grader">Grader</option>
              <option value="Roller">Roller</option>
              <option value="Generator">Generator</option>
              <option value="Compressor">Compressor</option>
              <option value="Scaffolding">Scaffolding</option>
              <option value="Power Tool">Power Tool</option>
              <option value="Safety Equipment">Safety Equipment</option>
              <option value="Other">Other</option>
            </select>
          </label>
          <label>
            Status
            <select value={status} onChange={(e) => setStatus(e.target.value)}>
              <option value="Available">Available</option>
              <option value="In Use">In Use</option>
              <option value="Maintenance">Maintenance</option>
              <option value="Decommissioned">Decommissioned</option>
            </select>
          </label>
        </div>

        <label>
          Name *
          <input value={name} onChange={(e) => setName(e.target.value)} required />
        </label>

        <div className={styles.row}>
          <label>Make / Model<input value={makeModel} onChange={(e) => setMakeModel(e.target.value)} /></label>
          <label>Serial Number<input value={serialNumber} onChange={(e) => setSerialNumber(e.target.value)} /></label>
          <label>Operator<input value={operatorName} onChange={(e) => setOperatorName(e.target.value)} /></label>
        </div>

        <div className={styles.row}>
          <label>
            Assigned Project
            <select value={assignedProjectId} onChange={(e) => setAssignedProjectId(e.target.value)}>
              <option value="">None</option>
              {projects?.map((p) => (
                <option key={p.id} value={p.id}>{p.name}</option>
              ))}
            </select>
          </label>
          <label>Next Inspection<input type="date" value={nextInspectionDue} onChange={(e) => setNextInspectionDue(e.target.value)} /></label>
          <label>Next Service<input type="date" value={nextServiceDue} onChange={(e) => setNextServiceDue(e.target.value)} /></label>
        </div>

        <label>
          Notes
          <textarea rows={2} value={notes} onChange={(e) => setNotes(e.target.value)} />
        </label>

        <div className={styles.actions}>
          <button type="button" className={styles.draft} onClick={() => navigate('/equipment')}>Cancel</button>
          <button type="submit" className={styles.submit} disabled={mutation.isPending}>
            {mutation.isPending ? 'Saving...' : 'Register Equipment'}
          </button>
        </div>
      </form>
    </div>
  )
}
