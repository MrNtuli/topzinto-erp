namespace Topzinto.Erp.Application.DTOs.Attendance;

public record AttendanceRecordDto(
    Guid Id,
    Guid EmployeeId,
    string EmployeeName,
    Guid? ProjectId,
    string? ProjectName,
    string WorkDate,
    string Status,
    string? CheckInTime,
    string? CheckOutTime,
    decimal? HoursWorked,
    string? Notes
);

public record CreateAttendanceRecordRequest(
    Guid EmployeeId,
    Guid? ProjectId,
    DateTime WorkDate,
    string Status,
    string? CheckInTime,
    string? CheckOutTime,
    decimal? HoursWorked,
    string? Notes
);

public record UpdateAttendanceRecordRequest(
    Guid EmployeeId,
    Guid? ProjectId,
    DateTime WorkDate,
    string Status,
    string? CheckInTime,
    string? CheckOutTime,
    decimal? HoursWorked,
    string? Notes
);
