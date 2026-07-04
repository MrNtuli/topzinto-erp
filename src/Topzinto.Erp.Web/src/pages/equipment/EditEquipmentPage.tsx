import { useEffect, useState } from 'react'
import { useNavigate, useParams } from 'react-router-dom'
import { useQuery, useMutation } from '@tanstack/react-query'
import { getProjects } from '@/api/projects'
import { getEquipment, updateEquipment } from '@/api/equipment'
import styles from '../site-reports/AddSiteReportPage.module.css'

export function EditEquipmentPage() {
  const { id } = useParams<{ id: string }>()
  const navigate = useNavigate()
  const { data, isLoading } = useQuery({ queryKey: ['equipment', id], queryFn: () => getEquipment(id!), enabled: !!id })
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

  useEffect(() => {
    if (!data) return
    setAssetTag(data.assetTag)
    setName(data.name)
    setCategory(data.category)
    setStatus(data.status)
    setMakeModel(data.makeModel ?? '')
    setSerialNumber(data.serialNumber ?? '')
    setOperatorName(data.operatorName ?? '')
    setAssignedProjectId(data.assignedProjectId ?? '')
    setNextInspectionDue(data.nextInspectionDueInput ?? '')
    setNextServiceDue(data.nextServiceDueInput ?? '')
    setNotes(data.notes ?? '')
  }, [data])

  const mutation = useMutation({
    mutationFn: () =>
      updateEquipment(id!, {
        assetTag, name, category, status,
        makeModel: makeModel || undefined,
        serialNumber: serialNumber || undefined,
        operatorName: operatorName || undefined,
        assignedProjectId: assignedProjectId || undefined,
        nextInspectionDue: nextInspectionDue || undefined,
        nextServiceDue: nextServiceDue || undefined,
        notes: notes || undefined,
      }),
    onSuccess: () => navigate(`/equipment/${id}`),
  })

  if (isLoading) return <p>Loading...</p>
  if (!data) return <p>Equipment not found.</p>

  return (
    <div className={styles.page}>
      <nav className={styles.breadcrumb}>Home &gt; Equipment &gt; Edit</nav>
      <h1>Edit Equipment</h1>
      <form className={styles.form} onSubmit={(e) => { e.preventDefault(); mutation.mutate() }}>
        <div className={styles.row}>
          <label>Asset Tag *<input value={assetTag} onChange={(e) => setAssetTag(e.target.value)} required /></label>
          <label>
            Category
            <select value={category} onChange={(e) => setCategory(e.target.value)}>
              <option value="Excavator">Excavator</option><option value="TLB">TLB</option><option value="Grader">Grader</option>
              <option value="Roller">Roller</option><option value="Generator">Generator</option><option value="Other">Other</option>
            </select>
          </label>
          <label>
            Status
            <select value={status} onChange={(e) => setStatus(e.target.value)}>
              <option value="Available">Available</option><option value="In Use">In Use</option>
              <option value="Maintenance">Maintenance</option><option value="Decommissioned">Decommissioned</option>
            </select>
          </label>
        </div>
        <label>Name *<input value={name} onChange={(e) => setName(e.target.value)} required /></label>
        <div className={styles.row}>
          <label>Make / Model<input value={makeModel} onChange={(e) => setMakeModel(e.target.value)} /></label>
          <label>Serial<input value={serialNumber} onChange={(e) => setSerialNumber(e.target.value)} /></label>
          <label>Operator<input value={operatorName} onChange={(e) => setOperatorName(e.target.value)} /></label>
        </div>
        <div className={styles.row}>
          <label>
            Project
            <select value={assignedProjectId} onChange={(e) => setAssignedProjectId(e.target.value)}>
              <option value="">None</option>
              {projects?.map((p) => <option key={p.id} value={p.id}>{p.name}</option>)}
            </select>
          </label>
          <label>Next Inspection<input type="date" value={nextInspectionDue} onChange={(e) => setNextInspectionDue(e.target.value)} /></label>
          <label>Next Service<input type="date" value={nextServiceDue} onChange={(e) => setNextServiceDue(e.target.value)} /></label>
        </div>
        <label>Notes<textarea rows={2} value={notes} onChange={(e) => setNotes(e.target.value)} /></label>
        <div className={styles.actions}>
          <button type="button" className={styles.draft} onClick={() => navigate(`/equipment/${id}`)}>Cancel</button>
          <button type="submit" className={styles.submit} disabled={mutation.isPending}>Save Changes</button>
        </div>
      </form>
    </div>
  )
}
