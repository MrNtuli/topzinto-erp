import { useEffect, useState } from 'react'
import { useNavigate, useParams } from 'react-router-dom'
import { useQuery, useMutation } from '@tanstack/react-query'
import { getProjects } from '@/api/projects'
import { getVehicle, updateVehicle } from '@/api/fleet'
import styles from '../site-reports/AddSiteReportPage.module.css'

export function EditVehiclePage() {
  const { id } = useParams<{ id: string }>()
  const navigate = useNavigate()
  const { data, isLoading } = useQuery({ queryKey: ['vehicle', id], queryFn: () => getVehicle(id!), enabled: !!id })
  const { data: projects } = useQuery({ queryKey: ['projects'], queryFn: () => getProjects() })

  const [registrationNumber, setRegistrationNumber] = useState('')
  const [makeModel, setMakeModel] = useState('')
  const [type, setType] = useState('LDV')
  const [status, setStatus] = useState('Available')
  const [driverName, setDriverName] = useState('')
  const [currentLocation, setCurrentLocation] = useState('')
  const [assignedProjectId, setAssignedProjectId] = useState('')
  const [licenseExpiryDate, setLicenseExpiryDate] = useState('')
  const [insuranceExpiryDate, setInsuranceExpiryDate] = useState('')
  const [roadworthyExpiryDate, setRoadworthyExpiryDate] = useState('')
  const [notes, setNotes] = useState('')

  useEffect(() => {
    if (!data) return
    setRegistrationNumber(data.registrationNumber)
    setMakeModel(data.makeModel)
    setType(data.type)
    setStatus(data.status)
    setDriverName(data.driverName ?? '')
    setCurrentLocation(data.currentLocation ?? '')
    setAssignedProjectId(data.assignedProjectId ?? '')
    setLicenseExpiryDate(data.licenseExpiryInput ?? '')
    setInsuranceExpiryDate(data.insuranceExpiryInput ?? '')
    setRoadworthyExpiryDate(data.roadworthyExpiryInput ?? '')
    setNotes(data.notes ?? '')
  }, [data])

  const mutation = useMutation({
    mutationFn: () =>
      updateVehicle(id!, {
        registrationNumber, makeModel, type, status,
        driverName: driverName || undefined,
        currentLocation: currentLocation || undefined,
        assignedProjectId: assignedProjectId || undefined,
        licenseExpiryDate: licenseExpiryDate || undefined,
        insuranceExpiryDate: insuranceExpiryDate || undefined,
        roadworthyExpiryDate: roadworthyExpiryDate || undefined,
        notes: notes || undefined,
      }),
    onSuccess: () => navigate(`/fleet/${id}`),
  })

  if (isLoading) return <p>Loading...</p>
  if (!data) return <p>Vehicle not found.</p>

  return (
    <div className={styles.page}>
      <nav className={styles.breadcrumb}>Home &gt; Fleet &gt; Edit</nav>
      <h1>Edit Vehicle</h1>
      <form className={styles.form} onSubmit={(e) => { e.preventDefault(); mutation.mutate() }}>
        <div className={styles.row}>
          <label>Registration *<input value={registrationNumber} onChange={(e) => setRegistrationNumber(e.target.value)} required /></label>
          <label>
            Type
            <select value={type} onChange={(e) => setType(e.target.value)}>
              <option value="LDV">LDV</option><option value="Bakkie">Bakkie</option><option value="Truck">Truck</option>
              <option value="Tipper Truck">Tipper Truck</option><option value="TLB">TLB</option>
              <option value="Water Tanker">Water Tanker</option><option value="Heavy Vehicle">Heavy Vehicle</option>
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
        <label>Make / Model *<input value={makeModel} onChange={(e) => setMakeModel(e.target.value)} required /></label>
        <div className={styles.row}>
          <label>Driver<input value={driverName} onChange={(e) => setDriverName(e.target.value)} /></label>
          <label>Location<input value={currentLocation} onChange={(e) => setCurrentLocation(e.target.value)} /></label>
          <label>
            Project
            <select value={assignedProjectId} onChange={(e) => setAssignedProjectId(e.target.value)}>
              <option value="">None</option>
              {projects?.map((p) => <option key={p.id} value={p.id}>{p.name}</option>)}
            </select>
          </label>
        </div>
        <div className={styles.row}>
          <label>License Expiry<input type="date" value={licenseExpiryDate} onChange={(e) => setLicenseExpiryDate(e.target.value)} /></label>
          <label>Insurance Expiry<input type="date" value={insuranceExpiryDate} onChange={(e) => setInsuranceExpiryDate(e.target.value)} /></label>
          <label>Roadworthy Expiry<input type="date" value={roadworthyExpiryDate} onChange={(e) => setRoadworthyExpiryDate(e.target.value)} /></label>
        </div>
        <label>Notes<textarea rows={2} value={notes} onChange={(e) => setNotes(e.target.value)} /></label>
        <div className={styles.actions}>
          <button type="button" className={styles.draft} onClick={() => navigate(`/fleet/${id}`)}>Cancel</button>
          <button type="submit" className={styles.submit} disabled={mutation.isPending}>Save Changes</button>
        </div>
      </form>
    </div>
  )
}
