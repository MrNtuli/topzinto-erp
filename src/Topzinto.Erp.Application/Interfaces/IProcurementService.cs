using Topzinto.Erp.Application.DTOs.Procurement;

namespace Topzinto.Erp.Application.Interfaces;

public interface IProcurementService
{
    Task<ProcurementSummaryDto> GetSummaryAsync(CancellationToken ct = default);
    Task<IReadOnlyList<PurchaseOrderDto>> GetAllAsync(string? search = null, string? status = null, CancellationToken ct = default);
    Task<PurchaseOrderDetailDto?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<PurchaseOrderDetailDto> CreateAsync(CreatePurchaseOrderRequest request, Guid? userId, CancellationToken ct = default);
    Task<PurchaseOrderDetailDto?> UpdateAsync(Guid id, UpdatePurchaseOrderRequest request, Guid? userId, CancellationToken ct = default);
}
