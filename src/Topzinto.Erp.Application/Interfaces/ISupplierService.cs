using Topzinto.Erp.Application.DTOs.Suppliers;

namespace Topzinto.Erp.Application.Interfaces;

public interface ISupplierService
{
    Task<IReadOnlyList<SupplierDto>> GetAllAsync(string? search = null, string? status = null, CancellationToken ct = default);
    Task<SupplierDetailDto?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<SupplierDetailDto> CreateAsync(CreateSupplierRequest request, Guid? userId, CancellationToken ct = default);
    Task<SupplierDetailDto?> UpdateAsync(Guid id, UpdateSupplierRequest request, Guid? userId, CancellationToken ct = default);
}
