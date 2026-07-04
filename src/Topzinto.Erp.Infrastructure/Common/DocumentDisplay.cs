using Topzinto.Erp.Domain.Enums;

namespace Topzinto.Erp.Infrastructure.Common;

public static class DocumentDisplay
{
    public static string FormatParentType(DocumentParentType t) => t switch
    {
        DocumentParentType.Project => "Project",
        DocumentParentType.Contract => "Contract",
        DocumentParentType.Tender => "Tender",
        DocumentParentType.Client => "Client",
        DocumentParentType.Company => "Company",
        _ => t.ToString(),
    };

    public static DocumentParentType ParseParentType(string? value) =>
        value?.ToLowerInvariant() switch
        {
            "project" => DocumentParentType.Project,
            "contract" => DocumentParentType.Contract,
            "tender" => DocumentParentType.Tender,
            "client" => DocumentParentType.Client,
            "company" => DocumentParentType.Company,
            _ => DocumentParentType.Project,
        };

    public static string FormatStatus(DocumentStatus s) => s switch
    {
        DocumentStatus.Draft => "Draft",
        DocumentStatus.PendingApproval => "Pending Approval",
        DocumentStatus.Approved => "Approved",
        DocumentStatus.Rejected => "Rejected",
        DocumentStatus.Expired => "Expired",
        _ => s.ToString(),
    };

    public static DocumentStatus ParseStatus(string? value) =>
        value?.ToLowerInvariant() switch
        {
            "draft" => DocumentStatus.Draft,
            "pending approval" or "pendingapproval" => DocumentStatus.PendingApproval,
            "approved" => DocumentStatus.Approved,
            "rejected" => DocumentStatus.Rejected,
            "expired" => DocumentStatus.Expired,
            _ => DocumentStatus.Approved,
        };

    public static string FormatDate(DateTime? d) => d?.ToString("yyyy-MM-dd") ?? "";

    public static bool IsExpiringSoon(DateTime? expiry)
    {
        if (expiry is null) return false;
        return expiry.Value.Date <= DateTime.UtcNow.Date.AddDays(30);
    }
}
