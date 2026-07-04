import { useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { useMutation } from '@tanstack/react-query'
import { createSupplier } from '@/api/suppliers'
import styles from '../site-reports/AddSiteReportPage.module.css'

export function AddSupplierPage() {
  const navigate = useNavigate()
  const [code, setCode] = useState('')
  const [name, setName] = useState('')
  const [category, setCategory] = useState('Materials')
  const [status, setStatus] = useState('Active')
  const [contactPerson, setContactPerson] = useState('')
  const [phone, setPhone] = useState('')
  const [email, setEmail] = useState('')
  const [city, setCity] = useState('')
  const [province, setProvince] = useState('KZN')
  const [vatNumber, setVatNumber] = useState('')
  const [notes, setNotes] = useState('')

  const mutation = useMutation({
    mutationFn: () =>
      createSupplier({
        code,
        name,
        category,
        status,
        contactPerson: contactPerson || undefined,
        phone: phone || undefined,
        email: email || undefined,
        city: city || undefined,
        province: province || undefined,
        vatNumber: vatNumber || undefined,
        notes: notes || undefined,
      }),
    onSuccess: (supplier) => navigate(`/suppliers/${supplier.id}`),
  })

  function handleSubmit(e: React.FormEvent) {
    e.preventDefault()
    if (!code.trim() || !name.trim()) return
    mutation.mutate()
  }

  return (
    <div className={styles.page}>
      <nav className={styles.breadcrumb}>Home &gt; Suppliers &gt; New</nav>
      <h1>New Supplier</h1>

      <form className={styles.form} onSubmit={handleSubmit}>
        <div className={styles.row}>
          <label>
            Supplier Code *
            <input value={code} onChange={(e) => setCode(e.target.value)} placeholder="SUP-006" required />
          </label>
          <label>
            Category
            <select value={category} onChange={(e) => setCategory(e.target.value)}>
              <option value="Materials">Materials</option>
              <option value="Plant Hire">Plant Hire</option>
              <option value="Services">Services</option>
              <option value="Subcontractor">Subcontractor</option>
              <option value="Other">Other</option>
            </select>
          </label>
          <label>
            Status
            <select value={status} onChange={(e) => setStatus(e.target.value)}>
              <option value="Active">Active</option>
              <option value="Inactive">Inactive</option>
              <option value="Blacklisted">Blacklisted</option>
            </select>
          </label>
        </div>

        <label>
          Supplier Name *
          <input value={name} onChange={(e) => setName(e.target.value)} required />
        </label>

        <div className={styles.row}>
          <label>Contact Person<input value={contactPerson} onChange={(e) => setContactPerson(e.target.value)} /></label>
          <label>Phone<input value={phone} onChange={(e) => setPhone(e.target.value)} /></label>
          <label>Email<input type="email" value={email} onChange={(e) => setEmail(e.target.value)} /></label>
        </div>

        <div className={styles.row}>
          <label>City<input value={city} onChange={(e) => setCity(e.target.value)} /></label>
          <label>Province<input value={province} onChange={(e) => setProvince(e.target.value)} /></label>
          <label>VAT Number<input value={vatNumber} onChange={(e) => setVatNumber(e.target.value)} /></label>
        </div>

        <label>
          Notes
          <textarea rows={2} value={notes} onChange={(e) => setNotes(e.target.value)} />
        </label>

        <div className={styles.actions}>
          <button type="button" className={styles.draft} onClick={() => navigate('/suppliers')}>Cancel</button>
          <button type="submit" className={styles.submit} disabled={mutation.isPending}>
            {mutation.isPending ? 'Saving...' : 'Create Supplier'}
          </button>
        </div>
      </form>
    </div>
  )
}
