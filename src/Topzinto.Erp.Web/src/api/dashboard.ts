import { apiFetch } from './client'
import { formatCurrency } from './projects'

export interface DashboardData {
  activeProjects: number
  activeTenders: number
  totalContractValue: number
  pendingClaimsValue: number
  fleetInUse: number
  fleetTotal: number
  equipmentInUse: number
  equipmentTotal: number
  activeUsers: number
  overdueTasks: number
  documentsExpiringSoon: number
  projectProgress: {
    completed: number
    inProgress: number
    onHold: number
    planned: number
  }
  latestSiteReports: {
    id: string
    projectName: string
    reportDate: string
    status: string
  }[]
  financialTrend: {
    label: string
    claimsAmount: number
    procurementAmount: number
  }[]
}

export function getDashboard(refresh = false) {
  const query = refresh ? '?refresh=true' : ''
  return apiFetch<DashboardData>(`/dashboard${query}`)
}

export { formatCurrency }
