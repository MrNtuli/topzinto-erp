# A04 – Functional Requirements Specification (FRS)

| Field | Value |
|-------|-------|
| **Document ID** | A04 |
| **Version** | 1.0 |
| **Status** | Approved |

---

## 1. Authentication & Users

| ID | Requirement | Priority |
|----|-------------|----------|
| AUTH-01 | Users authenticate with email and password | Must |
| AUTH-02 | Passwords hashed with bcrypt/Argon2 | Must |
| AUTH-03 | Session timeout after configurable inactivity | Must |
| AUTH-04 | Account lockout after failed attempts | Must |
| AUTH-05 | Secure password reset via email token | Must |
| AUTH-06 | Super Admin manages users | Must |
| AUTH-07 | Each user has one primary role + optional permissions | Must |

---

## 2. Role-Based Access Control

| ID | Requirement | Priority |
|----|-------------|----------|
| RBAC-01 | 20+ predefined roles with permission matrix | Must |
| RBAC-02 | Permissions: module.view, module.create, module.edit, module.delete, module.approve | Must |
| RBAC-03 | Project-scoped access for site roles | Should |
| RBAC-04 | Role-specific dashboard KPIs | Must |

---

## 3. Clients (Master Data)

| ID | Requirement | Priority |
|----|-------------|----------|
| CLI-01 | CRUD client organizations | Must |
| CLI-02 | Multiple contacts per client | Must |
| CLI-03 | Client project history view | Must |
| CLI-04 | No client login or portal | Must |

---

## 4. Projects

| ID | Requirement | Priority |
|----|-------------|----------|
| PRJ-01 | Project register with filters (status, client) | Must |
| PRJ-02 | Project hub tabs: Overview, Schedule, BOQ, Site Reports, Documents, Financial, Activity | Must |
| PRJ-03 | Status: Planned, Active, On Hold, Completed | Must |
| PRJ-04 | Progress percentage (manual + milestone-derived) | Must |
| PRJ-05 | Link client, contract, tender | Must |
| PRJ-06 | Assign team members | Must |
| PRJ-07 | Milestones and tasks with due dates | Must |
| PRJ-08 | Risk register and issues log | Should |
| PRJ-09 | Communication log (internal) | Should |

---

## 5. Tenders

| ID | Requirement | Priority |
|----|-------------|----------|
| TEN-01 | Tender pipeline with stages | Must |
| TEN-02 | Closing date with notifications | Must |
| TEN-03 | Compliance checklist (CIDB, tax, etc.) | Must |
| TEN-04 | Link documents and BOQ draft | Must |
| TEN-05 | Convert won tender to contract/project | Must |

---

## 6. Contracts

| ID | Requirement | Priority |
|----|-------------|----------|
| CON-01 | Contract register linked to projects | Must |
| CON-02 | Contract value, dates, retention % | Must |
| CON-03 | Key clauses metadata | Should |
| CON-04 | Contract documents | Must |

---

## 7. Documents

| ID | Requirement | Priority |
|----|-------------|----------|
| DOC-01 | Upload/download/preview | Must |
| DOC-02 | Versioning on re-upload | Must |
| DOC-03 | Categories and folder structure | Must |
| DOC-04 | Link to parent record (polymorphic) | Must |
| DOC-05 | Issue and expiry dates with alerts | Must |
| DOC-06 | Internal approval status | Must |
| DOC-07 | Full-text search | Should |

---

## 8. Site Reports

| ID | Requirement | Priority |
|----|-------------|----------|
| SR-01 | Daily site report form (weather, work done, issues, tomorrow plan) | Must |
| SR-02 | Photo attachments | Must |
| SR-03 | Link to project | Must |
| SR-04 | Submit and internal sign-off workflow | Must |
| SR-05 | List on dashboard "Latest Site Reports" | Must |

---

## 9. Fleet

| ID | Requirement | Priority |
|----|-------------|----------|
| FLT-01 | Vehicle register (type, reg, driver) | Must |
| FLT-02 | Status: Available, In Use, Maintenance | Must |
| FLT-03 | Licence and insurance expiry tracking | Must |
| FLT-04 | Maintenance schedule and log | Must |
| FLT-05 | Fuel log (basic) | Should |
| FLT-06 | Assign to project | Must |

---

## 10. Equipment

| ID | Requirement | Priority |
|----|-------------|----------|
| EQP-01 | Asset register by category | Must |
| EQP-02 | Bookings and availability calendar | Must |
| EQP-03 | Service history and inspections | Must |
| EQP-04 | Assign operator | Should |
| EQP-05 | Link documents (manuals, certs) | Must |

---

## 11. Procurement & Stores

| ID | Requirement | Priority |
|----|-------------|----------|
| PRO-01 | Supplier register | Must |
| PRO-02 | RFQ creation and quote comparison | Must |
| PRO-03 | Purchase order with approval | Must |
| PRO-04 | Delivery tracking | Must |
| STO-01 | Inventory items and stock levels | Must |
| STO-02 | Stock in/out transactions | Must |
| STO-03 | Issue to project | Must |
| STO-04 | Low-stock alerts | Must |

---

## 12. Scheduling

| ID | Requirement | Priority |
|----|-------------|----------|
| SCH-01 | Calendar view (projects, maintenance, inspections) | Must |
| SCH-02 | Gantt per project | Must |
| SCH-03 | Cross-project schedule overview | Should |
| SCH-04 | Milestone markers on timeline | Must |

---

## 13. Financial Tracking

| ID | Requirement | Priority |
|----|-------------|----------|
| FIN-01 | BOQ line items with qty, rate, amount | Must |
| FIN-02 | Budget vs committed (POs) vs actual | Must |
| FIN-03 | Claims register with status | Must |
| FIN-04 | Invoices register | Must |
| FIN-05 | Retention tracking | Should |
| FIN-06 | Export for accounting (CSV) | Should |

---

## 14. Reporting & Notifications

| ID | Requirement | Priority |
|----|-------------|----------|
| RPT-01 | Project status, tender, fleet, document expiry reports | Must |
| RPT-02 | Export PDF, Excel, CSV | Must |
| NTF-01 | In-app notification center | Must |
| NTF-02 | Alerts: tender closing, doc expiry, overdue tasks, service due | Must |

---

## 15. Audit & Administration

| ID | Requirement | Priority |
|----|-------------|----------|
| AUD-01 | Immutable audit log (user, IP, old/new values) | Must |
| ADM-01 | Company settings and branding | Must |
| ADM-02 | Backup status and manual trigger | Must |

---

## References

- [A05 – Non-Functional Requirements](./A05_Non_Functional_Requirements.md)
- [A06 – Business Rules](./A06_Business_Rules.md)
- [C01 – Database Schema](../C_Database/C01_Database_Schema.md)

---

*End of Document A04*
