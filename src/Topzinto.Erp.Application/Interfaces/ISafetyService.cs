using Topzinto.Erp.Application.DTOs.Safety;

namespace Topzinto.Erp.Application.Interfaces;

public interface ISafetyService
{
    Task<IReadOnlyList<SafetyIncidentDto>> GetAllAsync(Guid? projectId = null, string? status = null, CancellationToken ct = default);
    Task<SafetyIncidentDto?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<SafetyIncidentDto> CreateAsync(CreateSafetyIncidentRequest request, Guid? userId, CancellationToken ct = default);
    Task<SafetyIncidentDto?> UpdateAsync(Guid id, UpdateSafetyIncidentRequest request, Guid? userId, CancellationToken ct = default);
    Task<bool> DeleteAsync(Guid id, Guid? userId, CancellationToken ct = default);
}
