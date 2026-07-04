import { authorizedFetch } from './client'

async function downloadCsv(path: string, filename: string) {
  const res = await authorizedFetch(`/export/${path}`)
  if (!res.ok) throw new Error('Export failed')
  const blob = await res.blob()
  const url = URL.createObjectURL(blob)
  const a = document.createElement('a')
  a.href = url
  a.download = filename
  a.click()
  URL.revokeObjectURL(url)
}

const today = () => new Date().toISOString().slice(0, 10)

export function exportProjectsCsv() {
  return downloadCsv('projects', `projects_${today()}.csv`)
}

export function exportBoqCsv() {
  return downloadCsv('boq', `boq_${today()}.csv`)
}

export function exportClaimsCsv() {
  return downloadCsv('claims', `claims_${today()}.csv`)
}

export function exportSuppliersCsv() {
  return downloadCsv('suppliers', `suppliers_${today()}.csv`)
}

export function exportProcurementCsv() {
  return downloadCsv('procurement', `procurement_${today()}.csv`)
}

export function exportInvoicesCsv() {
  return downloadCsv('invoices', `invoices_${today()}.csv`)
}

export function exportFleetCsv() {
  return downloadCsv('fleet', `fleet_${today()}.csv`)
}

export function exportDocumentsCsv() {
  return downloadCsv('documents', `documents_${today()}.csv`)
}

export function exportEmployeesCsv() {
  return downloadCsv('employees', `employees_${today()}.csv`)
}

export function exportTimesheetsCsv() {
  return downloadCsv('timesheets', `timesheets_${today()}.csv`)
}

export async function exportProjectsExcel() {
  return downloadExcel('projects/excel', `projects_${today()}.xlsx`)
}

export function exportSuppliersExcel() {
  return downloadExcel('suppliers/excel', `suppliers_${today()}.xlsx`)
}

export function exportProcurementExcel() {
  return downloadExcel('procurement/excel', `procurement_${today()}.xlsx`)
}

export function exportEmployeesExcel() {
  return downloadExcel('employees/excel', `employees_${today()}.xlsx`)
}

async function downloadExcel(path: string, filename: string) {
  const res = await authorizedFetch(`/export/${path}`)
  if (!res.ok) throw new Error('Export failed')
  const blob = await res.blob()
  const url = URL.createObjectURL(blob)
  const a = document.createElement('a')
  a.href = url
  a.download = filename
  a.click()
  URL.revokeObjectURL(url)
}
