using Topzinto.Erp.Application.DTOs.Attendance;

namespace Topzinto.Erp.Application.Interfaces;

public interface IAttendanceService
{
    Task<IReadOnlyList<AttendanceRecordDto>> GetAllAsync(
        Guid? projectId = null,
        Guid? employeeId = null,
        string? status = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        CancellationToken ct = default);

    Task<AttendanceRecordDto?> GetByIdAsync(Guid id, CancellationToken ct = default);

    Task<AttendanceRecordDto> CreateAsync(CreateAttendanceRecordRequest request, Guid? userId, CancellationToken ct = default);

    Task<AttendanceRecordDto?> UpdateAsync(Guid id, UpdateAttendanceRecordRequest request, Guid? userId, CancellationToken ct = default);

    Task<bool> DeleteAsync(Guid id, Guid? userId, CancellationToken ct = default);
}
