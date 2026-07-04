using Topzinto.Erp.Application.DTOs.Equipment;

namespace Topzinto.Erp.Application.Interfaces;

public interface IEquipmentService
{
    Task<EquipmentSummaryDto> GetSummaryAsync(CancellationToken ct = default);
    Task<IReadOnlyList<EquipmentDto>> GetAllAsync(string? search = null, string? status = null, CancellationToken ct = default);
    Task<EquipmentDetailDto?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<EquipmentDetailDto> CreateAsync(CreateEquipmentRequest request, Guid? userId, CancellationToken ct = default);
    Task<EquipmentDetailDto?> UpdateAsync(Guid id, UpdateEquipmentRequest request, Guid? userId, CancellationToken ct = default);
}
