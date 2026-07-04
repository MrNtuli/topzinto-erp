namespace Topzinto.Erp.Application.DTOs.Suppliers;

public record SupplierDto(
    Guid Id,
    string Code,
    string Name,
    string Category,
    string Status,
    string? ContactPerson,
    string? Phone,
    string? Email,
    string? City,
    int PurchaseOrderCount
);

public record SupplierDetailDto(
    Guid Id,
    string Code,
    string Name,
    string Category,
    string Status,
    string? ContactPerson,
    string? Phone,
    string? Email,
    string? Address,
    string? City,
    string? Province,
    string? VatNumber,
    string? Notes,
    IReadOnlyList<SupplierPoDto> RecentOrders
);

public record SupplierPoDto(Guid Id, string PoNumber, string Title, string Status, string? ProjectName, decimal TotalAmount, string OrderDate);

public record CreateSupplierRequest(
    string Code,
    string Name,
    string Category,
    string Status,
    string? ContactPerson,
    string? Phone,
    string? Email,
    string? Address,
    string? City,
    string? Province,
    string? VatNumber,
    string? Notes
);

public record UpdateSupplierRequest(
    string Code,
    string Name,
    string Category,
    string Status,
    string? ContactPerson,
    string? Phone,
    string? Email,
    string? Address,
    string? City,
    string? Province,
    string? VatNumber,
    string? Notes
);
