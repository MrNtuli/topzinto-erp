# B01 – System Architecture

| Field | Value |
|-------|-------|
| **Document ID** | B01 |
| **Version** | 1.0 |

---

## 1. Architecture Style

**Modular Monolith** with Clean Architecture layers. Microservices-ready boundaries without operational complexity of distributed systems for v1.

```
┌─────────────────────────────────────────────────────────┐
│                    React SPA (Vite)                      │
│              TopZinto Design System + Shell                │
└─────────────────────────┬───────────────────────────────┘
                          │ HTTPS / REST + SignalR
┌─────────────────────────▼───────────────────────────────┐
│              ASP.NET Core 8 Web API                      │
│  ┌─────────────┐ ┌──────────────┐ ┌─────────────────┐ │
│  │ Controllers │ │  Middleware   │ │  SignalR Hubs   │ │
│  └──────┬──────┘ └──────────────┘ └─────────────────┘ │
│  ┌──────▼──────────────────────────────────────────┐  │
│  │              Application Services                │  │
│  │   (Projects, Fleet, Documents, Auth, etc.)       │  │
│  └──────┬──────────────────────────────────────────┘  │
│  ┌──────▼──────────────────────────────────────────┐  │
│  │         Domain (Entities, Rules, Interfaces)     │  │
│  └──────┬──────────────────────────────────────────┘  │
│  ┌──────▼──────────────────────────────────────────┐  │
│  │    Infrastructure (EF Core, Storage, Email)    │  │
│  └─────────────────────────────────────────────────┘  │
└─────────────────────────┬───────────────────────────────┘
          ┌───────────────┼───────────────┐
          ▼               ▼               ▼
    PostgreSQL        Redis          S3/MinIO
```

---

## 2. Solution Structure

```
src/
├── Topzinto.Erp.Api/           # HTTP layer, DI bootstrap
├── Topzinto.Erp.Application/   # Services, DTOs, interfaces
├── Topzinto.Erp.Domain/        # Entities, enums, domain rules
├── Topzinto.Erp.Infrastructure/# EF Core, repositories, storage
└── Topzinto.Erp.Web/           # React + TypeScript + Vite
```

---

## 3. Technology Stack

| Layer | Technology |
|-------|------------|
| API | ASP.NET Core 8, C# 12 |
| ORM | Entity Framework Core 8 |
| Database | PostgreSQL 16 |
| Cache | Redis 7 |
| Auth | ASP.NET Identity + JWT |
| Jobs | Hangfire |
| Logging | Serilog |
| Files | MinIO / Azure Blob (S3 API) |
| Frontend | React 18, TypeScript, Vite |
| State | TanStack Query + Zustand |
| UI | Radix primitives + custom design tokens |
| Charts | Apache ECharts |
| Real-time | SignalR |

---

## 4. Cross-Cutting Concerns

| Concern | Implementation |
|---------|----------------|
| Authentication | JWT bearer + refresh cookie |
| Authorization | Policy-based RBAC middleware |
| Audit | Interceptor on SaveChanges + explicit service |
| Validation | FluentValidation |
| Mapping | Manual or Mapster |
| Errors | Global exception handler → ProblemDetails |
| API Docs | Swagger/OpenAPI |

---

## 5. Module Boundaries

Each module has:
- Domain entities in `Domain/Entities/{Module}/`
- Service in `Application/Services/{Module}/`
- Controller in `Api/Controllers/{Module}/`
- React routes in `Web/src/pages/{module}/`

Modules communicate via **shared database** and **application services** — no direct cross-module repository access.

---

## 6. Integration Points (Future)

| System | Pattern |
|--------|---------|
| Sage / Pastel | Export CSV + future REST adapter |
| M365 | OAuth + Graph API |
| SMS / WhatsApp | Provider abstraction interface |
| GPS | Webhook ingestion service |
| Power BI | Read API + export datasets |

---

## References

- [C01 – Database Schema](../C_Database/C01_Database_Schema.md)
- [BLUEPRINT.md](../BLUEPRINT.md)

---

*End of Document B01*
