import { useEffect, useState } from 'react'
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { useParams, Link } from 'react-router-dom'
import { Camera } from 'lucide-react'
import { getSiteReport, downloadSiteReportPdf, uploadSiteReportPhoto, fetchSiteReportPhotoBlob } from '@/api/siteReports'
import styles from '../clients/ClientDetailPage.module.css'
import photoStyles from './SiteReportDetailPage.module.css'

function SitePhoto({ reportId, photo }: { reportId: string; photo: { id: string; fileName: string; caption: string | null } }) {
  const [src, setSrc] = useState<string | null>(null)

  useEffect(() => {
    let objectUrl: string | null = null
    fetchSiteReportPhotoBlob(reportId, photo.id)
      .then((blob) => {
        objectUrl = URL.createObjectURL(blob)
        setSrc(objectUrl)
      })
      .catch(() => setSrc(null))
    return () => {
      if (objectUrl) URL.revokeObjectURL(objectUrl)
    }
  }, [reportId, photo.id])

  if (!src) return <div className={photoStyles.photoPlaceholder}>Loading...</div>

  return (
    <figure className={photoStyles.photoCard}>
      <img src={src} alt={photo.caption ?? photo.fileName} />
      {photo.caption && <figcaption>{photo.caption}</figcaption>}
    </figure>
  )
}

export function SiteReportDetailPage() {
  const { id } = useParams<{ id: string }>()
  const queryClient = useQueryClient()
  const [uploadError, setUploadError] = useState('')

  const { data, isLoading, isError } = useQuery({
    queryKey: ['site-report', id],
    queryFn: () => getSiteReport(id!),
    enabled: !!id,
    retry: false,
  })

  const uploadMutation = useMutation({
    mutationFn: (file: File) => uploadSiteReportPhoto(id!, file),
    onSuccess: () => {
      setUploadError('')
      queryClient.invalidateQueries({ queryKey: ['site-report', id] })
    },
    onError: () => setUploadError('Upload failed. Use JPEG/PNG/WebP, max 10 MB, up to 5 photos.'),
  })

  if (isLoading) return <p>Loading...</p>
  if (isError || !data || !id) return <p>Site report not found.</p>

  const canUpload = (data.photos?.length ?? 0) < 5

  return (
    <div className={styles.page}>
      <nav className={styles.breadcrumb}>
        <Link to="/site-reports">Site Reports</Link> &gt; {data.reportDate}
      </nav>
      <header className={styles.header}>
        <h1>{data.projectName}</h1>
        <span className={styles.badge}>{data.status}</span>
        <div className={styles.headerActions}>
          <Link to={`/site-reports/${id}/edit`} className={styles.editBtn}>Edit</Link>
          <button type="button" className={styles.pdfBtn} onClick={() => downloadSiteReportPdf(id, `site_report_${data.reportDate}.pdf`)}>
            Download PDF
          </button>
        </div>
      </header>

      <div className={styles.grid}>
        <div className={`${styles.card} ${photoStyles.wide}`}>
          <div className={photoStyles.photoHeader}>
            <h3>Site Photos ({data.photos?.length ?? 0}/5)</h3>
            {canUpload && (
              <label className={photoStyles.uploadBtn}>
                <Camera size={16} />
                Add Photo
                <input
                  type="file"
                  accept="image/jpeg,image/png,image/webp,image/gif"
                  hidden
                  onChange={(e) => {
                    const file = e.target.files?.[0]
                    if (file) uploadMutation.mutate(file)
                    e.target.value = ''
                  }}
                />
              </label>
            )}
          </div>
          {uploadMutation.isPending && <p className={photoStyles.note}>Uploading...</p>}
          {uploadError && <p className={photoStyles.error}>{uploadError}</p>}
          {data.photos && data.photos.length > 0 ? (
            <div className={photoStyles.gallery}>
              {data.photos.map((photo) => (
                <SitePhoto key={photo.id} reportId={id} photo={photo} />
              ))}
            </div>
          ) : (
            <p className={photoStyles.note}>No photos yet. Add site progress images from the field.</p>
          )}
        </div>

        <div className={styles.card}>
          <h3>Report Details</h3>
          <dl>
            <dt>Date</dt><dd>{data.reportDate}</dd>
            <dt>Weather</dt><dd>{data.weather ?? '—'}</dd>
            <dt>Temperature</dt><dd>{data.temperature ?? '—'}</dd>
            <dt>Wind</dt><dd>{data.windSpeed ?? '—'}</dd>
            <dt>Personnel on Site</dt><dd>{data.personnelCount ?? '—'}</dd>
            <dt>Submitted By</dt><dd>{data.submittedByName ?? '—'}</dd>
            <dt>Submitted At</dt><dd>{data.submittedAt ?? '—'}</dd>
          </dl>
        </div>
        <div className={styles.card}>
          <h3>Work Completed</h3>
          <p>{data.workCompleted}</p>
        </div>
        <div className={styles.card}>
          <h3>Work Planned</h3>
          <p>{data.workPlanned ?? '—'}</p>
        </div>
        <div className={styles.card}>
          <h3>Delays &amp; Issues</h3>
          <p>{data.delaysIssues ?? 'None reported.'}</p>
        </div>
        {data.notes && (
          <div className={styles.card}>
            <h3>Notes</h3>
            <p>{data.notes}</p>
          </div>
        )}
      </div>
    </div>
  )
}
