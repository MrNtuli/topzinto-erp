import { apiFetch } from './client'

export interface SearchResult {
  type: string
  id: string
  title: string
  subtitle: string
  linkPath: string
}

export function search(q: string, limit = 20) {
  return apiFetch<SearchResult[]>(`/search?q=${encodeURIComponent(q)}&limit=${limit}`)
}
