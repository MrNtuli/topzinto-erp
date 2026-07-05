import { useEffect, useMemo, useState } from 'react'
import { useNavigate, useParams } from 'react-router-dom'
import { useMutation, useQuery } from '@tanstack/react-query'
import { getBoqItem, updateBoqItem } from '@/api/boq'
import styles from '../site-reports/AddSiteReportPage.module.css'

export function EditBoqItemPage() {
  const { id } = useParams<{ id: string }>()
  const navigate = useNavigate()
  const { data, isLoading, isError } = useQuery({
    queryKey: ['boq-item', id],
    queryFn: () => getBoqItem(id!),
    enabled: !!id,
  })

  const [itemCode, setItemCode] = useState('')
  const [description, setDescription] = useState('')
  const [category, setCategory] = useState('General')
  const [unit, setUnit] = useState('ea')
  const [quantity, setQuantity] = useState('1')
  const [rate, setRate] = useState('')
  const [notes, setNotes] = useState('')

  useEffect(() => {
    if (!data) return
    setItemCode(data.itemCode)
    setDescription(data.description)
    setCategory(data.category)
    setUnit(data.unit)
    setQuantity(String(data.quantity))
    setRate(String(data.rate))
    setNotes(data.notes ?? '')
  }, [data])

  const amount = useMemo(() => (parseFloat(quantity) || 0) * (parseFloat(rate) || 0), [quantity, rate])

  const mutation = useMutation({
    mutationFn: () =>
      updateBoqItem(id!, {
        itemCode,
        description,
        category,
        unit,
        quantity: parseFloat(quantity) || 0,
        rate: parseFloat(rate) || 0,
        notes: notes || undefined,
      }),
    onSuccess: () => navigate('/boq'),
  })

  function handleSubmit(e: React.FormEvent) {
    e.preventDefault()
    if (!itemCode.trim() || !description.trim()) return
    mutation.mutate()
  }

  if (isLoading) return <p className={styles.page}>Loading BOQ item...</p>
  if (isError || !data) return <p className={styles.page}>BOQ item not found.</p>

  return (
    <div className={styles.page}>
      <nav className={styles.breadcrumb}>Home &gt; BOQ &gt; Edit Item</nav>
      <h1>Edit BOQ Item</h1>

      <form className={styles.form} onSubmit={handleSubmit}>
        <label>
          Project (read-only)
          <input value={`${data.projectName}`} disabled />
        </label>

        <div className={styles.row}>
          <label>
            Item Code *
            <input value={itemCode} onChange={(e) => setItemCode(e.target.value)} required />
          </label>
          <label>
            Category
            <select value={category} onChange={(e) => setCategory(e.target.value)}>
              <option value="General">General</option>
              <option value="Earthworks">Earthworks</option>
              <option value="Concrete">Concrete</option>
              <option value="Steel">Steel</option>
              <option value="Masonry">Masonry</option>
              <option value="Roofing">Roofing</option>
              <option value="Paving">Paving</option>
              <option value="Electrical">Electrical</option>
              <option value="Plumbing">Plumbing</option>
            </select>
          </label>
        </div>

        <label>
          Description *
          <input value={description} onChange={(e) => setDescription(e.target.value)} required />
        </label>

        <div className={styles.row}>
          <label>
            Quantity *
            <input type="number" min="0" step="0.001" value={quantity} onChange={(e) => setQuantity(e.target.value)} required />
          </label>
          <label>
            Unit
            <input value={unit} onChange={(e) => setUnit(e.target.value)} />
          </label>
          <label>
            Rate (ZAR) *
            <input type="number" min="0" step="0.01" value={rate} onChange={(e) => setRate(e.target.value)} required />
          </label>
          <label>
            Amount (calculated)
            <input value={amount.toFixed(2)} disabled />
          </label>
        </div>

        <label>
          Notes
          <textarea rows={2} value={notes} onChange={(e) => setNotes(e.target.value)} />
        </label>

        {mutation.isError && (
          <p style={{ color: 'var(--color-danger, #c0392b)', fontSize: 14 }}>
            Failed to save changes. Check your connection and try again.
          </p>
        )}

        <div className={styles.actions}>
          <button type="button" className={styles.draft} onClick={() => navigate('/boq')}>Cancel</button>
          <button type="submit" className={styles.submit} disabled={mutation.isPending}>
            {mutation.isPending ? 'Saving...' : 'Save Changes'}
          </button>
        </div>
      </form>
    </div>
  )
}
