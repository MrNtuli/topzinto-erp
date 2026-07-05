import { useState, useEffect } from 'react'
import { useNavigate, useParams, Link } from 'react-router-dom'
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { getProjects } from '@/api/projects'
import { getVehicle, createFuelLog } from '@/api/fleet'
import styles from '../site-reports/AddSiteReportPage.module.css'

export function AddFuelLogPage() {
  const { id } = useParams<{ id: string }>()
  const navigate = useNavigate()
  const queryClient = useQueryClient()

  const { data: vehicle, isLoading, isError } = useQuery({
    queryKey: ['vehicle', id],
    queryFn: () => getVehicle(id!),
    enabled: !!id,
    retry: false,
  })

  const { data: projects } = useQuery({ queryKey: ['projects'], queryFn: () => getProjects() })

  const [logDate, setLogDate] = useState(new Date().toISOString().slice(0, 10))
  const [litres, setLitres] = useState('')
  const [cost, setCost] = useState('')
  const [odometerReading, setOdometerReading] = useState('')
  const [projectId, setProjectId] = useState('')
  const [notes, setNotes] = useState('')
  const [validationError, setValidationError] = useState('')

  useEffect(() => {
    if (vehicle?.assignedProjectId && !projectId) {
      setProjectId(vehicle.assignedProjectId)
    }
  }, [vehicle?.assignedProjectId, projectId])

  const mutation = useMutation({
    mutationFn: () =>
      createFuelLog(id!, {
        logDate,
        litres: parseFloat(litres),
        cost: parseFloat(cost),
        odometerReading: odometerReading ? parseFloat(odometerReading) : undefined,
        projectId: projectId || undefined,
        notes: notes.trim() || undefined,
      }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['vehicle', id] })
      navigate(`/fleet/${id}`)
    },
  })

  if (isLoading) return <p>Loading...</p>
  if (isError || !vehicle) return <p>Vehicle not found.</p>

  function handleSubmit(e: React.FormEvent) {
    e.preventDefault()
    setValidationError('')

    const litresNum = parseFloat(litres)
    const costNum = parseFloat(cost)

    if (!logDate) {
      setValidationError('Date is required.')
      return
    }
    if (!litres || litresNum <= 0) {
      setValidationError('Litres must be greater than zero.')
      return
    }
    if (!cost || costNum < 0) {
      setValidationError('Cost must be zero or greater.')
      return
    }
    if (odometerReading && parseFloat(odometerReading) < 0) {
      setValidationError('Odometer reading cannot be negative.')
      return
    }

    mutation.mutate()
  }

  return (
    <div className={styles.page}>
      <nav className={styles.breadcrumb}>
        <Link to="/fleet">Fleet</Link> &gt;{' '}
        <Link to={`/fleet/${id}`}>{vehicle.registrationNumber}</Link> &gt; Add Fuel Log
      </nav>
      <h1>Record Fuel — {vehicle.makeModel}</h1>

      <form className={styles.form} onSubmit={handleSubmit}>
        <div className={styles.row}>
          <label>
            Date *
            <input type="date" value={logDate} onChange={(e) => setLogDate(e.target.value)} required />
          </label>
          <label>
            Litres *
            <input
              type="number"
              min="0.01"
              step="0.01"
              value={litres}
              onChange={(e) => setLitres(e.target.value)}
              placeholder="45.5"
              required
            />
          </label>
          <label>
            Cost (R) *
            <input
              type="number"
              min="0"
              step="0.01"
              value={cost}
              onChange={(e) => setCost(e.target.value)}
              placeholder="950.00"
              required
            />
          </label>
        </div>

        <div className={styles.row}>
          <label>
            Odometer (km)
            <input
              type="number"
              min="0"
              step="1"
              value={odometerReading}
              onChange={(e) => setOdometerReading(e.target.value)}
              placeholder="125000"
            />
          </label>
          <label>
            Project (optional)
            <select
              value={projectId}
              onChange={(e) => setProjectId(e.target.value)}
            >
              <option value="">None</option>
              {projects?.map((p) => (
                <option key={p.id} value={p.id}>{p.name}</option>
              ))}
            </select>
          </label>
        </div>

        <label>
          Notes
          <textarea
            rows={2}
            value={notes}
            onChange={(e) => setNotes(e.target.value)}
            placeholder="Fuel station, receipt ref, driver, etc."
          />
        </label>

        {validationError && <p role="alert">{validationError}</p>}
        {mutation.isError && <p role="alert">Failed to save fuel log. Please try again.</p>}

        <div className={styles.actions}>
          <button type="button" className={styles.draft} onClick={() => navigate(`/fleet/${id}`)}>
            Cancel
          </button>
          <button type="submit" className={styles.submit} disabled={mutation.isPending}>
            {mutation.isPending ? 'Saving...' : 'Save Fuel Log'}
          </button>
        </div>
      </form>
    </div>
  )
}
