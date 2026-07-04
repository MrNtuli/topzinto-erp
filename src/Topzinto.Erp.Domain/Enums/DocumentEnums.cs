namespace Topzinto.Erp.Domain.Enums;

public enum DocumentParentType
{
    Project,
    Contract,
    Tender,
    Client,
    Company
}

public enum DocumentStatus
{
    Draft,
    PendingApproval,
    Approved,
    Rejected,
    Expired
}
