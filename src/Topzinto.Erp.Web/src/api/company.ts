import { apiFetch } from './client'

export interface CompanySettings {
  companyName: string
  tagline: string
  address: string | null
  city: string | null
  province: string | null
  phone: string | null
  email: string | null
  vatNumber: string | null
  cidbNumber: string | null
}

export function getCompanySettings() {
  return apiFetch<CompanySettings>('/company')
}

export function updateCompanySettings(data: CompanySettings) {
  return apiFetch<CompanySettings>('/company', { method: 'PUT', body: JSON.stringify(data) })
}
