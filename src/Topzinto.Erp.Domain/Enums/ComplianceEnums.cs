namespace Topzinto.Erp.Domain.Enums;

public enum ComplianceRecordType
{
    Insurance = 0,
    License = 1,
    Certificate = 2,
    Permit = 3,
    Inspection = 4,
    Other = 5,
}

public enum ComplianceRecordStatus
{
    Valid = 0,
    ExpiringSoon = 1,
    Expired = 2,
    Pending = 3,
    Revoked = 4,
}
