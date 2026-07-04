namespace Topzinto.Erp.Application.DTOs.Employees;

public record EmployeeDto(
    Guid Id,
    string EmployeeNumber,
    string FullName,
    string JobTitle,
    string Department,
    string Trade,
    string Status,
    string? Phone,
    string? Email,
    string? AssignedProjectName,
    string HireDate
);

public record EmployeeDetailDto(
    Guid Id,
    string EmployeeNumber,
    string FirstName,
    string LastName,
    string? IdNumber,
    string JobTitle,
    string Department,
    string Trade,
    string Status,
    string? Phone,
    string? Email,
    string HireDate,
    string? TerminationDate,
    Guid? AssignedProjectId,
    string? AssignedProjectName,
    decimal? HourlyRate,
    string? Notes
);

public record CreateEmployeeRequest(
    string EmployeeNumber,
    string FirstName,
    string LastName,
    string? IdNumber,
    string JobTitle,
    string Department,
    string Trade,
    string Status,
    string? Phone,
    string? Email,
    string HireDate,
    string? TerminationDate,
    Guid? AssignedProjectId,
    decimal? HourlyRate,
    string? Notes
);

public record UpdateEmployeeRequest(
    string EmployeeNumber,
    string FirstName,
    string LastName,
    string? IdNumber,
    string JobTitle,
    string Department,
    string Trade,
    string Status,
    string? Phone,
    string? Email,
    string HireDate,
    string? TerminationDate,
    Guid? AssignedProjectId,
    decimal? HourlyRate,
    string? Notes
);
