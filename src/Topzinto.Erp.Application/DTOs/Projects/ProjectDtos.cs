namespace Topzinto.Erp.Application.DTOs.Projects;

public record ProjectDto(
    Guid Id,
    string Code,
    string Name,
    string ClientName,
    Guid ClientId,
    string Status,
    int Progress,
    string? EndDate,
    decimal ContractValue
);

public record ProjectDetailDto(
    Guid Id,
    string Code,
    string Name,
    Guid ClientId,
    string ClientName,
    string Status,
    int Progress,
    string? StartDate,
    string? EndDate,
    decimal ContractValue,
    decimal Budget,
    string? Description,
    string? SiteLocation,
    Guid? ContractId,
    Guid? TenderId,
    string? StartDateInput,
    string? EndDateInput
);

public record CreateProjectRequest(
    string Code,
    string Name,
    Guid ClientId,
    string Status,
    int Progress,
    DateTime? StartDate,
    DateTime? EndDate,
    decimal ContractValue,
    decimal Budget,
    string? Description,
    string? SiteLocation
);

public record UpdateProjectRequest(
    string Code,
    string Name,
    Guid ClientId,
    string Status,
    int Progress,
    DateTime? StartDate,
    DateTime? EndDate,
    decimal ContractValue,
    decimal Budget,
    string? Description,
    string? SiteLocation
);
