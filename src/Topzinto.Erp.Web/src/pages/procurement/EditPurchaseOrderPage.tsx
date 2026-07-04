import { useEffect, useState } from 'react'
import { useNavigate, useParams } from 'react-router-dom'
import { useQuery, useMutation } from '@tanstack/react-query'
import { Plus, Trash2 } from 'lucide-react'
import { getSuppliers } from '@/api/suppliers'
import { getProjects } from '@/api/projects'
import { getPurchaseOrder, updatePurchaseOrder } from '@/api/procurement'
import { parseDisplayDate } from '@/lib/parseDisplayDate'
import styles from '../site-reports/AddSiteReportPage.module.css'
import lineStyles from './AddPurchaseOrderPage.module.css'

interface LineRow {
  description: string
  quantity: number
  unit: string
  unitPrice: number
}

const emptyLine = (): LineRow => ({ description: '', quantity: 1, unit: 'ea', unitPrice: 0 })

export function EditPurchaseOrderPage() {
  const { id } = useParams<{ id: string }>()
  const navigate = useNavigate()
  const { data, isLoading } = useQuery({ queryKey: ['purchase-order', id], queryFn: () => getPurchaseOrder(id!), enabled: !!id })
  const { data: suppliers } = useQuery({ queryKey: ['suppliers'], queryFn: () => getSuppliers() })
  const { data: projects } = useQuery({ queryKey: ['projects'], queryFn: () => getProjects() })

  const [title, setTitle] = useState('')
  const [supplierId, setSupplierId] = useState('')
  const [projectId, setProjectId] = useState('')
  const [status, setStatus] = useState('Pending Approval')
  const [orderDate, setOrderDate] = useState('')
  const [requiredDate, setRequiredDate] = useState('')
  const [requestedByName, setRequestedByName] = useState('')
  const [approvedByName, setApprovedByName] = useState('')
  const [notes, setNotes] = useState('')
  const [lines, setLines] = useState<LineRow[]>([emptyLine()])

  useEffect(() => {
    if (!data) return
    setTitle(data.title)
    setSupplierId(data.supplierId)
    setProjectId(data.projectId ?? '')
    setStatus(data.status)
    setOrderDate(parseDisplayDate(data.orderDate))
    setRequiredDate(parseDisplayDate(data.requiredDate))
    setRequestedByName(data.requestedByName ?? '')
    setApprovedByName(data.approvedByName ?? '')
    setNotes(data.notes ?? '')
    setLines(
      data.lines.length > 0
        ? data.lines.map((l) => ({
            description: l.description,
            quantity: l.quantity,
            unit: l.unit,
            unitPrice: l.unitPrice,
          }))
        : [emptyLine()],
    )
  }, [data])

  const mutation = useMutation({
    mutationFn: () =>
      updatePurchaseOrder(id!, {
        title,
        supplierId,
        projectId: projectId || undefined,
        status,
        orderDate: orderDate ? new Date(orderDate).toISOString() : new Date().toISOString(),
        requiredDate: requiredDate || undefined,
        requestedByName: requestedByName || undefined,
        approvedByName: approvedByName || undefined,
        notes: notes || undefined,
        lines: lines.filter((l) => l.description.trim()).map((l) => ({
          description: l.description,
          quantity: l.quantity,
          unit: l.unit,
          unitPrice: l.unitPrice,
        })),
      }),
    onSuccess: () => navigate(`/procurement/${id}`),
  })

  function updateLine(index: number, patch: Partial<LineRow>) {
    setLines((prev) => prev.map((l, i) => (i === index ? { ...l, ...patch } : l)))
  }

  function handleSubmit(e: React.FormEvent) {
    e.preventDefault()
    if (!title.trim() || !supplierId || !lines.some((l) => l.description.trim())) return
    mutation.mutate()
  }

  if (isLoading) return <p>Loading...</p>

  return (
    <div className={styles.page} style={{ maxWidth: 960 }}>
      <nav className={styles.breadcrumb}>Home &gt; Procurement &gt; Edit PO</nav>
      <h1>Edit Purchase Order {data?.poNumber}</h1>

      <form className={styles.form} onSubmit={handleSubmit}>
        <div className={styles.row}>
          <label>
            PO Number
            <input value={data?.poNumber ?? ''} disabled />
          </label>
          <label>
            Status
            <select value={status} onChange={(e) => setStatus(e.target.value)}>
              <option value="Draft">Draft</option>
              <option value="Pending Approval">Pending Approval</option>
              <option value="Approved">Approved</option>
              <option value="Ordered">Ordered</option>
              <option value="Delivered">Delivered</option>
              <option value="Cancelled">Cancelled</option>
            </select>
          </label>
          <label>
            Order Date *
            <input type="date" value={orderDate} onChange={(e) => setOrderDate(e.target.value)} required />
          </label>
          <label>
            Required Date
            <input type="date" value={requiredDate} onChange={(e) => setRequiredDate(e.target.value)} />
          </label>
        </div>

        <label>
          Title *
          <input value={title} onChange={(e) => setTitle(e.target.value)} required />
        </label>

        <div className={styles.row}>
          <label>
            Supplier *
            <select value={supplierId} onChange={(e) => setSupplierId(e.target.value)} required>
              <option value="">Select supplier</option>
              {suppliers?.map((s) => (
                <option key={s.id} value={s.id}>{s.code} — {s.name}</option>
              ))}
            </select>
          </label>
          <label>
            Project (optional)
            <select value={projectId} onChange={(e) => setProjectId(e.target.value)}>
              <option value="">No project</option>
              {projects?.map((p) => (
                <option key={p.id} value={p.id}>{p.code} — {p.name}</option>
              ))}
            </select>
          </label>
        </div>

        <div className={styles.row}>
          <label>
            Requested By
            <input value={requestedByName} onChange={(e) => setRequestedByName(e.target.value)} />
          </label>
          <label>
            Approved By
            <input value={approvedByName} onChange={(e) => setApprovedByName(e.target.value)} />
          </label>
        </div>

        <fieldset className={styles.fieldset}>
          <legend>Line Items *</legend>
          <div className={lineStyles.lines}>
            {lines.map((line, i) => (
              <div key={i} className={lineStyles.lineRow}>
                <input
                  placeholder="Description"
                  value={line.description}
                  onChange={(e) => updateLine(i, { description: e.target.value })}
                  className={lineStyles.desc}
                />
                <input
                  type="number"
                  min={0}
                  step="0.001"
                  value={line.quantity}
                  onChange={(e) => updateLine(i, { quantity: +e.target.value })}
                  className={lineStyles.qty}
                />
                <input
                  value={line.unit}
                  onChange={(e) => updateLine(i, { unit: e.target.value })}
                  className={lineStyles.unit}
                />
                <input
                  type="number"
                  min={0}
                  value={line.unitPrice}
                  onChange={(e) => updateLine(i, { unitPrice: +e.target.value })}
                  className={lineStyles.price}
                  placeholder="Unit price"
                />
                {lines.length > 1 && (
                  <button type="button" className={lineStyles.removeBtn} onClick={() => setLines((p) => p.filter((_, j) => j !== i))}>
                    <Trash2 size={16} />
                  </button>
                )}
              </div>
            ))}
          </div>
          <button type="button" className={lineStyles.addBtn} onClick={() => setLines((p) => [...p, emptyLine()])}>
            <Plus size={16} /> Add line
          </button>
        </fieldset>

        <label>
          Notes
          <textarea rows={2} value={notes} onChange={(e) => setNotes(e.target.value)} />
        </label>

        <div className={styles.actions}>
          <button type="button" className={styles.draft} onClick={() => navigate(`/procurement/${id}`)}>Cancel</button>
          <button type="submit" className={styles.submit} disabled={mutation.isPending}>
            {mutation.isPending ? 'Saving...' : 'Save Changes'}
          </button>
        </div>
      </form>
    </div>
  )
}
