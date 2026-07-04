using Topzinto.Erp.Application.DTOs.Timesheets;

namespace Topzinto.Erp.Application.Interfaces;

public interface ITimesheetService
{
    Task<IReadOnlyList<TimesheetEntryDto>> GetAllAsync(Guid? projectId = null, Guid? employeeId = null, string? status = null, CancellationToken ct = default);
    Task<IReadOnlyList<ProjectLabourSummaryDto>> GetLabourSummaryAsync(Guid? projectId = null, CancellationToken ct = default);
    Task<TimesheetEntryDto?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<TimesheetEntryDto> CreateAsync(CreateTimesheetRequest request, Guid? userId, CancellationToken ct = default);
    Task<TimesheetEntryDto?> UpdateAsync(Guid id, UpdateTimesheetRequest request, Guid? userId, CancellationToken ct = default);
}
