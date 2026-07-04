import { useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { useMutation } from '@tanstack/react-query'
import { createInventoryItem } from '@/api/stores'
import styles from '../site-reports/AddSiteReportPage.module.css'

export function AddInventoryItemPage() {
  const navigate = useNavigate()
  const [itemCode, setItemCode] = useState('')
  const [name, setName] = useState('')
  const [category, setCategory] = useState('Materials')
  const [unit, setUnit] = useState('ea')
  const [quantityOnHand, setQuantityOnHand] = useState('0')
  const [reorderLevel, setReorderLevel] = useState('0')
  const [location, setLocation] = useState('')
  const [unitCost, setUnitCost] = useState('0')
  const [notes, setNotes] = useState('')

  const mutation = useMutation({
    mutationFn: () =>
      createInventoryItem({
        itemCode,
        name,
        category,
        unit,
        quantityOnHand: Number(quantityOnHand) || 0,
        reorderLevel: Number(reorderLevel) || 0,
        location: location || undefined,
        unitCost: Number(unitCost) || 0,
        notes: notes || undefined,
      }),
    onSuccess: () => navigate('/stores'),
  })

  return (
    <div className={styles.page}>
      <nav className={styles.breadcrumb}>Home &gt; Stores &gt; New Item</nav>
      <h1>Add Inventory Item</h1>
      <form className={styles.form} onSubmit={(e) => { e.preventDefault(); mutation.mutate() }}>
        <div className={styles.row}>
          <label>Item Code *<input value={itemCode} onChange={(e) => setItemCode(e.target.value)} required /></label>
          <label>Category<input value={category} onChange={(e) => setCategory(e.target.value)} /></label>
          <label>Unit<input value={unit} onChange={(e) => setUnit(e.target.value)} /></label>
        </div>
        <label>Name *<input value={name} onChange={(e) => setName(e.target.value)} required /></label>
        <div className={styles.row}>
          <label>Qty on Hand<input type="number" min={0} step="0.001" value={quantityOnHand} onChange={(e) => setQuantityOnHand(e.target.value)} /></label>
          <label>Reorder Level<input type="number" min={0} step="0.001" value={reorderLevel} onChange={(e) => setReorderLevel(e.target.value)} /></label>
          <label>Unit Cost<input type="number" min={0} value={unitCost} onChange={(e) => setUnitCost(e.target.value)} /></label>
        </div>
        <label>Location<input value={location} onChange={(e) => setLocation(e.target.value)} /></label>
        <label>Notes<textarea rows={2} value={notes} onChange={(e) => setNotes(e.target.value)} /></label>
        <div className={styles.actions}>
          <button type="button" className={styles.draft} onClick={() => navigate('/stores')}>Cancel</button>
          <button type="submit" className={styles.submit} disabled={mutation.isPending}>Create Item</button>
        </div>
      </form>
    </div>
  )
}
