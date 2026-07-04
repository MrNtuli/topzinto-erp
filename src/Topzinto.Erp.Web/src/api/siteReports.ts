import { apiFetch, authorizedFetch } from './client'

export interface SiteReportPhoto {
  id: string
  fileName: string
  contentType: string
  sizeBytes: number
  caption: string | null
}

export interface SiteReport {
  id: string
  projectId: string
  projectName: string
  reportDate: string
  weather: string | null
  status: string
  submittedByName: string | null
  workCompletedPreview: string
}

export interface SiteReportDetail extends Omit<SiteReport, 'workCompletedPreview'> {
  temperature: string | null
  windSpeed: string | null
  personnelCount: number | null
  workCompleted: string
  workPlanned: string | null
  delaysIssues: string | null
  notes: string | null
  submittedAt: string | null
  photos: SiteReportPhoto[]
}

export function getSiteReports(projectId?: string) {
  const q = projectId ? `?projectId=${projectId}` : ''
  return apiFetch<SiteReport[]>(`/sitereports${q}`)
}

export function getSiteReport(id: string) {
  return apiFetch<SiteReportDetail>(`/sitereports/${id}`)
}

export function createSiteReport(data: {
  projectId: string
  reportDate: string
  weather?: string
  temperature?: string
  windSpeed?: string
  personnelCount?: number
  workCompleted: string
  workPlanned?: string
  delaysIssues?: string
  notes?: string
  submit: boolean
}) {
  return apiFetch<SiteReportDetail>('/sitereports', { method: 'POST', body: JSON.stringify(data) })
}

export function updateSiteReport(id: string, data: {
  weather?: string
  temperature?: string
  windSpeed?: string
  personnelCount?: number
  workCompleted: string
  workPlanned?: string
  delaysIssues?: string
  notes?: string
  submit: boolean
}) {
  return apiFetch<SiteReportDetail>(`/sitereports/${id}`, { method: 'PUT', body: JSON.stringify(data) })
}

export async function uploadSiteReportPhoto(reportId: string, file: File, caption?: string) {
  const form = new FormData()
  form.append('file', file)
  if (caption) form.append('caption', caption)

  const res = await authorizedFetch(`/sitereports/${reportId}/photos`, {
    method: 'POST',
    body: form,
  })
  if (!res.ok) throw new Error('Upload failed')
  return res.json() as Promise<SiteReportPhoto>
}

export async function fetchSiteReportPhotoBlob(reportId: string, photoId: string) {
  const res = await authorizedFetch(`/sitereports/${reportId}/photos/${photoId}`)
  if (!res.ok) throw new Error('Photo not found')
  return res.blob()
}

export async function downloadSiteReportPdf(id: string, fileName?: string) {
  const res = await authorizedFetch(`/sitereports/${id}/pdf`)
  if (!res.ok) throw new Error('PDF download failed')
  const blob = await res.blob()
  const url = URL.createObjectURL(blob)
  const a = document.createElement('a')
  a.href = url
  a.download = fileName ?? `site_report_${id}.pdf`
  a.click()
  URL.revokeObjectURL(url)
}
