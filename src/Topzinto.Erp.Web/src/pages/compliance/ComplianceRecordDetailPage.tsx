import { useState } from 'react'
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { useParams, Link, useNavigate } from 'react-router-dom'
import { deleteComplianceRecord, getComplianceRecord } from '@/api/compliance'
import styles from '../clients/ClientDetailPage.module.css'
import listStyles from './ComplianceRecordsPage.module.css'

function typeClass(type: string) {
  switch (type) {
    case 'Insurance': return listStyles.typeInsurance
    case 'License': return listStyles.typeLicense
    case 'Certificate': return listStyles.typeCertificate
    case 'Permit': return listStyles.typePermit
    case 'Inspection': return listStyles.typeInspection
    default: return listStyles.typeOther
  }
}

function statusClass(status: string) {
  switch (status) {
    case 'Valid': return listStyles.statusValid
    case 'Expiring Soon': return listStyles.statusExpiringSoon
    case 'Expired': return listStyles.statusExpired
    case 'Pending': return listStyles.statusPending
    case 'Revoked': return listStyles.statusRevoked
    default: return listStyles.statusValid
  }
}

export function ComplianceRecordDetailPage() {
  const { id } = useParams<{ id: string }>()
  const navigate = useNavigate()
  const queryClient = useQueryClient()
  const [confirmDelete, setConfirmDelete] = useState(false)

  const { data, isLoading, isError } = useQuery({
    queryKey: ['compliance-record', id],
    queryFn: () => getComplianceRecord(id!),
    enabled: !!id,
    retry: false,
  })

  const deleteMutation = useMutation({
    mutationFn: () => deleteComplianceRecord(id!),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['compliance-records'] })
      navigate('/compliance')
    },
  })

  if (isLoading) return <p>Loading...</p>
  if (isError || !data) return <p>Compliance record not found.</p>

  return (
    <div className={styles.page}>
      <nav className={styles.breadcrumb}>
        <Link to="/compliance">Compliance Records</Link> &gt; {data.title}
      </nav>
      <header className={styles.header}>
        <h1>{data.title}</h1>
        <span className={`${styles.badge} ${typeClass(data.type)}`}>{data.type}</span>
        <span className={`${styles.badge} ${statusClass(data.status)}`}>{data.status}</span>
        <div className={styles.headerActions}>
          <Link to={`/compliance/${id}/edit`} className={styles.editBtn}>Edit</Link>
          {!confirmDelete ? (
            <button type="button" className={styles.editBtn} onClick={() => setConfirmDelete(true)}>Delete</button>
          ) : (
            <button
              type="button"
              className={styles.pdfBtn}
              disabled={deleteMutation.isPending}
              onClick={() => deleteMutation.mutate()}
            >
              {deleteMutation.isPending ? 'Deleting...' : 'Confirm Delete'}
            </button>
          )}
        </div>
      </header>

      <div className={styles.grid}>
        <div className={styles.card}>
          <h3>Record Details</h3>
          <dl>
            <dt>Type</dt><dd>{data.type}</dd>
            <dt>Status</dt><dd>{data.status}</dd>
            <dt>Entity Type</dt><dd>{data.entityType ?? '—'}</dd>
            <dt>Project</dt><dd>{data.projectName ?? '—'}</dd>
            <dt>Issue Date</dt><dd>{data.issueDate}</dd>
            <dt>Expiry Date</dt><dd>{data.expiryDate ?? '—'}</dd>
            <dt>Responsible Person</dt><dd>{data.responsiblePerson ?? '—'}</dd>
          </dl>
        </div>
        <div className={styles.card}>
          <h3>Notes</h3>
          <p>{data.notes ?? 'No notes recorded.'}</p>
        </div>
      </div>
    </div>
  )
}
