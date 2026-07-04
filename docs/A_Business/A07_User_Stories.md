# A07 – User Stories & Use Cases

| Field | Value |
|-------|-------|
| **Document ID** | A07 |
| **Version** | 1.0 |
| **Status** | Approved |

---

## Epic 1: Authentication & Access

### US-001: Login
**As a** TopZinto employee  
**I want to** log in with my email and password  
**So that** I can access the system securely  

**Acceptance:** Valid credentials → dashboard; invalid → error; lockout after 5 failures.

### US-002: Role-based dashboard
**As a** Managing Director  
**I want to** see portfolio KPIs on login  
**So that** I have immediate operational visibility  

**Acceptance:** Dashboard shows active projects, tenders, contracts value, fleet utilization.

---

## Epic 2: Projects

### US-010: Create project
**As a** Project Manager  
**I want to** create a new project with client, dates, and contract value  
**So that** we can track delivery centrally  

### US-011: Project hub
**As a** Project Manager  
**I want to** view all project information in one place (tabs)  
**So that** I don't switch between spreadsheets and folders  

### US-012: Track progress
**As a** Operations Manager  
**I want to** see progress bars across all projects  
**So that** I can identify delayed projects quickly  

---

## Epic 3: Tenders

### US-020: Tender pipeline
**As an** Estimator  
**I want to** track tenders through stages with closing dates  
**So that** we never miss a submission deadline  

### US-021: Tender to project
**As a** Contract Manager  
**I want to** convert a won tender into a contract and project  
**So that** handover is seamless  

---

## Epic 4: Site Operations

### US-030: Daily site report
**As a** Supervisor  
**I want to** submit a daily site report with photos from site  
**So that** management has real-time field visibility  

### US-031: Programme Gantt
**As a** Project Manager  
**I want to** view and update the project programme on a Gantt chart  
**So that** I can manage milestones and dependencies  

---

## Epic 5: Fleet & Equipment

### US-040: Fleet compliance
**As a** Fleet Manager  
**I want to** see vehicles with expiring licences and insurance  
**So that** we remain roadworthy and compliant  

### US-041: Equipment booking
**As a** Site Supervisor  
**I want to** book equipment for my project dates  
**So that** plant is available when needed  

---

## Epic 6: Procurement

### US-050: Create PO
**As a** Procurement Officer  
**I want to** create a purchase order linked to a project  
**So that** costs are tracked against budget  

### US-051: Stock issue
**As a** Store Controller  
**I want to** issue materials from store to a project  
**So that** usage is recorded and stock updated  

---

## Epic 7: Documents

### US-060: Upload with expiry
**As an** Admin user  
**I want to** upload a document with an expiry date  
**So that** the system alerts me before it expires  

### US-061: Document search
**As a** Project Manager  
**I want to** search documents across projects  
**So that** I find contracts and drawings quickly  

---

## Epic 8: Reporting

### US-070: Export project report
**As a** Director  
**I want to** export a project status report to PDF  
**So that** I can share with stakeholders offline  

---

## Use Case: Submit Site Report

| Step | Actor | Action |
|------|-------|--------|
| 1 | Supervisor | Opens Site Reports → Add New |
| 2 | Supervisor | Selects project, date, enters weather and work done |
| 3 | Supervisor | Attaches photos |
| 4 | Supervisor | Saves draft or submits |
| 5 | System | Creates audit log; notifies PM if submitted |
| 6 | PM | Reviews on dashboard "Latest Site Reports" |

---

## References

- [A04 – Functional Requirements](./A04_Functional_Requirements.md)
- [D01 – UI Screen Inventory](../D_UI_UX/D01_Screen_Inventory.md)

---

*End of Document A07*
