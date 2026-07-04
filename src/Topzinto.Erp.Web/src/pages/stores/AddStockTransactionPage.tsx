import { useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { useQuery, useMutation } from '@tanstack/react-query'
import { getInventoryItems, createInventoryTransaction } from '@/api/stores'
import { getProjects } from '@/api/projects'
import styles from '../site-reports/AddSiteReportPage.module.css'

export function AddStockTransactionPage() {
  const navigate = useNavigate()
  const { data: items } = useQuery({ queryKey: ['inventory'], queryFn: () => getInventoryItems() })
  const { data: projects } = useQuery({ queryKey: ['projects'], queryFn: () => getProjects() })

  const [inventoryItemId, setInventoryItemId] = useState('')
  const [transactionType, setTransactionType] = useState('Issue')
  const [quantity, setQuantity] = useState('1')
  const [reference, setReference] = useState('')
  const [projectId, setProjectId] = useState('')
  const [notes, setNotes] = useState('')

  const mutation = useMutation({
    mutationFn: () =>
      createInventoryTransaction({
        inventoryItemId,
        transactionType,
        quantity: Number(quantity) || 0,
        transactionDate: new Date().toISOString(),
        reference: reference || undefined,
        projectId: projectId || undefined,
        notes: notes || undefined,
      }),
    onSuccess: () => navigate('/stores/transactions'),
  })

  return (
    <div className={styles.page}>
      <nav className={styles.breadcrumb}>Home &gt; Stores &gt; Record Transaction</nav>
      <h1>Stock Transaction</h1>
      <form className={styles.form} onSubmit={(e) => { e.preventDefault(); if (!inventoryItemId) return; mutation.mutate() }}>
        <label>
          Item *
          <select value={inventoryItemId} onChange={(e) => setInventoryItemId(e.target.value)} required>
            <option value="">Select item</option>
            {items?.map((i) => <option key={i.id} value={i.id}>{i.itemCode} — {i.name}</option>)}
          </select>
        </label>
        <div className={styles.row}>
          <label>
            Type
            <select value={transactionType} onChange={(e) => setTransactionType(e.target.value)}>
              <option value="Receipt">Receipt</option>
              <option value="Issue">Issue</option>
              <option value="Adjustment">Adjustment</option>
              <option value="Return">Return</option>
            </select>
          </label>
          <label>Quantity *<input type="number" min={0} step="0.001" value={quantity} onChange={(e) => setQuantity(e.target.value)} required /></label>
          <label>Reference<input value={reference} onChange={(e) => setReference(e.target.value)} /></label>
        </div>
        <label>
          Project (optional)
          <select value={projectId} onChange={(e) => setProjectId(e.target.value)}>
            <option value="">None</option>
            {projects?.map((p) => <option key={p.id} value={p.id}>{p.name}</option>)}
          </select>
        </label>
        <label>Notes<textarea rows={2} value={notes} onChange={(e) => setNotes(e.target.value)} /></label>
        <div className={styles.actions}>
          <button type="button" className={styles.draft} onClick={() => navigate('/stores')}>Cancel</button>
          <button type="submit" className={styles.submit} disabled={mutation.isPending}>Record</button>
        </div>
      </form>
    </div>
  )
}
