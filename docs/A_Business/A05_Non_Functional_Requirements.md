# A05 – Non-Functional Requirements Specification (NFR)

| Field | Value |
|-------|-------|
| **Document ID** | A05 |
| **Version** | 1.0 |
| **Status** | Approved |

---

## 1. Security

| ID | Requirement | Target |
|----|-------------|--------|
| SEC-01 | HTTPS in production | Mandatory |
| SEC-02 | Password hashing (bcrypt cost ≥ 12) | Mandatory |
| SEC-03 | JWT access + refresh token rotation | Mandatory |
| SEC-04 | RBAC on all API endpoints | Mandatory |
| SEC-05 | CSRF protection | Mandatory |
| SEC-06 | XSS prevention (output encoding, CSP) | Mandatory |
| SEC-07 | SQL injection prevention (parameterized queries/ORM) | Mandatory |
| SEC-08 | File upload validation (type, size, scan-ready) | Mandatory |
| SEC-09 | Sensitive field encryption at rest | Mandatory |
| SEC-10 | Rate limiting on auth endpoints | Mandatory |
| SEC-11 | Audit logs never editable | Mandatory |

---

## 2. Performance

| ID | Requirement | Target |
|----|-------------|--------|
| PERF-01 | Page load (authenticated shell) | < 3s on broadband |
| PERF-02 | API list endpoints (paginated) | < 500ms p95 |
| PERF-03 | Dashboard KPI aggregation | < 2s |
| PERF-04 | Concurrent users (v1) | 50 simultaneous |
| PERF-05 | Document upload | Up to 50MB per file |

---

## 3. Availability & Reliability

| ID | Requirement | Target |
|----|-------------|--------|
| AVL-01 | Uptime (production) | 99.5% |
| AVL-02 | Daily automated DB backup | Mandatory |
| AVL-03 | Backup verification | Weekly |
| AVL-04 | Disaster recovery runbook | Documented |

---

## 4. Scalability

| ID | Requirement | Target |
|----|-------------|--------|
| SCL-01 | Modular architecture for new modules | Mandatory |
| SCL-02 | Database indexing strategy | Mandatory |
| SCL-03 | Horizontal API scaling ready (stateless) | Mandatory |
| SCL-04 | tenant_id column on core tables (future multi-tenant) | Design |

---

## 5. Maintainability

| ID | Requirement | Target |
|----|-------------|--------|
| MNT-01 | Clean Architecture layers | Mandatory |
| MNT-02 | OpenAPI documentation | Mandatory |
| MNT-03 | EF Core migrations | Mandatory |
| MNT-04 | Structured logging (Serilog) | Mandatory |
| MNT-05 | Unit tests on business logic | Target 70% core |

---

## 6. Usability

| ID | Requirement | Target |
|----|-------------|--------|
| UX-01 | Responsive: desktop, tablet, mobile | Mandatory |
| UX-02 | Consistent design system | Mandatory |
| UX-03 | Breadcrumb navigation | Mandatory |
| UX-04 | Keyboard-accessible forms | Should |
| UX-05 | Role-specific dashboards | Mandatory |

---

## 7. Compatibility

| ID | Requirement | Target |
|----|-------------|--------|
| CMP-01 | Chrome, Edge, Firefox (latest 2 versions) | Mandatory |
| CMP-02 | Docker deployment | Mandatory |
| CMP-03 | PostgreSQL 16+ | Mandatory |

---

## References

- [B01 – System Architecture](../B_Architecture/B01_System_Architecture.md)
- [A08 – Risk Assessment](./A08_Risk_Assessment.md)

---

*End of Document A05*
