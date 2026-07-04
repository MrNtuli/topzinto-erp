using Topzinto.Erp.Domain.Enums;

namespace Topzinto.Erp.Infrastructure.Common;

public static class FinancialDisplay
{
    public static string FormatClaimStatus(ClaimStatus s) => s switch
    {
        ClaimStatus.Draft => "Draft",
        ClaimStatus.Submitted => "Submitted",
        ClaimStatus.Approved => "Approved",
        ClaimStatus.Paid => "Paid",
        ClaimStatus.Rejected => "Rejected",
        _ => s.ToString(),
    };

    public static ClaimStatus ParseClaimStatus(string? value) =>
        value?.ToLowerInvariant() switch
        {
            "draft" => ClaimStatus.Draft,
            "submitted" => ClaimStatus.Submitted,
            "approved" => ClaimStatus.Approved,
            "paid" => ClaimStatus.Paid,
            "rejected" => ClaimStatus.Rejected,
            _ => ClaimStatus.Draft,
        };

    public static string FormatInvoiceStatus(InvoiceStatus s) => s switch
    {
        InvoiceStatus.Draft => "Draft",
        InvoiceStatus.Sent => "Sent",
        InvoiceStatus.Paid => "Paid",
        InvoiceStatus.Overdue => "Overdue",
        InvoiceStatus.Cancelled => "Cancelled",
        _ => s.ToString(),
    };

    public static InvoiceStatus ParseInvoiceStatus(string? value) =>
        value?.ToLowerInvariant() switch
        {
            "draft" => InvoiceStatus.Draft,
            "sent" => InvoiceStatus.Sent,
            "paid" => InvoiceStatus.Paid,
            "overdue" => InvoiceStatus.Overdue,
            "cancelled" or "canceled" => InvoiceStatus.Cancelled,
            _ => InvoiceStatus.Draft,
        };

    public static string FormatDate(DateTime? d) => d?.ToString("yyyy-MM-dd") ?? "";

    public static DateTime? ParseDateTime(string? value) =>
        DateTime.TryParse(value, out var d) ? d.Date : null;
}
