import { apiFetch } from './client'

export interface FleetSummary {
  total: number
  available: number
  inUse: number
  maintenance: number
  expiringSoon: number
}

export interface Vehicle {
  id: string
  registrationNumber: string
  makeModel: string
  type: string
  status: string
  driverName: string | null
  currentLocation: string | null
  assignedProjectName: string | null
  licenseExpiryDate: string | null
  isExpiringSoon: boolean
}

export interface VehicleDetail extends Vehicle {
  driverLicenseNumber: string | null
  insuranceExpiryDate: string | null
  roadworthyExpiryDate: string | null
  assignedProjectId: string | null
  notes: string | null
  licenseExpiryInput: string | null
  insuranceExpiryInput: string | null
  roadworthyExpiryInput: string | null
  maintenanceRecords: { id: string; serviceDate: string; description: string; cost: number; nextServiceDue: string | null; serviceProvider: string | null }[]
  fuelLogs: { id: string; logDate: string; litres: number; cost: number; odometerReading: number | null; notes: string | null }[]
}

export function getFleetSummary() {
  return apiFetch<FleetSummary>('/fleet/summary')
}

export function getVehicles(params?: { search?: string; status?: string }) {
  const q = new URLSearchParams()
  if (params?.search) q.set('search', params.search)
  if (params?.status) q.set('status', params.status)
  const query = q.toString()
  return apiFetch<Vehicle[]>(`/fleet${query ? `?${query}` : ''}`)
}

export function getVehicle(id: string) {
  return apiFetch<VehicleDetail>(`/fleet/${id}`)
}

export function createVehicle(data: {
  registrationNumber: string
  makeModel: string
  type: string
  status: string
  driverName?: string
  driverLicenseNumber?: string
  licenseExpiryDate?: string
  insuranceExpiryDate?: string
  roadworthyExpiryDate?: string
  currentLocation?: string
  assignedProjectId?: string
  notes?: string
}) {
  return apiFetch<VehicleDetail>('/fleet', { method: 'POST', body: JSON.stringify(data) })
}

export function updateVehicle(id: string, data: Parameters<typeof createVehicle>[0]) {
  return apiFetch<VehicleDetail>(`/fleet/${id}`, { method: 'PUT', body: JSON.stringify(data) })
}
