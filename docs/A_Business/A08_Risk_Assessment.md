# A08 – Risk Assessment & Mitigation

| Field | Value |
|-------|-------|
| **Document ID** | A08 |
| **Version** | 1.0 |
| **Status** | Approved |

---

## Risk Register

| ID | Risk | Likelihood | Impact | Mitigation |
|----|------|------------|--------|------------|
| R-01 | Scope creep delays v1 | High | High | Strict v1 scope doc (A03); phase gates |
| R-02 | User adoption resistance | Medium | High | Role-specific dashboards; training; familiar UI patterns |
| R-03 | Data migration from spreadsheets | Medium | Medium | Phased migration; manual import tools; validation |
| R-04 | Security breach | Low | Critical | RBAC, audit, encryption, pen test before prod |
| R-05 | Performance degradation at scale | Medium | Medium | Pagination, indexing, caching; load testing |
| R-06 | Backup failure | Low | Critical | Automated backup + verification + alerts |
| R-07 | Key person dependency (single developer) | Medium | High | Documentation suite; clean architecture; CI/CD |
| R-08 | Internet unavailable at remote sites | Medium | Medium | Online-first v1; design offline queue for v1.2 |
| R-09 | Integration complexity (Sage, etc.) | Medium | Low | Adapter pattern; defer to post-v1 |
| R-10 | Incorrect financial data | Medium | High | Business rules; approval workflows; audit trail |
| R-11 | Document storage growth | Medium | Medium | S3-compatible storage; retention policies |
| R-12 | Regulatory compliance gaps | Low | High | CIDB/CSD fields; compliance module; legal review |

---

## Technical Risks

| ID | Risk | Mitigation |
|----|------|------------|
| TR-01 | .NET/PostgreSQL environment issues | Docker Compose; documented setup |
| TR-02 | Third-party Gantt library licensing | Evaluate OSS vs commercial early |
| TR-03 | File upload vulnerabilities | Strict validation; isolated storage |

---

## Project Risks

| ID | Risk | Mitigation |
|----|------|------------|
| PR-01 | Unclear requirements mid-build | Module-by-module with sign-off |
| PR-02 | Testing insufficient | Test plan per phase; UAT with TopZinto staff |

---

## Contingency Plans

1. **Scope reduction:** If timeline pressured, defer Stores and BOQ depth to v1.1 while keeping Projects + Documents + Fleet core.
2. **Hosting failure:** DR runbook with restore from daily backup to alternate server.
3. **Security incident:** Disable affected accounts; rotate secrets; review audit logs.

---

## References

- [A03 – Project Scope](./A03_Project_Scope.md)
- [A05 – Non-Functional Requirements](./A05_Non_Functional_Requirements.md)

---

*End of Document A08*
