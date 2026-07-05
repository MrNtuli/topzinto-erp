using Topzinto.Erp.Application.DTOs.Compliance;

namespace Topzinto.Erp.Application.Interfaces;

public interface IComplianceService
{
    Task<IReadOnlyList<ComplianceRecordDto>> GetAllAsync(Guid? projectId = null, string? status = null, CancellationToken ct = default);
    Task<ComplianceRecordDto?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<ComplianceRecordDto> CreateAsync(CreateComplianceRecordRequest request, Guid? userId, CancellationToken ct = default);
    Task<ComplianceRecordDto?> UpdateAsync(Guid id, UpdateComplianceRecordRequest request, Guid? userId, CancellationToken ct = default);
    Task<bool> DeleteAsync(Guid id, Guid? userId, CancellationToken ct = default);
}
