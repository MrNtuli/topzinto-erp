namespace Topzinto.Erp.Application.DTOs.Tenders;

public record TenderDto(
    Guid Id,
    string ReferenceNumber,
    string Title,
    string ClientName,
    Guid ClientId,
    string Status,
    string ClosingDate,
    decimal EstimatedValue
);

public record TenderDetailDto(
    Guid Id,
    string ReferenceNumber,
    string Title,
    Guid ClientId,
    string ClientName,
    string ClosingDate,
    string Status,
    decimal EstimatedValue,
    string? Notes
);

public record CreateTenderRequest(
    string ReferenceNumber,
    string Title,
    Guid ClientId,
    DateTime ClosingDate,
    string Status,
    decimal EstimatedValue,
    string? Notes
);

public record UpdateTenderRequest(
    string ReferenceNumber,
    string Title,
    Guid ClientId,
    DateTime ClosingDate,
    string Status,
    decimal EstimatedValue,
    string? Notes
);
