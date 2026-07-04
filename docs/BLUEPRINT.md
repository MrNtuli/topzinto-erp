# TE-ERP Blueprint Pack

**TopZinto Enterprise Construction ERP** — Master reference for development.

| Version | 1.0 |
|---------|-----|
| Status | Active |
| Audience | TopZinto CC internal operations |

---

## 1. Product Summary

Single integrated platform for TopZinto construction operations. **Internal users only.** Clients are master data, not system users.

---

## 2. Tech Stack (Locked)

| Component | Choice |
|-----------|--------|
| API | .NET 8 / ASP.NET Core |
| DB | PostgreSQL 16 |
| Cache | Redis |
| Frontend | React 18 + TypeScript + Vite |
| Storage | MinIO (S3-compatible) |
| Deploy | Docker Compose |

---

## 3. Release Plan

### Phase 0 — Foundation (Weeks 1–4)
Auth, RBAC, audit, app shell, document core, notifications framework, CI/CD, Docker

### Phase 1 — Business Spine (Weeks 5–12)
Clients → Projects → Contracts → Tenders

### Phase 2 — Site Operations (Weeks 13–18)
Site reports, scheduling, tasks/milestones

### Phase 3 — Assets (Weeks 19–26)
Fleet, equipment

### Phase 4 — Spend & Stock (Weeks 27–34)
Suppliers, procurement, stores

### Phase 5 — Financial & Platform (Weeks 35–40)
BOQ/cost tracking, claims, reports, backups admin

---

## 4. Role & Permission Matrix (Summary)

| Module | Director | PM | Contract Mgr | QS | Procurement | Fleet Mgr | Supervisor | Finance | Admin |
|--------|----------|-----|--------------|-----|-------------|-----------|------------|---------|-------|
| Dashboard | Full | Project | Project | View | View | Fleet | Site | Financial | Full |
| Projects | Full | Full | Edit | View | View | View | Edit* | View | Full |
| Tenders | View | View | Full | Full | — | — | — | View | Full |
| Contracts | View | View | Full | View | — | — | — | View | Full |
| Fleet | View | View | — | — | — | Full | View | — | Full |
| Procurement | View | Approve* | — | — | Full | — | Request | View | Full |
| Site Reports | View | View | View | — | — | — | Full | — | Full |
| Admin | — | — | — | — | — | — | — | — | Full |

*Project-scoped or approval within threshold

**20 roles** seeded; matrix expanded in application config.

---

## 5. v1 Module Checklist

- [x] Documentation (Section A, B, C, D)
- [ ] Phase 0 code
- [ ] Phase 1 code
- [ ] Phase 2 code
- [ ] Phase 3 code
- [ ] Phase 4 code
- [ ] Phase 5 code

---

## 6. Document Index

```
docs/
├── BLUEPRINT.md                    ← You are here
├── A_Business/
│   ├── A01_Executive_Summary.md
│   ├── A02_Company_Business_Analysis.md
│   ├── A03_Project_Scope.md
│   ├── A04_Functional_Requirements.md
│   ├── A05_Non_Functional_Requirements.md
│   ├── A06_Business_Rules.md
│   ├── A07_User_Stories.md
│   └── A08_Risk_Assessment.md
├── B_Architecture/
│   └── B01_System_Architecture.md
├── C_Database/
│   └── C01_Database_Schema.md
└── D_UI_UX/
    └── D01_Screen_Inventory.md
```

---

## 7. Next Build Target: Phase 0

1. Solution scaffold (`Topzinto.Erp.sln`)
2. Domain entities: User, Role, Permission, AuditLog
3. Identity + JWT auth endpoints
4. React app shell matching mockup inspiration
5. Login page + protected dashboard route
6. Docker Compose: API + PostgreSQL + Redis

---

*End of Blueprint*
