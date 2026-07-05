# TopZinto Enterprise Construction ERP (TE-ERP)

[![CI](https://github.com/MrNtuli/topzinto-erp/actions/workflows/ci.yml/badge.svg)](https://github.com/MrNtuli/topzinto-erp/actions/workflows/ci.yml)

Internal operations platform for **TopZinto CC**.

## Project Structure

```
Topzinto System/
├── docs/                    # Enterprise documentation (A01–A08, Blueprint)
├── src/
│   ├── Topzinto.Erp.Api/           # ASP.NET Core 8 Web API
│   ├── Topzinto.Erp.Application/   # Services & DTOs
│   ├── Topzinto.Erp.Domain/        # Entities & business rules
│   ├── Topzinto.Erp.Infrastructure/ # EF Core, Identity, persistence
│   └── Topzinto.Erp.Web/           # React + TypeScript + Vite
├── docker-compose.yml
└── Topzinto.Erp.sln
```

## Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Node.js 20+](https://nodejs.org/)
- [Docker Desktop](https://www.docker.com/products/docker-desktop/) (optional — for PostgreSQL/MinIO; SQLite works without Docker)

## Quick Start

**Fastest (any OS with Node):** from repo root, run `npm run dev:all` — launches the platform startup script below.

**Windows:** `.\scripts\start-dev.ps1` — opens API + Web in two terminals and polls `/api/health`.

**Linux/macOS:** `./scripts/start-dev.sh`

### 1. Start database

```bash
docker compose up postgres redis minio -d
```

MinIO console: http://localhost:9001 (user `topzinto` / `topzinto_dev`)

### 2. Run API

```bash
cd src/Topzinto.Erp.Api
dotnet run
```

Uses **SQLite** in development (`topzinto_erp.db` created automatically). For PostgreSQL, set `UseSqlite: false` in `appsettings.Development.json` and start Docker Postgres.

API: http://localhost:5000  
Swagger: http://localhost:5000/swagger

### 3. Run frontend

```bash
cd src/Topzinto.Erp.Web
npm install
npm run dev
```

Web: http://localhost:5173

### Default login

| Email | Password | Role |
|-------|----------|------|
| admin@topzinto.com | Topzinto@2024 | Administrator |
| pm@topzinto.com | Topzinto@2024 | Project Manager |
| foreman@topzinto.com | Topzinto@2024 | Foreman |
| procurement@topzinto.com | Topzinto@2024 | Procurement |

## Documentation

See [`docs/BLUEPRINT.md`](docs/BLUEPRINT.md) for the master plan and module build order.

**GitHub & CI:** [`docs/GITHUB.md`](docs/GITHUB.md) — init repo, push, enable Actions.

## Phase Status

- [x] **Phase 0** — Auth, RBAC, audit, app shell, SQLite dev DB
- [x] **Phase 1** — Clients, Projects, Contracts, Tenders (API + UI)
- [x] **Phase 2** — Site reports, scheduling, tasks & milestones
- [x] **Phase 3** — Fleet, equipment
- [x] **Phase 4** — Procurement, suppliers, stores
- [x] **Phase 5** — BOQ/cost tracking, claims, reports, live dashboard KPIs

### v1.1 polish
- [x] Project hub tabs (Schedule, BOQ, Site Reports, Financial, Documents)
- [x] Site report detail page
- [x] Settings (profile, audit preview, SQLite backup)
- [x] Document metadata register with expiry tracking
- [x] CSV exports (projects, BOQ, claims)
- [x] Admin audit log viewer
- [x] Documents & Employees placeholders resolved (Employees → v2 HR)

### v1.2
- [x] In-app notifications center (auto-alerts: tenders, docs, tasks, fleet)
- [x] Company settings (editable in Settings)
- [x] Document registration form

### v1.3
- [x] Global search (projects, clients, suppliers, documents, POs, vehicles)
- [x] Document file upload/download (local storage in dev)
- [x] EF Core migrations (`dotnet ef database update`)
- [x] MinIO in docker-compose (optional — requires Docker Desktop)

**Upgrading from pre-v1.3:** delete `src/Topzinto.Erp.Api/topzinto_erp.db` and restart the API so migrations apply cleanly.

### v1.4
- [x] Dashboard financial trend chart (6-month claims vs procurement — live data)
- [x] Extended CSV exports (suppliers, procurement, invoices, fleet, documents)
- [x] Excel-friendly CSV (UTF-8 BOM)

### v1.5
- [x] Create forms: Clients, Projects, Suppliers, Purchase Orders
- [x] Line-item PO entry with add/remove rows

### v1.6
- [x] Create forms: Tenders, Contracts, Fleet vehicles, Equipment assets

### v1.7
- [x] Edit forms: Clients, Projects, Suppliers, Site Reports
- [x] Site report PDF export (QuestPDF, branded layout)

### v1.8 — v1 COMPLETE
- [x] Edit forms: Fleet vehicles, Equipment assets
- [x] Tender & Contract detail pages
- [x] Stores: add inventory item + stock transactions
- [x] Schedule: add project task form
- [x] True Excel export (Projects `.xlsx` via ClosedXML)

### v2.0
- [x] **HR module** — employee records (list, detail, create, edit), project assignment, 8 demo employees seeded
- [x] **RBAC in UI** — sidebar nav filtered by user role (field staff see limited modules)
- [x] **Global search** — employees included
- [x] **Production config** — `appsettings.Production.json` template for PostgreSQL

### v2.1
- [x] Edit forms: Tenders, Contracts, Purchase Orders (line items, approval fields)

### v2.2
- [x] **Timesheets** — log hours, project labour summary, labour cost (hours × rate)
- [x] **Create forms:** BOQ items, Progress claims
- [x] Sidebar: Timesheets module (HR, PM, site supervisors)

### v2.3
- [x] Edit timesheet entries (approve/adjust hours)
- [x] Inventory item detail + edit (qty via transactions only)
- [x] Project hub **Labour** tab + labour cost on Financial tab
- [x] CSV exports: Employees, Timesheets

### v2.4 — Internal Chat
- [x] **Team Chat** — department & project channels (`/messages`)
- [x] Real-time messaging via SignalR
- [x] Seeded channels: General, Site Ops, Procurement, Fleet, HR, project rooms
- [x] Message history stored in DB (audit-friendly)

### v2.5
- [x] **Excel exports** — Suppliers, Procurement (PO + line items sheet), Employees
- [x] **Route-level RBAC** — blocked URLs show Access Denied (same rules as sidebar)
- [x] **Settings fix** — Company settings service registered in DI

### v2.6 — Chat Unread
- [x] **Unread counts** — per-channel badges on Messages page
- [x] **Mark as read** — auto-mark when opening a channel
- [x] **Sidebar badge** — total unread on Messages nav item

### v2.7 — Chat Attachments
- [x] **File attachments** — paperclip button in chat (PDF, images, docs up to 10 MB)
- [x] **Download** — click attachment in message thread to download
- [x] **Real-time** — file messages broadcast via SignalR like text

### v2.8 — Chat @Mentions
- [x] **@mentions** — type `@` in compose to pick a user (autocomplete by name or email)
- [x] **Highlight** — mentioned names render orange/bold in message bubbles
- [x] **Notifications** — mentioned users receive in-app alerts (Category: Chat, link `/messages`)
- [x] **Demo users** — PM, Foreman, and Procurement accounts seeded for mention testing

### v2.9 — Direct Messages
- [x] **1:1 chat** — private DMs between any two team members
- [x] **New message** — `+` button in Direct Messages to start a conversation
- [x] **Access control** — only participants can read/send in a DM channel
- [x] **Same features** — attachments, @mentions, unread counts, and SignalR work in DMs

### v2.10 — Production Storage
- [x] **MinIO file storage** — swap `FileStorage:Provider` from `Local` to `Minio` for production/Docker
- [x] **Auto bucket** — creates `topzinto-documents` bucket on first upload
- [x] **PostgreSQL Docker** — `docker compose up` runs API against Postgres + MinIO
- [x] **Config** — `appsettings.Production.json` template with MinIO + Postgres settings

### v2.11 — Database Backups
- [x] **PostgreSQL backups** — `pg_dump` SQL export (Docker image includes client tools)
- [x] **SQLite backups** — file copy for local dev
- [x] **Scheduled backups** — daily auto backup when `Backup:Enabled` is true
- [x] **Retention** — keeps last N backups (default 7)
- [x] **Settings UI** — create, list, and download backups from Settings page

### v2.12 — Reverse Proxy Deploy
- [x] **Docker web container** — nginx serves React SPA on port 80
- [x] **API proxy** — `/api` and `/hubs` (SignalR WebSocket) forwarded to API
- [x] **Optional HTTPS** — Caddy with self-signed cert (`docker compose --profile ssl up`)
- [x] **Single entry point** — open http://localhost for full app in Docker

### v2.13 — Redis Caching
- [x] **Dashboard cache** — KPI summary cached 5 min (Redis in Docker, in-memory in dev)
- [x] **Reports hub cache** — report cards cached 5 min
- [x] **Health check** — `GET /api/health` shows cache status (`memory` / `redis`)
- [x] **Docker wired** — Redis container connected to API in compose

### v2.14 — CI/CD & Production Deploy
- [x] **GitHub Actions CI** — build API, web, and Docker images on push/PR
- [x] **Production compose overlay** — `docker-compose.prod.yml` (no public DB/Redis ports, Caddy on 80/443)
- [x] **Let's Encrypt** — `deploy/Caddyfile.production` template + full guide in [`docs/DEPLOY.md`](docs/DEPLOY.md)
- [x] **Env template** — `deploy/.env.production.example` for secrets and domain

### v2.15 — Smart Cache Invalidation
- [x] **Auto-invalidate** — dashboard & reports cache cleared when business data changes (projects, POs, claims, etc.)
- [x] **Skip noise** — chat, notifications, audit, and login activity do not bust the cache
- [x] **Manual clear** — `POST /api/admin/cache/invalidate` for admins

### v2.16 — Account Security
- [x] **Change password** — Settings page form with Identity validation rules
- [x] **Edit profile** — update first/last name; synced to header auth state
- [x] **Audit trail** — password and profile changes logged

### v2.17 — User Management
- [x] **Admin user list** — `/admin/users` with role, status, last login
- [x] **Create accounts** — email, name, role, initial password
- [x] **Edit & deactivate** — change role, toggle active (cannot deactivate self)
- [x] **Admin password reset** — set new password for any user
- [x] **API RBAC** — Director/SuperAdmin only; all actions audited

### v2.18 — Email Alerts & Session Security
- [x] **Email on @mentions** — optional SMTP; logs to console when disabled in dev
- [x] **SMTP config** — `Email:Enabled`, host, credentials in appsettings / Docker env
- [x] **Active session check** — deactivated users blocked immediately (not just at login)
- [x] **Health check** — `GET /api/health` shows `email: smtp` or `disabled`

### v2.19 — Session UX & Audit Export
- [x] **Auto logout** — 401 responses clear session and redirect to login with message
- [x] **Unified auth fetch** — exports, backups, and API calls share session handling
- [x] **Audit CSV export** — Directors export up to 1000 audit entries from `/admin/audit`

### v2.20 — Forgot Password
- [x] **Request reset** — `/forgot-password` page sends email (or shows dev link when SMTP off)
- [x] **Reset password** — `/reset-password?email=...&token=...` sets new password
- [x] **Email integration** — uses SMTP when enabled; dev link shown locally for testing
- [x] **Audit trail** — forgot/reset password actions logged

### v2.21 — System Alert Emails
- [x] **Scheduled scan** — background job every 6h checks tenders, docs, tasks, fleet compliance
- [x] **Role-based email** — Directors get all alerts; others get category-matched digests
- [x] **In-app + email** — new alerts still appear in Notifications centre
- [x] **Manual scan** — `POST /api/admin/alerts/scan` for Directors

### v2.22 — Site Report Photos
- [x] **Photo upload** — up to 5 images per site report (JPEG, PNG, WebP, max 10 MB)
- [x] **Gallery view** — photos displayed on site report detail page
- [x] **File storage** — uses same MinIO/local storage as documents and chat
- [x] **Migration** — `AddSiteReportPhotos` (delete dev DB and restart API if upgrading)

### v2.23 — API Module RBAC
- [x] **Role policies** — API endpoints enforce the same module access rules as the sidebar
- [x] **Field staff** — site workers limited to dashboard, site reports, schedule, documents, timesheets, chat
- [x] **Module matrix** — fleet, procurement, BOQ, reports, etc. restricted by role on the API layer
- [x] **403 responses** — unauthorized API calls return forbidden (not just hidden UI)

### v2.24 — Dashboard Refresh
- [x] **Refresh button** — busts server cache and reloads live KPIs and charts
- [x] **Last updated** — shows when dashboard data was last fetched
- [x] **Auto-refetch** — refreshes when returning to the tab (after 1 min stale)
- [x] **Time-aware greeting** — Good Morning / Afternoon / Evening

### v2.25 — Automated Tests
- [x] **Test project** — `tests/Topzinto.Erp.Tests` (xUnit)
- [x] **RBAC unit tests** — `ModuleRoleMatrix` matches sidebar role rules
- [x] **API integration tests** — health, auth, and dashboard smoke tests
- [x] **CI wired** — `dotnet test` runs on every push/PR

### v2.26 — Deploy Workflow & Go-Live
- [x] **GitHub deploy** — `.github/workflows/deploy.yml` (manual SSH deploy to VPS)
- [x] **Server script** — `deploy/update.sh` for on-server updates
- [x] **Go-live checklist** — [`docs/GO-LIVE.md`](docs/GO-LIVE.md)

### v2.27 — Session Idle Timeout
- [x] **Auto logout** — signed out after 30 minutes with no mouse/keyboard activity
- [x] **Warning banner** — amber alert 2 minutes before logout
- [x] **Login message** — explains idle timeout on redirect

### v2.28 — Git & GitHub Bootstrap
- [x] **Git-ready** — `.gitignore`, `.gitattributes` (secrets, DB, uploads excluded)
- [x] **Setup guide** — [`docs/GITHUB.md`](docs/GITHUB.md) (init, first commit, push, CI)
- [x] **Repo live** — https://github.com/MrNtuli/topzinto-erp

### v2.29 — Account Lockout
- [x] **Failed login limit** — account locked for 30 minutes after 5 wrong passwords
- [x] **Clear messaging** — login page shows lockout time remaining
- [x] **Auto reset** — lockout cleared on successful login, password change, or reset

### v2.30 — Admin Unlock Account
- [x] **Locked indicator** — user list shows Locked status after failed login attempts
- [x] **Director unlock** — `/admin/users` → Unlock button clears lockout immediately
- [x] **Audit trail** — unlock actions logged

### v2.31 — Production Security Polish
- [x] **Swagger disabled in production** — OpenAPI/SwaggerUI only registered in Development
- [x] **Login rate limiting** — 10 POST `/api/auth/login` attempts per minute per IP (HTTP 429)
- [x] **CI badge** — GitHub Actions build status in README

### v2.32 — Dev Experience & Remember Me
- [x] **One-command dev startup** — `npm run dev:all` from repo root, or `.\scripts\start-dev.ps1` (Windows) / `./scripts/start-dev.sh` (Linux/macOS); opens API + Web in separate terminals with health-check hints
- [x] **Remember me login** — checkbox sends `rememberMe` to API; JWT expires in **7 days** when checked, **8 hours** when unchecked
- [x] **Session storage** — unchecked uses `sessionStorage` (cleared when browser closes); checked uses `localStorage` for persistence

### v2.33 — Forgot Password Flow (Go-Live Ready)
- [x] **Request reset** — `POST /api/auth/forgot-password` always returns generic success (no email enumeration)
- [x] **Secure tokens** — ASP.NET Identity reset tokens (1-hour expiry); hashed server-side
- [x] **Reset password** — `POST /api/auth/reset-password` with strong password validation
- [x] **Rate limiting** — 5 forgot-password requests per minute per IP (HTTP 429)
- [x] **Audit trail** — forgot/reset requests logged with IP and user agent
- [x] **UI** — `/forgot-password` and `/reset-password?email=...&token=...` with loading/success/error states
- [x] **Dev mode** — reset link returned in API response and logged when SMTP is disabled
- [x] **Integration tests** — full forgot/reset flow, rate limit, and validation coverage

### v2.34 — Change Password (Authenticated User)
- [x] **API** — `POST /api/auth/change-password` (requires JWT; `currentPassword`, `newPassword`)
- [x] **Validation** — verifies current password; enforces strong password policy (8+ chars, upper, lower, digit, special)
- [x] **Audit trail** — password changes logged with IP and user agent
- [x] **UI** — Settings → **Change Password** (current, new, confirm) with loading/success/error states
- [x] **Integration tests** — success, wrong current password, and weak new password coverage
- [x] **Session invalidation** — other sessions signed out via refresh-token revocation (v2.36)

### v2.35 — My Profile (User Account Page)
- [x] **API** — `GET /api/users/me` and `PATCH /api/users/me` (requires JWT)
- [x] **Editable fields** — first name, last name, phone (email and role are read-only; admin-only changes)
- [x] **Profile data** — returns email, role, and last login when available
- [x] **Audit trail** — profile updates logged with IP and user agent
- [x] **UI** — `/profile` page with loading/success/error states; link from top bar user menu
- [x] **Settings** — password change remains in Settings; profile editing moved to My Profile
- [x] **Integration tests** — get profile, update profile, email/role cannot be changed via API

### v2.36 — Refresh Tokens & Session Invalidation
- [x] **Refresh tokens** — login returns `accessToken` + `refreshToken`; hashed server-side with rotation on refresh
- [x] **Refresh endpoint** — `POST /api/auth/refresh` issues new token pair; old refresh token revoked
- [x] **Logout** — `POST /api/auth/logout` revokes refresh token (or all sessions when no token supplied)
- [x] **Session invalidation** — password change and reset revoke all refresh tokens and bump security stamp
- [x] **JWT validation** — access tokens include security stamp claim; invalidated tokens rejected immediately
- [x] **Frontend** — auth store persists refresh token; API client auto-refreshes on 401
- [x] **Integration tests** — refresh flow, invalid token, and post-password-change token rejection

### v2.37 — MFA / Two-Factor Authentication (TOTP)
- [x] **Setup** — `POST /api/auth/mfa/setup` returns shared key + authenticator URI (QR-ready)
- [x] **Enable / disable** — verify TOTP code via ASP.NET Identity authenticator; audit logged
- [x] **Login flow** — password login returns MFA challenge when enabled; `POST /api/auth/mfa/verify` completes sign-in
- [x] **Settings UI** — Settings → 2FA card with QR setup, manual key, enable/disable
- [x] **Integration tests** — MFA status, setup endpoint, unauthorized access

### v2.38 — SMTP Admin UI
- [x] **Database-backed SMTP** — `CompanySetting` stores host, port, credentials (password hashed server-side; never returned)
- [x] **Admin API** — `GET/PUT /api/admin/email-settings`, `POST /api/admin/email/test` (Director/SuperAdmin)
- [x] **Runtime resolution** — `EmailService` prefers DB settings over appsettings when configured
- [x] **Settings UI** — Director-only Email / SMTP card with save and send test email
- [x] **Integration tests** — `EmailSettingsTests.cs`

### v2.39 — Project Activity Feed
- [x] **Activity endpoint** — `GET /api/projects/{id}/activity` returns audit-based `ActivityItemDto` list
- [x] **Cross-entity scope** — Project, tasks, milestones, site reports, BOQ, claims and documents
- [x] **Project detail UI** — Activity tab timeline with loading, empty and error states (latest 100 events)
- [x] **API client** — `getProjectActivity` in `src/api/projects.ts`
- [x] **Integration tests** — `ProjectActivityTests.cs`

### v2.40 — Schedule Gantt
- [x] **Gantt endpoint** — `GET /api/schedule/gantt?projectId=` returns `GanttDataDto` with tasks and milestones
- [x] **Schedule service** — `ScheduleService.GetGanttDataAsync` with optional project filter
- [x] **Gantt UI** — `/schedule/gantt` CSS-based horizontal bar chart with project filter
- [x] **Schedule navigation** — Timeline / Gantt sub-nav on Schedule pages
- [x] **API client** — `getGanttData` in `src/api/schedule.ts`
- [x] **Mobile fallback** — card list view on narrow screens; horizontal scroll chart on desktop
- [x] **Integration tests** — `GanttDataTests.cs` smoke test

### v2.41 — Fleet Fuel Log Create Form
- [x] **Backend** — `POST /api/fleet/{id}/fuel-logs` (`CreateFuelLogRequest`: date, litres, cost, odometer, project, notes)
- [x] **API client** — `createFuelLog` in `src/api/fleet.ts`
- [x] **Fuel log form** — `/fleet/:id/fuel/new` with validation, loading, success and error states
- [x] **Vehicle detail** — fuel log list shows odometer/notes; link to add fuel log
- [x] **RBAC** — Fleet module policy (`FleetManager`, `EquipmentManager`, `OperationsManager`, `ProjectManager`, Directors)
- [x] **Integration tests** — `FuelLogFlowTests.cs` (create success + vehicle not found)

### v2.42 — Edit BOQ Items + Claims
- [x] **Backend** — `PUT /api/boq/{id}` and `PUT /api/claims/{id}` (amount recalculated for BOQ; audit logged)
- [x] **DTO enrichment** — `BoqItemDto.Notes`, `ClaimDto.Notes` + `SubmittedByName` for edit prefill
- [x] **API clients** — `getBoqItem`, `updateBoqItem`, `getClaim`, `updateClaim` in `src/api/boq.ts`
- [x] **Edit forms** — `/boq/:id/edit` and `/boq/claims/:id/edit` with validation, loading, success and error states
- [x] **Navigation** — edit links from BOQ page tables and project BOQ / Financial tabs
- [x] **RBAC** — BOQ module policy (QuantitySurveyor, Estimator, Finance, ContractManager, ProjectManager, OperationsManager, Directors)
- [x] **Integration tests** — `BoqClaimsUpdateTests.cs` (BOQ amount recalc + claim title update)

### v2.43 — Admin Roles UI
- [x] **API** — `GET /api/admin/roles` and `GET /api/admin/roles/matrix` (Director/SuperAdmin only)
- [x] **API client** — `getRoleMatrix` in `src/api/admin.ts`
- [x] **Roles page** — `/admin/roles` lists system roles with user counts and read-only module permission matrix
- [x] **Role detail** — per-role module access grid, assigned users list, loading/empty/error states
- [x] **Navigation** — Settings → Administration link; route guard via `roleAccess.ts`
- [x] **RBAC** — Director and SuperAdmin only (matches existing admin endpoints)

### v2.44 — Safety MVP
- [x] **SafetyIncident entity** — project-linked incidents with severity/status enums
- [x] **Migration** — `AddSafetyIncidents`
- [x] **API** — `SafetyController` CRUD with `ISafetyService` / `SafetyService`
- [x] **RBAC** — `ErpModules.Safety` for Safety Officer and Directors
- [x] **Integration tests** — `SafetyIncidentTests.cs`

### v2.45 — Compliance MVP
- [x] **ComplianceRecord entity** — certificates, licences, permits with expiry tracking
- [x] **Migration** — `AddComplianceRecords`
- [x] **API** — `ComplianceController` CRUD with `IComplianceService` / `ComplianceService`
- [x] **RBAC** — `ErpModules.Compliance` module matrix
- [x] **Integration tests** — `ComplianceRecordTests.cs`
- [x] **Health version** — bumped to `2.45`

---

## Production Deploy (Docker)

**Full guide:** [`docs/DEPLOY.md`](docs/DEPLOY.md) — VPS setup, DNS, Let's Encrypt, backups, CI/CD.

```powershell
# Local / dev stack
docker compose up -d --build
# App: http://localhost

# Production (domain + HTTPS)
cp deploy/.env.production.example .env.production
cp deploy/Caddyfile.production deploy/Caddyfile
# Edit both files with your domain and secrets
docker compose -f docker-compose.yml -f docker-compose.prod.yml --env-file .env.production up -d --build
# App: https://your-domain.co.za

# Optional local HTTPS (self-signed):
docker compose --profile ssl up -d --build
# App: https://localhost
```

The **web** container serves the React app and proxies API/SignalR to the **api** container. Local dev without Docker still uses SQLite + Vite on port 5173.

To test MinIO locally without Docker API:
1. `docker compose up minio -d`
2. Set in `appsettings.Development.json`: `"FileStorage": { "Provider": "Minio", ... }` (copy Minio block from Production)

---

## Troubleshooting — "I see nothing"

**Most common cause:** only one server is running. You need **both** the API (port 5000) and the Web dev server (port 5173). Run `.\scripts\start-dev.ps1` to start both.

1. **Start the API** (terminal 1):
   ```powershell
   cd src\Topzinto.Erp.Api
   dotnet run
   ```
   Wait for `Now listening on: http://localhost:5000`

2. **Start the frontend** (terminal 2):
   ```powershell
   cd src\Topzinto.Erp.Web
   npm run dev
   ```
   Open **http://localhost:5173** (not the API URL)

3. **Log in** with `admin@topzinto.com` / `Topzinto@2024`

4. If data is empty, the API seeds demo data on first run. Delete `topzinto_erp.db` and restart API to re-seed.

## Tech Stack

| Layer | Technology |
|-------|------------|
| API | .NET 8, ASP.NET Core, EF Core |
| Database | PostgreSQL 16 |
| Frontend | React 18, TypeScript, Vite |
| Auth | JWT + ASP.NET Identity |
