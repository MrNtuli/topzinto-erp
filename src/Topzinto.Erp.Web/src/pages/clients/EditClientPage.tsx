import { useEffect, useState } from 'react'
import { useNavigate, useParams } from 'react-router-dom'
import { useQuery, useMutation } from '@tanstack/react-query'
import { getClient, updateClient } from '@/api/clients'
import styles from '../site-reports/AddSiteReportPage.module.css'

export function EditClientPage() {
  const { id } = useParams<{ id: string }>()
  const navigate = useNavigate()
  const { data, isLoading } = useQuery({
    queryKey: ['client', id],
    queryFn: () => getClient(id!),
    enabled: !!id,
  })

  const [name, setName] = useState('')
  const [type, setType] = useState('Private')
  const [registrationNumber, setRegistrationNumber] = useState('')
  const [address, setAddress] = useState('')
  const [city, setCity] = useState('')
  const [province, setProvince] = useState('')
  const [notes, setNotes] = useState('')

  useEffect(() => {
    if (!data) return
    setName(data.name)
    setType(data.type)
    setRegistrationNumber(data.registrationNumber ?? '')
    setAddress(data.address ?? '')
    setCity(data.city ?? '')
    setProvince(data.province ?? '')
    setNotes(data.notes ?? '')
  }, [data])

  const mutation = useMutation({
    mutationFn: () =>
      updateClient(id!, {
        name,
        type,
        registrationNumber: registrationNumber || undefined,
        address: address || undefined,
        city: city || undefined,
        province: province || undefined,
        notes: notes || undefined,
      }),
    onSuccess: () => navigate(`/clients/${id}`),
  })

  if (isLoading) return <p>Loading...</p>
  if (!data) return <p>Client not found.</p>

  return (
    <div className={styles.page}>
      <nav className={styles.breadcrumb}>Home &gt; Clients &gt; Edit</nav>
      <h1>Edit Client</h1>
      <form className={styles.form} onSubmit={(e) => { e.preventDefault(); mutation.mutate() }}>
        <div className={styles.row}>
          <label>Client Name *<input value={name} onChange={(e) => setName(e.target.value)} required /></label>
          <label>
            Type *
            <select value={type} onChange={(e) => setType(e.target.value)}>
              <option value="Private">Private</option>
              <option value="Government">Government</option>
              <option value="Municipal">Municipal</option>
              <option value="SOE">SOE</option>
            </select>
          </label>
        </div>
        <div className={styles.row}>
          <label>Registration<input value={registrationNumber} onChange={(e) => setRegistrationNumber(e.target.value)} /></label>
          <label>Province<input value={province} onChange={(e) => setProvince(e.target.value)} /></label>
          <label>City<input value={city} onChange={(e) => setCity(e.target.value)} /></label>
        </div>
        <label>Address<input value={address} onChange={(e) => setAddress(e.target.value)} /></label>
        <label>Notes<textarea rows={2} value={notes} onChange={(e) => setNotes(e.target.value)} /></label>
        <div className={styles.actions}>
          <button type="button" className={styles.draft} onClick={() => navigate(`/clients/${id}`)}>Cancel</button>
          <button type="submit" className={styles.submit} disabled={mutation.isPending}>Save Changes</button>
        </div>
      </form>
    </div>
  )
}
