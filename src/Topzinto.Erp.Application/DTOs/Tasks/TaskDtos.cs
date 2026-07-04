namespace Topzinto.Erp.Application.DTOs.Tasks;

public record ProjectTaskDto(
    Guid Id,
    Guid ProjectId,
    string ProjectName,
    string Title,
    string Status,
    string Priority,
    string? DueDate,
    string? AssignedToName
);

public record CreateProjectTaskRequest(
    Guid ProjectId,
    Guid? MilestoneId,
    string Title,
    string? Description,
    string? AssignedToName,
    DateTime? DueDate,
    string Priority,
    string Status
);

public record ProjectMilestoneDto(
    Guid Id,
    Guid ProjectId,
    string ProjectName,
    string Name,
    string? StartDate,
    string DueDate,
    string Status,
    int Progress
);

public record CreateMilestoneRequest(
    Guid ProjectId,
    string Name,
    DateTime? StartDate,
    DateTime DueDate,
    string Status,
    int Progress
);

public record ScheduleEventDto(
    Guid Id,
    string Title,
    string Type,
    string Date,
    string? EndDate,
    string? ProjectName,
    string Status
);
