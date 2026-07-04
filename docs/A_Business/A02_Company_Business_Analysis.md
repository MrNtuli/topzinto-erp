# A02 – Company Business Analysis

| Field | Value |
|-------|-------|
| **Document ID** | A02 |
| **Version** | 1.0 |
| **Status** | Approved |

---

## 1. Purpose

Analyze TopZinto CC's business context, departments, workflows, and construction lifecycle to inform TE-ERP design.

---

## 2. Company Profile

**TopZinto CC** is a South African construction company operating across:

- Government projects
- Municipal projects
- Infrastructure & civil engineering
- Building construction & housing
- Road & water infrastructure
- Maintenance contracts & renovations

**Operating model:** Contractor executing projects for large client entities (government departments, municipalities, SOEs, private developers). Clients do **not** use TopZinto's internal systems.

---

## 3. Department Analysis

| Department | Core Functions | TE-ERP Modules |
|------------|----------------|----------------|
| **Executive** | Strategy, approvals, portfolio oversight | Dashboard, Reports |
| **Operations** | Resource allocation, programme coordination | Projects, Schedule, Fleet, Equipment |
| **Projects** | Delivery, site management, progress | Projects, Site Reports, Tasks |
| **Contracts** | Contract admin, claims, variations | Contracts, Financial Tracking |
| **Estimating / QS** | Tenders, BOQ, costing | Tenders, BOQ & Costing |
| **Procurement** | Suppliers, POs, deliveries | Procurement, Suppliers, Stores |
| **Finance** | Cost control, invoicing, retention | Financial Tracking, Reports |
| **HR** | Staff records, assignments | Employees |
| **Fleet** | Vehicles, drivers, compliance | Fleet |
| **Plant** | Equipment, maintenance, bookings | Equipment |
| **H&S** | Incidents, inspections, compliance | Health & Safety, Compliance |
| **Admin** | Documents, reception, settings | Documents, Administration |

---

## 4. Construction Project Lifecycle

```
Tender Opportunity → Bid Preparation → Submission → Award/Loss
       ↓ (if awarded)
Contract Execution → Mobilization → Construction Phases → Practical Completion
       ↓
Defects Liability → Final Account → Close-out
```

**TE-ERP support per phase:**

| Phase | System Support |
|-------|----------------|
| Tender | Tender register, deadlines, compliance docs, BOQ draft |
| Award | Contract creation, link to project |
| Mobilization | Team assignment, fleet/equipment allocation, site setup |
| Execution | Schedule, site reports, procurement, cost tracking |
| Monitoring | Dashboards, progress, claims, H&S |
| Close-out | Document archive, final costs, retention tracking |

---

## 5. Key Business Processes

### 5.1 Tender Management
1. Identify opportunity → capture in system
2. Assign estimator → prepare BOQ and costing
3. Compile compliance documents (CIDB, tax, BEE certs)
4. Submit before deadline
5. Track outcome → convert to contract/project if won

### 5.2 Project Execution
1. Create project from contract
2. Assign PM, team, resources
3. Load programme (Gantt)
4. Daily site reports from supervisors
5. Track costs against BOQ/budget
6. Submit claims → track payment

### 5.3 Procurement
1. Material request from site/project
2. RFQ to suppliers → compare quotes
3. PO issued → delivery tracked
4. Goods received → stock updated → cost to project

### 5.4 Fleet & Equipment
1. Register assets with compliance docs
2. Schedule maintenance and inspections
3. Book/assign to projects
4. Alert on licence/insurance/service expiry

### 5.5 Document Control
1. Upload with category and parent link
2. Version on revision
3. Set expiry (permits, insurance, certs)
4. Internal approval workflow
5. Export for client submission (outside system)

---

## 6. Pain Points (Current State)

| Pain Point | TE-ERP Solution |
|------------|-----------------|
| Spreadsheet project tracking | Centralized project hub |
| Missed tender deadlines | Tender calendar + notifications |
| Lost documents | Document management with search |
| Unknown fleet status | Fleet dashboard + assignments |
| Manual cost consolidation | Financial tracking linked to BOQ/POs |
| No audit trail | Immutable audit logs |
| Delayed site reporting | Digital site report forms |

---

## 7. Document Flow

```
External (Email/Paper) → Internal Capture in TE-ERP → Process/Approve → Export PDF for Client
```

All client-facing submissions happen **outside** the system; TE-ERP tracks status internally.

---

## 8. Assumptions

- TopZinto staff have internet access (office and site with connectivity)
- v1.0 is single-company (TopZinto only)
- Accounting remains in Sage/Pastel; TE-ERP tracks costs and exports data
- English is the primary UI language

---

## References

- [A01 – Executive Summary](./A01_Executive_Summary.md)
- [A04 – Functional Requirements](./A04_Functional_Requirements.md)
- [A07 – User Stories](./A07_User_Stories.md)

---

*End of Document A02*
