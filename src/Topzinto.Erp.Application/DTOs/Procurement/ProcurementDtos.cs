namespace Topzinto.Erp.Application.DTOs.Procurement;

public record ProcurementSummaryDto(
    int TotalOrders,
    int PendingApproval,
    int Approved,
    int Delivered,
    decimal TotalValue
);

public record PurchaseOrderDto(
    Guid Id,
    string PoNumber,
    string Title,
    string SupplierName,
    string? ProjectName,
    string Status,
    decimal TotalAmount,
    string OrderDate,
    string? RequiredDate
);

public record PurchaseOrderLineDto(
    Guid Id,
    string Description,
    decimal Quantity,
    string Unit,
    decimal UnitPrice,
    decimal LineTotal
);

public record PurchaseOrderDetailDto(
    Guid Id,
    string PoNumber,
    string Title,
    Guid SupplierId,
    string SupplierName,
    Guid? ProjectId,
    string? ProjectName,
    string Status,
    decimal TotalAmount,
    string OrderDate,
    string? RequiredDate,
    string? RequestedByName,
    string? ApprovedByName,
    string? Notes,
    IReadOnlyList<PurchaseOrderLineDto> Lines
);

public record CreatePurchaseOrderRequest(
    string PoNumber,
    string Title,
    Guid SupplierId,
    Guid? ProjectId,
    string Status,
    DateTime OrderDate,
    DateTime? RequiredDate,
    string? RequestedByName,
    string? Notes,
    IReadOnlyList<CreatePurchaseOrderLineRequest> Lines
);

public record CreatePurchaseOrderLineRequest(
    string Description,
    decimal Quantity,
    string Unit,
    decimal UnitPrice
);

public record UpdatePurchaseOrderRequest(
    string Title,
    Guid SupplierId,
    Guid? ProjectId,
    string Status,
    DateTime OrderDate,
    DateTime? RequiredDate,
    string? RequestedByName,
    string? ApprovedByName,
    string? Notes,
    IReadOnlyList<CreatePurchaseOrderLineRequest> Lines
);
