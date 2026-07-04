using Topzinto.Erp.Domain.Enums;

namespace Topzinto.Erp.Infrastructure.Common;

public static class ProcurementDisplay
{
    public static string FormatPoStatus(PoStatus s) => s switch
    {
        PoStatus.Draft => "Draft",
        PoStatus.PendingApproval => "Pending Approval",
        PoStatus.Approved => "Approved",
        PoStatus.Ordered => "Ordered",
        PoStatus.Delivered => "Delivered",
        PoStatus.Cancelled => "Cancelled",
        _ => s.ToString(),
    };

    public static PoStatus ParsePoStatus(string? value) =>
        value?.ToLowerInvariant() switch
        {
            "draft" => PoStatus.Draft,
            "pending approval" or "pendingapproval" => PoStatus.PendingApproval,
            "approved" => PoStatus.Approved,
            "ordered" => PoStatus.Ordered,
            "delivered" => PoStatus.Delivered,
            "cancelled" or "canceled" => PoStatus.Cancelled,
            _ => PoStatus.Draft,
        };

    public static string FormatSupplierStatus(SupplierStatus s) => s switch
    {
        SupplierStatus.Active => "Active",
        SupplierStatus.Inactive => "Inactive",
        SupplierStatus.Blacklisted => "Blacklisted",
        _ => s.ToString(),
    };

    public static SupplierStatus ParseSupplierStatus(string? value) =>
        value?.ToLowerInvariant() switch
        {
            "active" => SupplierStatus.Active,
            "inactive" => SupplierStatus.Inactive,
            "blacklisted" => SupplierStatus.Blacklisted,
            _ => SupplierStatus.Active,
        };

    public static string FormatSupplierCategory(SupplierCategory c) => c switch
    {
        SupplierCategory.Materials => "Materials",
        SupplierCategory.PlantHire => "Plant Hire",
        SupplierCategory.Services => "Services",
        SupplierCategory.Subcontractor => "Subcontractor",
        SupplierCategory.Other => "Other",
        _ => c.ToString(),
    };

    public static SupplierCategory ParseSupplierCategory(string? value) =>
        value?.ToLowerInvariant() switch
        {
            "materials" => SupplierCategory.Materials,
            "plant hire" or "planthire" => SupplierCategory.PlantHire,
            "services" => SupplierCategory.Services,
            "subcontractor" => SupplierCategory.Subcontractor,
            "other" => SupplierCategory.Other,
            _ => SupplierCategory.Materials,
        };

    public static string FormatTransactionType(InventoryTransactionType t) => t switch
    {
        InventoryTransactionType.StockIn => "Stock In",
        InventoryTransactionType.StockOut => "Stock Out",
        InventoryTransactionType.Adjustment => "Adjustment",
        _ => t.ToString(),
    };

    public static InventoryTransactionType ParseTransactionType(string? value) =>
        value?.ToLowerInvariant() switch
        {
            "stock in" or "stockin" or "in" => InventoryTransactionType.StockIn,
            "stock out" or "stockout" or "out" => InventoryTransactionType.StockOut,
            "adjustment" => InventoryTransactionType.Adjustment,
            _ => InventoryTransactionType.StockIn,
        };

    public static string FormatDate(DateTime? d) => d?.ToString("yyyy-MM-dd") ?? "";
}
