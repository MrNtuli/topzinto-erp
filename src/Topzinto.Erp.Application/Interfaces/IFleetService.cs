using Topzinto.Erp.Application.DTOs.Fleet;

namespace Topzinto.Erp.Application.Interfaces;

public interface IFleetService
{
    Task<FleetSummaryDto> GetSummaryAsync(CancellationToken ct = default);
    Task<IReadOnlyList<VehicleDto>> GetAllAsync(string? search = null, string? status = null, CancellationToken ct = default);
    Task<VehicleDetailDto?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<VehicleDetailDto> CreateAsync(CreateVehicleRequest request, Guid? userId, CancellationToken ct = default);
    Task<VehicleDetailDto?> UpdateAsync(Guid id, UpdateVehicleRequest request, Guid? userId, CancellationToken ct = default);
}
