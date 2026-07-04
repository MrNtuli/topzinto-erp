namespace Topzinto.Erp.Application.DTOs.Timesheets;

public record TimesheetEntryDto(
    Guid Id,
    Guid EmployeeId,
    string EmployeeName,
    Guid ProjectId,
    string ProjectName,
    string WorkDate,
    decimal Hours,
    string Status,
    string? Description,
    string? Notes,
    decimal? LabourCost
);

public record ProjectLabourSummaryDto(
    Guid ProjectId,
    string ProjectName,
    decimal TotalHours,
    decimal TotalLabourCost,
    int EntryCount
);

public record CreateTimesheetRequest(
    Guid EmployeeId,
    Guid ProjectId,
    string WorkDate,
    decimal Hours,
    string Status,
    string? Description,
    string? Notes
);

public record UpdateTimesheetRequest(
    Guid EmployeeId,
    Guid ProjectId,
    string WorkDate,
    decimal Hours,
    string Status,
    string? Description,
    string? Notes
);
