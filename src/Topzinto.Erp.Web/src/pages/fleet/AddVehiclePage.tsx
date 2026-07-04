import { useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { useQuery, useMutation } from '@tanstack/react-query'
import { getProjects } from '@/api/projects'
import { createVehicle } from '@/api/fleet'
import styles from '../site-reports/AddSiteReportPage.module.css'

export function AddVehiclePage() {
  const navigate = useNavigate()
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
  const [notes, setNotes] = useState('')

  const mutation = useMutation({
    mutationFn: () =>
      createVehicle({
        registrationNumber,
        makeModel,
        type,
        status,
        driverName: driverName || undefined,
        currentLocation: currentLocation || undefined,
        assignedProjectId: assignedProjectId || undefined,
        licenseExpiryDate: licenseExpiryDate || undefined,
        insuranceExpiryDate: insuranceExpiryDate || undefined,
        notes: notes || undefined,
      }),
    onSuccess: (vehicle) => navigate(`/fleet/${vehicle.id}`),
  })

  function handleSubmit(e: React.FormEvent) {
    e.preventDefault()
    if (!registrationNumber.trim() || !makeModel.trim()) return
    mutation.mutate()
  }

  return (
    <div className={styles.page}>
      <nav className={styles.breadcrumb}>Home &gt; Fleet &gt; New Vehicle</nav>
      <h1>Register Vehicle</h1>

      <form className={styles.form} onSubmit={handleSubmit}>
        <div className={styles.row}>
          <label>
            Registration *
            <input value={registrationNumber} onChange={(e) => setRegistrationNumber(e.target.value)} placeholder="ND 000 GP" required />
          </label>
          <label>
            Type
            <select value={type} onChange={(e) => setType(e.target.value)}>
              <option value="LDV">LDV</option>
              <option value="Bakkie">Bakkie</option>
              <option value="Truck">Truck</option>
              <option value="Tipper Truck">Tipper Truck</option>
              <option value="TLB">TLB</option>
              <option value="Water Tanker">Water Tanker</option>
              <option value="Heavy Vehicle">Heavy Vehicle</option>
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
          Make / Model *
          <input value={makeModel} onChange={(e) => setMakeModel(e.target.value)} placeholder="Toyota Hilux 2.8 GD-6" required />
        </label>

        <div className={styles.row}>
          <label>Driver<input value={driverName} onChange={(e) => setDriverName(e.target.value)} /></label>
          <label>Location<input value={currentLocation} onChange={(e) => setCurrentLocation(e.target.value)} /></label>
          <label>
            Assigned Project
            <select value={assignedProjectId} onChange={(e) => setAssignedProjectId(e.target.value)}>
              <option value="">None</option>
              {projects?.map((p) => (
                <option key={p.id} value={p.id}>{p.name}</option>
              ))}
            </select>
          </label>
        </div>

        <div className={styles.row}>
          <label>License Expiry<input type="date" value={licenseExpiryDate} onChange={(e) => setLicenseExpiryDate(e.target.value)} /></label>
          <label>Insurance Expiry<input type="date" value={insuranceExpiryDate} onChange={(e) => setInsuranceExpiryDate(e.target.value)} /></label>
        </div>

        <label>
          Notes
          <textarea rows={2} value={notes} onChange={(e) => setNotes(e.target.value)} />
        </label>

        <div className={styles.actions}>
          <button type="button" className={styles.draft} onClick={() => navigate('/fleet')}>Cancel</button>
          <button type="submit" className={styles.submit} disabled={mutation.isPending}>
            {mutation.isPending ? 'Saving...' : 'Register Vehicle'}
          </button>
        </div>
      </form>
    </div>
  )
}
