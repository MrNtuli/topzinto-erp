namespace Topzinto.Erp.Application.DTOs.Stores;

public record StoresSummaryDto(
    int TotalItems,
    int LowStock,
    decimal TotalValue,
    int TransactionsThisMonth
);

public record InventoryItemDto(
    Guid Id,
    string ItemCode,
    string Name,
    string Category,
    string Unit,
    decimal QuantityOnHand,
    decimal ReorderLevel,
    string? Location,
    decimal UnitCost,
    bool IsLowStock
);

public record InventoryItemDetailDto(
    Guid Id,
    string ItemCode,
    string Name,
    string Category,
    string Unit,
    decimal QuantityOnHand,
    decimal ReorderLevel,
    string? Location,
    decimal UnitCost,
    string? Notes,
    IReadOnlyList<InventoryTransactionDto> RecentTransactions
);

public record InventoryTransactionDto(
    Guid Id,
    string ItemCode,
    string ItemName,
    string TransactionType,
    decimal Quantity,
    string TransactionDate,
    string? Reference,
    string? ProjectName,
    string? RecordedByName,
    string? Notes
);

public record CreateInventoryItemRequest(
    string ItemCode,
    string Name,
    string Category,
    string Unit,
    decimal QuantityOnHand,
    decimal ReorderLevel,
    string? Location,
    decimal UnitCost,
    string? Notes
);

public record UpdateInventoryItemRequest(
    string ItemCode,
    string Name,
    string Category,
    string Unit,
    decimal ReorderLevel,
    string? Location,
    decimal UnitCost,
    string? Notes
);

public record CreateInventoryTransactionRequest(
    Guid InventoryItemId,
    string TransactionType,
    decimal Quantity,
    DateTime TransactionDate,
    string? Reference,
    Guid? ProjectId,
    string? Notes,
    string? RecordedByName
);
