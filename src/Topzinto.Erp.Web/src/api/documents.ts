import { apiFetch } from './client'

export interface DocumentSummary {
  total: number
  expiringSoon: number
  expired: number
}

export interface Document {
  id: string
  title: string
  category: string
  parentType: string
  parentName: string | null
  fileName: string | null
  version: number
  status: string
  issueDate: string | null
  expiryDate: string | null
  isExpiringSoon: boolean
  hasFile: boolean
  fileSizeBytes: number | null
}

export function getDocumentsSummary() {
  return apiFetch<DocumentSummary>('/documents/summary')
}

export function getDocuments(params?: { search?: string; parentType?: string; parentId?: string; expiringOnly?: boolean }) {
  const q = new URLSearchParams()
  if (params?.search) q.set('search', params.search)
  if (params?.parentType) q.set('parentType', params.parentType)
  if (params?.parentId) q.set('parentId', params.parentId)
  if (params?.expiringOnly) q.set('expiringOnly', 'true')
  const query = q.toString()
  return apiFetch<Document[]>(`/documents${query ? `?${query}` : ''}`)
}

export function createDocument(data: {
  title: string
  category: string
  parentType: string
  parentId?: string
  parentName?: string
  fileName?: string
  issueDate?: string
  expiryDate?: string
  status: string
  notes?: string
}) {
  return apiFetch<Document>('/documents', { method: 'POST', body: JSON.stringify(data) })
}

export async function uploadDocumentFile(id: string, file: File) {
  const token = JSON.parse(localStorage.getItem('topzinto-auth') || '{}')?.state?.accessToken
  const form = new FormData()
  form.append('file', file)
  const res = await fetch(`/api/documents/${id}/upload`, {
    method: 'POST',
    headers: token ? { Authorization: `Bearer ${token}` } : {},
    body: form,
  })
  if (!res.ok) throw new Error('Upload failed')
  return res.json() as Promise<Document>
}

export function getDocumentDownloadUrl(id: string) {
  return `/api/documents/${id}/download`
}

export async function downloadDocumentFile(id: string, fileName?: string | null) {
  const token = JSON.parse(localStorage.getItem('topzinto-auth') || '{}')?.state?.accessToken
  const res = await fetch(`/api/documents/${id}/download`, {
    headers: token ? { Authorization: `Bearer ${token}` } : {},
  })
  if (!res.ok) throw new Error('Download failed')
  const blob = await res.blob()
  const url = URL.createObjectURL(blob)
  const a = document.createElement('a')
  a.href = url
  a.download = fileName ?? 'download'
  a.click()
  URL.revokeObjectURL(url)
}
