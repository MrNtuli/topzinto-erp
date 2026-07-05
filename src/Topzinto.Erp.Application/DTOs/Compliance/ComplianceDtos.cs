namespace Topzinto.Erp.Application.DTOs.Compliance;

public record ComplianceRecordDto(
    Guid Id,
    string Title,
    string Type,
    string? EntityType,
    Guid? EntityId,
    Guid? ProjectId,
    string? ProjectName,
    string IssueDate,
    string? ExpiryDate,
    string Status,
    string? ResponsiblePerson,
    string? Notes
);

public record CreateComplianceRecordRequest(
    string Title,
    string Type,
    string? EntityType,
    Guid? EntityId,
    Guid? ProjectId,
    DateTime IssueDate,
    DateTime? ExpiryDate,
    string Status,
    string? ResponsiblePerson,
    string? Notes
);

public record UpdateComplianceRecordRequest(
    string Title,
    string Type,
    string? EntityType,
    Guid? EntityId,
    Guid? ProjectId,
    DateTime IssueDate,
    DateTime? ExpiryDate,
    string Status,
    string? ResponsiblePerson,
    string? Notes
);
