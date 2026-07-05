namespace Topzinto.Erp.Application.DTOs.Safety;

public record SafetyIncidentDto(
    Guid Id,
    Guid ProjectId,
    string ProjectName,
    string IncidentDate,
    string Title,
    string Description,
    string Severity,
    string Status,
    string? Location,
    string? ReportedByName,
    string? CorrectiveAction
);

public record CreateSafetyIncidentRequest(
    Guid ProjectId,
    DateTime IncidentDate,
    string Title,
    string Description,
    string Severity,
    string Status,
    string? Location,
    string? ReportedByName,
    string? CorrectiveAction
);

public record UpdateSafetyIncidentRequest(
    Guid ProjectId,
    DateTime IncidentDate,
    string Title,
    string Description,
    string Severity,
    string Status,
    string? Location,
    string? ReportedByName,
    string? CorrectiveAction
);
