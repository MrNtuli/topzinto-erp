namespace Topzinto.Erp.Domain.Enums;

public enum SupplierStatus
{
    Active,
    Inactive,
    Blacklisted
}

public enum SupplierCategory
{
    Materials,
    PlantHire,
    Services,
    Subcontractor,
    Other
}

public enum PoStatus
{
    Draft,
    PendingApproval,
    Approved,
    Ordered,
    Delivered,
    Cancelled
}

public enum InventoryTransactionType
{
    StockIn,
    StockOut,
    Adjustment
}
