# A03 – Project Scope Document

| Field | Value |
|-------|-------|
| **Document ID** | A03 |
| **Version** | 1.0 |
| **Status** | Approved |

---

## 1. Purpose

Define what is included and excluded from TE-ERP v1.0 for TopZinto CC internal operations.

---

## 2. In Scope (v1.0)

### Phase 0 – Foundation
- [ ] Authentication (login, logout, password reset, session timeout, lockout)
- [ ] RBAC with permission matrix
- [ ] Audit logs (immutable)
- [ ] App shell (sidebar, topbar, search, notifications, profile)
- [ ] Document service core (upload, version, category, expiry)
- [ ] Notification framework
- [ ] System settings (company profile, branding)

### Phase 1 – Business Spine
- [ ] Clients (master data + contacts)
- [ ] Projects (register + detail hub)
- [ ] Contracts (linked to projects)
- [ ] Tenders (pipeline + deadlines + compliance checklist)

### Phase 2 – Site Operations
- [ ] Site reports (form, photos, sign-off)
- [ ] Scheduling (calendar + Gantt)
- [ ] Tasks & milestones

### Phase 3 – Assets
- [ ] Fleet (vehicles, drivers, maintenance, fuel log)
- [ ] Equipment (register, bookings, inspections, service history)

### Phase 4 – Spend & Stock
- [ ] Suppliers
- [ ] Procurement (RFQ → PO → delivery)
- [ ] Stores / inventory (stock in/out, issue to project)

### Phase 5 – Financial & Platform
- [ ] BOQ & cost tracking
- [ ] Claims & invoices register
- [ ] Reports (PDF/Excel/CSV)
- [ ] Backup administration
- [ ] Dashboard (role-based KPIs)

---

## 3. Out of Scope (v1.0)

| Item | Reason | Future Phase |
|------|--------|--------------|
| Payroll processing | Requires labour law complexity | v2 |
| Full accounting ledger | Sage/Pastel integration planned | v1.2 |
| Tax management | Accounting system domain | v2 |
| Banking integration | Security & compliance scope | v2 |
| Public client portal | Clients won't use system | N/A |
| Supplier portal | Internal procurement first | v1.2 |
| Offline mobile sync | Online-first v1 | v1.2 |
| AI predictions | Foundation first | v2 |
| GPS / IoT / Drones | Integration hooks only | v2 |
| BIM / GIS | Specialized tools | v2 |
| Multi-tenant / licensing | TopZinto internal only | v2 |

---

## 4. Deliverables

| Deliverable | Description |
|-------------|-------------|
| Web Application | Responsive SPA for TopZinto staff |
| REST API | OpenAPI-documented backend |
| Database | PostgreSQL with migrations |
| Documentation | This docs suite + API docs |
| Deployment | Docker Compose for dev/staging |

---

## 5. Constraints

- Single company (TopZinto CC)
- Internal users only
- South African Rand (ZAR) currency
- English UI

---

## 6. Acceptance Criteria (v1.0)

1. All in-scope modules functional end-to-end
2. RBAC enforced on every API endpoint
3. Audit log captures critical mutations
4. Documents support versioning and expiry alerts
5. Dashboard shows accurate KPIs for Director role
6. Reports export to PDF and Excel
7. Daily automated database backup with admin restore UI
8. System usable on desktop and tablet

---

## References

- [A01 – Executive Summary](./A01_Executive_Summary.md)
- [A04 – Functional Requirements](./A04_Functional_Requirements.md)
- [BLUEPRINT – Release Plan](../BLUEPRINT.md)

---

*End of Document A03*
