namespace Topzinto.Erp.Application.DTOs.Contracts;

public record ContractDto(
    Guid Id,
    string ContractNumber,
    string Title,
    string ClientName,
    Guid ClientId,
    string Status,
    decimal Value,
    string? EndDate,
    string? ProjectName
);

public record ContractDetailDto(
    Guid Id,
    string ContractNumber,
    string Title,
    Guid ClientId,
    string ClientName,
    Guid? ProjectId,
    string? ProjectName,
    decimal Value,
    string? StartDate,
    string? EndDate,
    decimal RetentionPercent,
    string Status,
    string? Notes
);

public record CreateContractRequest(
    string ContractNumber,
    string Title,
    Guid ClientId,
    Guid? ProjectId,
    decimal Value,
    DateTime? StartDate,
    DateTime? EndDate,
    decimal RetentionPercent,
    string Status,
    string? Notes
);

public record UpdateContractRequest(
    string ContractNumber,
    string Title,
    Guid ClientId,
    Guid? ProjectId,
    decimal Value,
    DateTime? StartDate,
    DateTime? EndDate,
    decimal RetentionPercent,
    string Status,
    string? Notes
);
