import { useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { useMutation } from '@tanstack/react-query'
import { createClient } from '@/api/clients'
import styles from '../site-reports/AddSiteReportPage.module.css'

export function AddClientPage() {
  const navigate = useNavigate()
  const [name, setName] = useState('')
  const [type, setType] = useState('Private')
  const [registrationNumber, setRegistrationNumber] = useState('')
  const [address, setAddress] = useState('')
  const [city, setCity] = useState('')
  const [province, setProvince] = useState('KZN')
  const [notes, setNotes] = useState('')
  const [contactName, setContactName] = useState('')
  const [contactPhone, setContactPhone] = useState('')
  const [contactEmail, setContactEmail] = useState('')

  const mutation = useMutation({
    mutationFn: () =>
      createClient({
        name,
        type,
        registrationNumber: registrationNumber || undefined,
        address: address || undefined,
        city: city || undefined,
        province: province || undefined,
        notes: notes || undefined,
        contacts: contactName
          ? [{ name: contactName, phone: contactPhone || undefined, email: contactEmail || undefined, isPrimary: true }]
          : undefined,
      }),
    onSuccess: (client) => navigate(`/clients/${client.id}`),
  })

  function handleSubmit(e: React.FormEvent) {
    e.preventDefault()
    if (!name.trim()) return
    mutation.mutate()
  }

  return (
    <div className={styles.page}>
      <nav className={styles.breadcrumb}>Home &gt; Clients &gt; New</nav>
      <h1>New Client</h1>

      <form className={styles.form} onSubmit={handleSubmit}>
        <div className={styles.row}>
          <label>
            Client Name *
            <input value={name} onChange={(e) => setName(e.target.value)} required />
          </label>
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
          <label>
            Registration Number
            <input value={registrationNumber} onChange={(e) => setRegistrationNumber(e.target.value)} />
          </label>
          <label>
            Province
            <input value={province} onChange={(e) => setProvince(e.target.value)} />
          </label>
          <label>
            City
            <input value={city} onChange={(e) => setCity(e.target.value)} />
          </label>
        </div>

        <label>
          Address
          <input value={address} onChange={(e) => setAddress(e.target.value)} />
        </label>

        <fieldset className={styles.fieldset}>
          <legend>Primary Contact (optional)</legend>
          <div className={styles.row}>
            <label>Name<input value={contactName} onChange={(e) => setContactName(e.target.value)} /></label>
            <label>Phone<input value={contactPhone} onChange={(e) => setContactPhone(e.target.value)} /></label>
            <label>Email<input type="email" value={contactEmail} onChange={(e) => setContactEmail(e.target.value)} /></label>
          </div>
        </fieldset>

        <label>
          Notes
          <textarea rows={2} value={notes} onChange={(e) => setNotes(e.target.value)} />
        </label>

        <div className={styles.actions}>
          <button type="button" className={styles.draft} onClick={() => navigate('/clients')}>Cancel</button>
          <button type="submit" className={styles.submit} disabled={mutation.isPending}>
            {mutation.isPending ? 'Saving...' : 'Create Client'}
          </button>
        </div>
      </form>
    </div>
  )
}
