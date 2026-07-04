import { useEffect, useState } from 'react'
import { useNavigate, useParams } from 'react-router-dom'
import { useQuery, useMutation } from '@tanstack/react-query'
import { getInventoryItem, updateInventoryItem } from '@/api/stores'
import styles from '../site-reports/AddSiteReportPage.module.css'

export function EditInventoryItemPage() {
  const { id } = useParams<{ id: string }>()
  const navigate = useNavigate()
  const { data, isLoading } = useQuery({ queryKey: ['inventory-item', id], queryFn: () => getInventoryItem(id!), enabled: !!id })

  const [itemCode, setItemCode] = useState('')
  const [name, setName] = useState('')
  const [category, setCategory] = useState('Materials')
  const [unit, setUnit] = useState('ea')
  const [reorderLevel, setReorderLevel] = useState('0')
  const [location, setLocation] = useState('')
  const [unitCost, setUnitCost] = useState('0')
  const [notes, setNotes] = useState('')

  useEffect(() => {
    if (!data) return
    setItemCode(data.itemCode)
    setName(data.name)
    setCategory(data.category)
    setUnit(data.unit)
    setReorderLevel(String(data.reorderLevel))
    setLocation(data.location ?? '')
    setUnitCost(String(data.unitCost))
    setNotes(data.notes ?? '')
  }, [data])

  const mutation = useMutation({
    mutationFn: () =>
      updateInventoryItem(id!, {
        itemCode,
        name,
        category,
        unit,
        reorderLevel: Number(reorderLevel) || 0,
        location: location || undefined,
        unitCost: Number(unitCost) || 0,
        notes: notes || undefined,
      }),
    onSuccess: () => navigate(`/stores/${id}`),
  })

  if (isLoading) return <p>Loading...</p>

  return (
    <div className={styles.page}>
      <nav className={styles.breadcrumb}>Home &gt; Stores &gt; Edit Item</nav>
      <h1>Edit Inventory Item</h1>
      <form className={styles.form} onSubmit={(e) => { e.preventDefault(); mutation.mutate() }}>
        <div className={styles.row}>
          <label>Item Code *<input value={itemCode} onChange={(e) => setItemCode(e.target.value)} required /></label>
          <label>Category<input value={category} onChange={(e) => setCategory(e.target.value)} /></label>
          <label>Unit<input value={unit} onChange={(e) => setUnit(e.target.value)} /></label>
        </div>
        <label>Name *<input value={name} onChange={(e) => setName(e.target.value)} required /></label>
        <div className={styles.row}>
          <label>
            Qty on Hand (read-only)
            <input type="number" value={data?.quantityOnHand ?? 0} disabled />
          </label>
          <label>Reorder Level<input type="number" min={0} step="0.001" value={reorderLevel} onChange={(e) => setReorderLevel(e.target.value)} /></label>
          <label>Unit Cost<input type="number" min={0} value={unitCost} onChange={(e) => setUnitCost(e.target.value)} /></label>
        </div>
        <p style={{ fontSize: 14, color: 'var(--color-text-muted)' }}>Adjust quantity via stock transactions, not this form.</p>
        <label>Location<input value={location} onChange={(e) => setLocation(e.target.value)} /></label>
        <label>Notes<textarea rows={2} value={notes} onChange={(e) => setNotes(e.target.value)} /></label>
        <div className={styles.actions}>
          <button type="button" className={styles.draft} onClick={() => navigate(`/stores/${id}`)}>Cancel</button>
          <button type="submit" className={styles.submit} disabled={mutation.isPending}>Save Changes</button>
        </div>
      </form>
    </div>
  )
}
