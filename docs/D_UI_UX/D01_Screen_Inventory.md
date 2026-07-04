# D01 – UI Screen Inventory (v1.0)

| Field | Value |
|-------|-------|
| **Document ID** | D01 |
| **Version** | 1.0 |

---

## Design Tokens

| Token | Value |
|-------|-------|
| Primary Navy | `#1B2B4B` |
| Construction Orange | `#F26522` |
| Success | `#22C55E` |
| Warning | `#F59E0B` |
| Danger | `#EF4444` |
| Background | `#F4F6F9` |
| Card | `#FFFFFF` |
| Font | Inter, system-ui |

---

## Global Shell

| Screen | Route | Roles |
|--------|-------|-------|
| Login | `/login` | Public |
| Forgot Password | `/forgot-password` | Public |
| Reset Password | `/reset-password` | Public |
| Dashboard | `/` | All authenticated |
| Global Search Results | `/search` | All |
| Notifications | `/notifications` | All |
| Profile | `/profile` | All |
| Settings | `/settings` | Admin |

---

## Module Screens

### Clients
| Screen | Route |
|--------|-------|
| Client List | `/clients` |
| Client Detail | `/clients/:id` |
| Create/Edit Client | `/clients/new`, `/clients/:id/edit` |

### Projects
| Screen | Route |
|--------|-------|
| Project List | `/projects` |
| Project Hub | `/projects/:id` |
| Project Hub Tabs | `?tab=overview\|schedule\|boq\|site-reports\|documents\|financial\|activity` |
| Create Project | `/projects/new` |

### Tenders
| Screen | Route |
|--------|-------|
| Tender List | `/tenders` |
| Tender Detail | `/tenders/:id` |

### Contracts
| Screen | Route |
|--------|-------|
| Contract List | `/contracts` |
| Contract Detail | `/contracts/:id` |

### Site Reports
| Screen | Route |
|--------|-------|
| Site Report List | `/site-reports` |
| Add Site Report | `/site-reports/new` |
| Site Report Detail | `/site-reports/:id` |

### Schedule
| Screen | Route |
|--------|-------|
| Calendar | `/schedule` |
| Programme Gantt | `/schedule/gantt` |

### Fleet
| Screen | Route |
|--------|-------|
| Fleet List | `/fleet` |
| Vehicle Detail | `/fleet/:id` |

### Equipment
| Screen | Route |
|--------|-------|
| Equipment List | `/equipment` |
| Equipment Detail | `/equipment/:id` |

### Procurement
| Screen | Route |
|--------|-------|
| PO List | `/procurement` |
| PO Detail | `/procurement/:id` |
| Suppliers | `/suppliers` |

### Stores
| Screen | Route |
|--------|-------|
| Inventory | `/stores` |
| Stock Transactions | `/stores/transactions` |

### Documents
| Screen | Route |
|--------|-------|
| Document Library | `/documents` |

### Reports
| Screen | Route |
|--------|-------|
| Reports Hub | `/reports` |

### Administration
| Screen | Route |
|--------|-------|
| Users | `/admin/users` |
| Roles | `/admin/roles` |
| Audit Logs | `/admin/audit` |
| Backups | `/admin/backups` |

---

## Reusable Components

- `AppShell`, `Sidebar`, `TopBar`
- `KpiCard`, `ChartCard`
- `PageHeader`, `FilterBar`
- `DataTable` (sort, filter, pagination, export)
- `StatusBadge`, `ProgressBar`
- `EntityTabs`, `SummaryCard`
- `GanttChart`, `CalendarView`
- `DocumentUploader`, `FolderTree`
- `FormSection`, `ConfirmDialog`

---

## References

- [A07 – User Stories](../A_Business/A07_User_Stories.md)
- [BLUEPRINT.md](../BLUEPRINT.md)

---

*End of Document D01*
