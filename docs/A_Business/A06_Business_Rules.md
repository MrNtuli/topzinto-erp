# A06 – Business Rules Specification

| Field | Value |
|-------|-------|
| **Document ID** | A06 |
| **Version** | 1.0 |
| **Status** | Approved |

---

## 1. General Rules

| Rule ID | Rule |
|---------|------|
| BR-GEN-01 | All monetary values stored in ZAR (decimal 18,2) |
| BR-GEN-02 | All dates stored UTC; displayed in Africa/Johannesburg |
| BR-GEN-03 | Soft delete only — records marked `IsDeleted`, never hard-deleted |
| BR-GEN-04 | Every create/update on critical entities generates audit log |
| BR-GEN-05 | Client organizations cannot have system user accounts |

---

## 2. Authentication Rules

| Rule ID | Rule |
|---------|------|
| BR-AUTH-01 | Minimum password: 8 chars, upper, lower, number, special |
| BR-AUTH-02 | Lock account after 5 failed attempts for 30 minutes |
| BR-AUTH-03 | Session expires after 30 minutes inactivity (configurable) |
| BR-AUTH-04 | Password reset token expires in 1 hour |
| BR-AUTH-05 | Super Admin cannot be deleted; minimum 1 Super Admin must exist |

---

## 3. Project Rules

| Rule ID | Rule |
|---------|------|
| BR-PRJ-01 | Project code must be unique |
| BR-PRJ-02 | Project must have a client before status = Active |
| BR-PRJ-03 | Completed projects are read-only except Admin |
| BR-PRJ-04 | Progress 0–100%; 100% only when status = Completed |
| BR-PRJ-05 | Contract value defaults from linked contract if exists |

---

## 4. Tender Rules

| Rule ID | Rule |
|---------|------|
| BR-TEN-01 | Tender closing date cannot be in the past on create |
| BR-TEN-02 | Notification sent 7, 3, 1 days before closing |
| BR-TEN-03 | Won tender must create or link contract before project activation |
| BR-TEN-04 | Lost tenders archived but retained for reporting |

---

## 5. Document Rules

| Rule ID | Rule |
|---------|------|
| BR-DOC-01 | Every document must link to a parent entity |
| BR-DOC-02 | Re-upload creates new version; previous retained |
| BR-DOC-03 | Expiry notification at 30, 14, 7, 1 days before |
| BR-DOC-04 | Expired documents flagged red in UI |
| BR-DOC-05 | Max file size 50MB; allowed types: pdf, doc, docx, xls, xlsx, jpg, png, zip |

---

## 6. Fleet Rules

| Rule ID | Rule |
|---------|------|
| BR-FLT-01 | Vehicle cannot be In Use on two projects simultaneously |
| BR-FLT-02 | Vehicle with expired licence cannot be assigned |
| BR-FLT-03 | Maintenance due triggers notification 14 days before |

---

## 7. Equipment Rules

| Rule ID | Rule |
|---------|------|
| BR-EQP-01 | Booking conflicts prevented for same asset/time |
| BR-EQP-02 | Inspection overdue blocks new bookings (configurable) |

---

## 8. Procurement Rules

| Rule ID | Rule |
|---------|------|
| BR-PRO-01 | PO requires approval above configurable threshold |
| BR-PRO-02 | PO must link to project for cost allocation |
| BR-PRO-03 | Stock out cannot exceed available quantity |

---

## 9. Site Report Rules

| Rule ID | Rule |
|---------|------|
| BR-SR-01 | One report per project per date (editable until submitted) |
| BR-SR-02 | Submitted reports require Supervisor+ to edit |
| BR-SR-03 | Photos optional but max 20 per report |

---

## 10. Financial Rules

| Rule ID | Rule |
|---------|------|
| BR-FIN-01 | BOQ line total = qty × rate (auto-calculated) |
| BR-FIN-02 | Claim amount cannot exceed contract value minus previous claims |
| BR-FIN-03 | Retention held until practical completion (manual release) |

---

## References

- [A04 – Functional Requirements](./A04_Functional_Requirements.md)
- [A07 – User Stories](./A07_User_Stories.md)

---

*End of Document A06*
