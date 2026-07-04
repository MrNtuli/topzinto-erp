import { apiFetch } from './client'

export interface ReportCard {
  id: string
  title: string
  description: string
  value: string
  link: string
}

export interface ReportsHub {
  reports: ReportCard[]
}

export function getReportsHub() {
  return apiFetch<ReportsHub>('/reports/hub')
}
