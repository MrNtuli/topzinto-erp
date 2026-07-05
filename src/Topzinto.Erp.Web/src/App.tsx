import { Navigate, Route, Routes } from 'react-router-dom'
import { useAuthStore } from '@/stores/authStore'
import { AppShell } from '@/components/layout/AppShell'
import { LoginPage } from '@/pages/auth/LoginPage'
import { ForgotPasswordPage } from '@/pages/auth/ForgotPasswordPage'
import { ResetPasswordPage } from '@/pages/auth/ResetPasswordPage'
import { DashboardPage } from '@/pages/dashboard/DashboardPage'
import { ProjectsPage } from '@/pages/projects/ProjectsPage'
import { ProjectDetailPage } from '@/pages/projects/ProjectDetailPage'
import { ClientsPage } from '@/pages/clients/ClientsPage'
import { ClientDetailPage } from '@/pages/clients/ClientDetailPage'
import { ContractsPage } from '@/pages/contracts/ContractsPage'
import { TendersPage } from '@/pages/tenders/TendersPage'
import { SiteReportsPage } from '@/pages/site-reports/SiteReportsPage'
import { AddSiteReportPage } from '@/pages/site-reports/AddSiteReportPage'
import { SiteReportDetailPage } from '@/pages/site-reports/SiteReportDetailPage'
import { SchedulePage } from '@/pages/schedule/SchedulePage'
import { GanttPage } from '@/pages/schedule/GanttPage'
import { FleetPage } from '@/pages/fleet/FleetPage'
import { VehicleDetailPage } from '@/pages/fleet/VehicleDetailPage'
import { EquipmentPage } from '@/pages/equipment/EquipmentPage'
import { EquipmentDetailPage } from '@/pages/equipment/EquipmentDetailPage'
import { SuppliersPage } from '@/pages/suppliers/SuppliersPage'
import { SupplierDetailPage } from '@/pages/suppliers/SupplierDetailPage'
import { ProcurementPage } from '@/pages/procurement/ProcurementPage'
import { PurchaseOrderDetailPage } from '@/pages/procurement/PurchaseOrderDetailPage'
import { StoresPage } from '@/pages/stores/StoresPage'
import { StockTransactionsPage } from '@/pages/stores/StockTransactionsPage'
import { BoqPage } from '@/pages/boq/BoqPage'
import { ReportsPage } from '@/pages/reports/ReportsPage'
import { SettingsPage } from '@/pages/settings/SettingsPage'
import { ProfilePage } from '@/pages/profile/ProfilePage'
import { DocumentsPage } from '@/pages/documents/DocumentsPage'
import { EmployeesPage } from '@/pages/employees/EmployeesPage'
import { EmployeeDetailPage } from '@/pages/employees/EmployeeDetailPage'
import { AddEmployeePage } from '@/pages/employees/AddEmployeePage'
import { EditEmployeePage } from '@/pages/employees/EditEmployeePage'
import { AuditLogsPage } from '@/pages/admin/AuditLogsPage'
import { UsersPage } from '@/pages/admin/UsersPage'
import { NotificationsPage } from '@/pages/notifications/NotificationsPage'
import { AddDocumentPage } from '@/pages/documents/AddDocumentPage'
import { AddClientPage } from '@/pages/clients/AddClientPage'
import { AddProjectPage } from '@/pages/projects/AddProjectPage'
import { AddSupplierPage } from '@/pages/suppliers/AddSupplierPage'
import { AddPurchaseOrderPage } from '@/pages/procurement/AddPurchaseOrderPage'
import { AddTenderPage } from '@/pages/tenders/AddTenderPage'
import { AddContractPage } from '@/pages/contracts/AddContractPage'
import { AddVehiclePage } from '@/pages/fleet/AddVehiclePage'
import { AddEquipmentPage } from '@/pages/equipment/AddEquipmentPage'
import { EditClientPage } from '@/pages/clients/EditClientPage'
import { EditProjectPage } from '@/pages/projects/EditProjectPage'
import { EditSupplierPage } from '@/pages/suppliers/EditSupplierPage'
import { EditSiteReportPage } from '@/pages/site-reports/EditSiteReportPage'
import { EditVehiclePage } from '@/pages/fleet/EditVehiclePage'
import { EditEquipmentPage } from '@/pages/equipment/EditEquipmentPage'
import { EditTenderPage } from '@/pages/tenders/EditTenderPage'
import { EditContractPage } from '@/pages/contracts/EditContractPage'
import { EditPurchaseOrderPage } from '@/pages/procurement/EditPurchaseOrderPage'
import { TenderDetailPage } from '@/pages/tenders/TenderDetailPage'
import { ContractDetailPage } from '@/pages/contracts/ContractDetailPage'
import { AddInventoryItemPage } from '@/pages/stores/AddInventoryItemPage'
import { AddStockTransactionPage } from '@/pages/stores/AddStockTransactionPage'
import { EditTimesheetPage } from '@/pages/timesheets/EditTimesheetPage'
import { InventoryItemDetailPage } from '@/pages/stores/InventoryItemDetailPage'
import { EditInventoryItemPage } from '@/pages/stores/EditInventoryItemPage'
import { AddTaskPage } from '@/pages/schedule/AddTaskPage'
import { MessagesPage } from '@/pages/messages/MessagesPage'
import { TimesheetsPage } from '@/pages/timesheets/TimesheetsPage'
import { AddTimesheetPage } from '@/pages/timesheets/AddTimesheetPage'
import { AddBoqItemPage } from '@/pages/boq/AddBoqItemPage'
import { AddClaimPage } from '@/pages/boq/AddClaimPage'
import { RoleRoute } from '@/components/auth/RoleRoute'

function ProtectedRoute({ children }: { children: React.ReactNode }) {
  const isAuthenticated = useAuthStore((s) => s.isAuthenticated)
  if (!isAuthenticated) return <Navigate to="/login" replace />
  return <>{children}</>
}

export default function App() {
  return (
    <Routes>
      <Route path="/login" element={<LoginPage />} />
      <Route path="/forgot-password" element={<ForgotPasswordPage />} />
      <Route path="/reset-password" element={<ResetPasswordPage />} />
      <Route
        path="/*"
        element={
          <ProtectedRoute>
            <AppShell>
              <RoleRoute>
              <Routes>
                <Route path="/" element={<DashboardPage />} />
                <Route path="/projects" element={<ProjectsPage />} />
                <Route path="/projects/new" element={<AddProjectPage />} />
                <Route path="/projects/:id/edit" element={<EditProjectPage />} />
                <Route path="/projects/:id" element={<ProjectDetailPage />} />
                <Route path="/clients" element={<ClientsPage />} />
                <Route path="/clients/new" element={<AddClientPage />} />
                <Route path="/clients/:id/edit" element={<EditClientPage />} />
                <Route path="/clients/:id" element={<ClientDetailPage />} />
                <Route path="/contracts" element={<ContractsPage />} />
                <Route path="/contracts/new" element={<AddContractPage />} />
                <Route path="/contracts/:id/edit" element={<EditContractPage />} />
                <Route path="/contracts/:id" element={<ContractDetailPage />} />
                <Route path="/tenders" element={<TendersPage />} />
                <Route path="/tenders/new" element={<AddTenderPage />} />
                <Route path="/tenders/:id/edit" element={<EditTenderPage />} />
                <Route path="/tenders/:id" element={<TenderDetailPage />} />
                <Route path="/site-reports" element={<SiteReportsPage />} />
                <Route path="/site-reports/new" element={<AddSiteReportPage />} />
                <Route path="/site-reports/:id/edit" element={<EditSiteReportPage />} />
                <Route path="/site-reports/:id" element={<SiteReportDetailPage />} />
                <Route path="/schedule" element={<SchedulePage />} />
                <Route path="/schedule/gantt" element={<GanttPage />} />
                <Route path="/schedule/new-task" element={<AddTaskPage />} />
                <Route path="/fleet" element={<FleetPage />} />
                <Route path="/fleet/new" element={<AddVehiclePage />} />
                <Route path="/fleet/:id/edit" element={<EditVehiclePage />} />
                <Route path="/fleet/:id" element={<VehicleDetailPage />} />
                <Route path="/equipment" element={<EquipmentPage />} />
                <Route path="/equipment/new" element={<AddEquipmentPage />} />
                <Route path="/equipment/:id/edit" element={<EditEquipmentPage />} />
                <Route path="/equipment/:id" element={<EquipmentDetailPage />} />
                <Route path="/procurement" element={<ProcurementPage />} />
                <Route path="/procurement/new" element={<AddPurchaseOrderPage />} />
                <Route path="/procurement/:id/edit" element={<EditPurchaseOrderPage />} />
                <Route path="/procurement/:id" element={<PurchaseOrderDetailPage />} />
                <Route path="/suppliers" element={<SuppliersPage />} />
                <Route path="/suppliers/new" element={<AddSupplierPage />} />
                <Route path="/suppliers/:id/edit" element={<EditSupplierPage />} />
                <Route path="/suppliers/:id" element={<SupplierDetailPage />} />
                <Route path="/stores" element={<StoresPage />} />
                <Route path="/stores/new" element={<AddInventoryItemPage />} />
                <Route path="/stores/transactions/new" element={<AddStockTransactionPage />} />
                <Route path="/stores/transactions" element={<StockTransactionsPage />} />
                <Route path="/stores/:id/edit" element={<EditInventoryItemPage />} />
                <Route path="/stores/:id" element={<InventoryItemDetailPage />} />
                <Route path="/boq" element={<BoqPage />} />
                <Route path="/boq/new" element={<AddBoqItemPage />} />
                <Route path="/boq/claims/new" element={<AddClaimPage />} />
                <Route path="/reports" element={<ReportsPage />} />
                <Route path="/profile" element={<ProfilePage />} />
                <Route path="/settings" element={<SettingsPage />} />
                <Route path="/documents" element={<DocumentsPage />} />
                <Route path="/documents/new" element={<AddDocumentPage />} />
                <Route path="/notifications" element={<NotificationsPage />} />
                <Route path="/employees" element={<EmployeesPage />} />
                <Route path="/employees/new" element={<AddEmployeePage />} />
                <Route path="/employees/:id/edit" element={<EditEmployeePage />} />
                <Route path="/employees/:id" element={<EmployeeDetailPage />} />
                <Route path="/timesheets" element={<TimesheetsPage />} />
                <Route path="/timesheets/new" element={<AddTimesheetPage />} />
                <Route path="/timesheets/:id/edit" element={<EditTimesheetPage />} />
                <Route path="/messages" element={<MessagesPage />} />
                <Route path="/admin/audit" element={<AuditLogsPage />} />
                <Route path="/admin/users" element={<UsersPage />} />
                <Route path="*" element={<Navigate to="/" replace />} />
              </Routes>
              </RoleRoute>
            </AppShell>
          </ProtectedRoute>
        }
      />
    </Routes>
  )
}
