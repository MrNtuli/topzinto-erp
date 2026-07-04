# C01 – Database Schema Overview

| Field | Value |
|-------|-------|
| **Document ID** | C01 |
| **Version** | 1.0 |

---

## 1. Conventions

- **PK:** `Id` (UUID or BIGINT identity — UUID for distributed-ready)
- **Audit columns:** `CreatedAt`, `CreatedBy`, `UpdatedAt`, `UpdatedBy`, `IsDeleted`
- **Tenant:** `TenantId` (default TopZinto single tenant in v1)
- **Naming:** PascalCase tables, snake_case PostgreSQL columns via EF config

---

## 2. Core Entities

### Identity & Security
```
Users
Roles
UserRoles
Permissions
RolePermissions
RefreshTokens
AuditLogs
```

### Organization
```
CompanySettings
Clients
ClientContacts
```

### Projects (Hub)
```
Projects
ProjectMembers
ProjectMilestones
ProjectTasks
ProjectRisks
ProjectIssues
ProjectCommunications
```

### Commercial
```
Tenders
TenderComplianceItems
Contracts
BoqItems
Claims
Invoices
```

### Operations
```
SiteReports
SiteReportPhotos
Schedules
ScheduleItems
```

### Fleet
```
Vehicles
VehicleAssignments
VehicleMaintenance
FuelLogs
Drivers
```

### Equipment
```
Equipment
EquipmentBookings
EquipmentInspections
EquipmentMaintenance
```

### Procurement
```
Suppliers
PurchaseRequisitions
PurchaseOrders
PurchaseOrderLines
Deliveries
InventoryItems
InventoryTransactions
```

### Documents (Polymorphic)
```
Documents
DocumentVersions
DocumentCategories
DocumentApprovals
```

### Notifications
```
Notifications
NotificationPreferences
```

---

## 3. Key Relationships

```
Client 1──* Project
Tender 0..1──1 Contract
Contract 1──1 Project
Project 1──* SiteReport
Project 1──* BoqItem
Project 1──* ProjectTask
Project *──* User (via ProjectMembers)
Vehicle *──* Project (via VehicleAssignments)
Equipment *──* Project (via EquipmentBookings)
Document *──1 (any parent via ParentType + ParentId)
PurchaseOrder *──1 Project
PurchaseOrder *──1 Supplier
```

---

## 4. Indexes (Critical)

| Table | Index |
|-------|-------|
| Projects | ClientId, Status, Code (unique) |
| Documents | ParentType+ParentId, ExpiryDate |
| AuditLogs | UserId, CreatedAt, EntityType+EntityId |
| Vehicles | Status, LicenceExpiryDate |
| Tenders | ClosingDate, Status |
| Notifications | UserId, IsRead, CreatedAt |

---

## 5. Enums

```
ProjectStatus: Planned, Active, OnHold, Completed
TenderStatus: Identified, Preparing, Submitted, Won, Lost
ContractStatus: Draft, Active, Completed, Terminated
VehicleStatus: Available, InUse, Maintenance, Decommissioned
DocumentStatus: Draft, PendingApproval, Approved, Rejected, Expired
PoStatus: Draft, PendingApproval, Approved, Ordered, Delivered, Cancelled
```

---

## References

- [A04 – Functional Requirements](../A_Business/A04_Functional_Requirements.md)
- [B01 – System Architecture](../B_Architecture/B01_System_Architecture.md)

---

*End of Document C01*
