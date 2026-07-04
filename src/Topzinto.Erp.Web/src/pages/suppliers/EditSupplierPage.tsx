import { useEffect, useState } from 'react'
import { useNavigate, useParams } from 'react-router-dom'
import { useQuery, useMutation } from '@tanstack/react-query'
import { getSupplier, updateSupplier } from '@/api/suppliers'
import styles from '../site-reports/AddSiteReportPage.module.css'

export function EditSupplierPage() {
  const { id } = useParams<{ id: string }>()
  const navigate = useNavigate()
  const { data, isLoading } = useQuery({ queryKey: ['supplier', id], queryFn: () => getSupplier(id!), enabled: !!id })

  const [code, setCode] = useState('')
  const [name, setName] = useState('')
  const [category, setCategory] = useState('Materials')
  const [status, setStatus] = useState('Active')
  const [contactPerson, setContactPerson] = useState('')
  const [phone, setPhone] = useState('')
  const [email, setEmail] = useState('')
  const [address, setAddress] = useState('')
  const [city, setCity] = useState('')
  const [province, setProvince] = useState('')
  const [vatNumber, setVatNumber] = useState('')
  const [notes, setNotes] = useState('')

  useEffect(() => {
    if (!data) return
    setCode(data.code)
    setName(data.name)
    setCategory(data.category)
    setStatus(data.status)
    setContactPerson(data.contactPerson ?? '')
    setPhone(data.phone ?? '')
    setEmail(data.email ?? '')
    setAddress(data.address ?? '')
    setCity(data.city ?? '')
    setProvince(data.province ?? '')
    setVatNumber(data.vatNumber ?? '')
    setNotes(data.notes ?? '')
  }, [data])

  const mutation = useMutation({
    mutationFn: () =>
      updateSupplier(id!, {
        code, name, category, status,
        contactPerson: contactPerson || undefined,
        phone: phone || undefined,
        email: email || undefined,
        address: address || undefined,
        city: city || undefined,
        province: province || undefined,
        vatNumber: vatNumber || undefined,
        notes: notes || undefined,
      }),
    onSuccess: () => navigate(`/suppliers/${id}`),
  })

  if (isLoading) return <p>Loading...</p>
  if (!data) return <p>Supplier not found.</p>

  return (
    <div className={styles.page}>
      <nav className={styles.breadcrumb}>Home &gt; Suppliers &gt; Edit</nav>
      <h1>Edit Supplier</h1>
      <form className={styles.form} onSubmit={(e) => { e.preventDefault(); mutation.mutate() }}>
        <div className={styles.row}>
          <label>Code *<input value={code} onChange={(e) => setCode(e.target.value)} required /></label>
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
        <label>Name *<input value={name} onChange={(e) => setName(e.target.value)} required /></label>
        <div className={styles.row}>
          <label>Contact<input value={contactPerson} onChange={(e) => setContactPerson(e.target.value)} /></label>
          <label>Phone<input value={phone} onChange={(e) => setPhone(e.target.value)} /></label>
          <label>Email<input type="email" value={email} onChange={(e) => setEmail(e.target.value)} /></label>
        </div>
        <label>Address<input value={address} onChange={(e) => setAddress(e.target.value)} /></label>
        <div className={styles.row}>
          <label>City<input value={city} onChange={(e) => setCity(e.target.value)} /></label>
          <label>Province<input value={province} onChange={(e) => setProvince(e.target.value)} /></label>
          <label>VAT<input value={vatNumber} onChange={(e) => setVatNumber(e.target.value)} /></label>
        </div>
        <label>Notes<textarea rows={2} value={notes} onChange={(e) => setNotes(e.target.value)} /></label>
        <div className={styles.actions}>
          <button type="button" className={styles.draft} onClick={() => navigate(`/suppliers/${id}`)}>Cancel</button>
          <button type="submit" className={styles.submit} disabled={mutation.isPending}>Save Changes</button>
        </div>
      </form>
    </div>
  )
}
