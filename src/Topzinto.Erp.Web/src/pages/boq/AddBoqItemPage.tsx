import { useState, useMemo } from 'react'
import { useNavigate } from 'react-router-dom'
import { useQuery, useMutation } from '@tanstack/react-query'
import { getProjects } from '@/api/projects'
import { createBoqItem } from '@/api/boq'
import styles from '../site-reports/AddSiteReportPage.module.css'

export function AddBoqItemPage() {
  const navigate = useNavigate()
  const { data: projects } = useQuery({ queryKey: ['projects'], queryFn: () => getProjects() })

  const [projectId, setProjectId] = useState('')
  const [itemCode, setItemCode] = useState('')
  const [description, setDescription] = useState('')
  const [category, setCategory] = useState('General')
  const [unit, setUnit] = useState('ea')
  const [quantity, setQuantity] = useState('1')
  const [rate, setRate] = useState('')
  const [notes, setNotes] = useState('')

  const amount = useMemo(() => (parseFloat(quantity) || 0) * (parseFloat(rate) || 0), [quantity, rate])

  const mutation = useMutation({
    mutationFn: () =>
      createBoqItem({
        projectId,
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
    if (!projectId || !itemCode.trim() || !description.trim()) return
    mutation.mutate()
  }

  return (
    <div className={styles.page}>
      <nav className={styles.breadcrumb}>Home &gt; BOQ &gt; New Item</nav>
      <h1>New BOQ Item</h1>

      <form className={styles.form} onSubmit={handleSubmit}>
        <label>
          Project *
          <select value={projectId} onChange={(e) => setProjectId(e.target.value)} required>
            <option value="">Select project</option>
            {projects?.map((p) => (
              <option key={p.id} value={p.id}>{p.code} — {p.name}</option>
            ))}
          </select>
        </label>

        <div className={styles.row}>
          <label>
            Item Code *
            <input value={itemCode} onChange={(e) => setItemCode(e.target.value)} placeholder="BOQ-006" required />
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
            <input value={unit} onChange={(e) => setUnit(e.target.value)} placeholder="m³, ea, ton" />
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

        <div className={styles.actions}>
          <button type="button" className={styles.draft} onClick={() => navigate('/boq')}>Cancel</button>
          <button type="submit" className={styles.submit} disabled={mutation.isPending}>
            {mutation.isPending ? 'Saving...' : 'Create BOQ Item'}
          </button>
        </div>
      </form>
    </div>
  )
}
