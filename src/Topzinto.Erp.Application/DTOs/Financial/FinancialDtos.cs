namespace Topzinto.Erp.Application.DTOs.Financial;

public record BoqSummaryDto(decimal TotalValue, int ItemCount, int ProjectCount, decimal AverageRate);

public record BoqItemDto(
    Guid Id,
    string ItemCode,
    string Description,
    string Category,
    string Unit,
    decimal Quantity,
    decimal Rate,
    decimal Amount,
    Guid ProjectId,
    string ProjectName,
    string? Notes
);

public record CreateBoqItemRequest(
    Guid ProjectId,
    string ItemCode,
    string Description,
    string Category,
    string Unit,
    decimal Quantity,
    decimal Rate,
    string? Notes
);

public record UpdateBoqItemRequest(
    string ItemCode,
    string Description,
    string Category,
    string Unit,
    decimal Quantity,
    decimal Rate,
    string? Notes
);

public record ClaimDto(
    Guid Id,
    string ClaimNumber,
    string Title,
    string ProjectName,
    string Status,
    decimal Amount,
    string ClaimDate,
    string? PeriodFrom,
    string? PeriodTo,
    string? SubmittedByName,
    string? Notes
);

public record CreateClaimRequest(
    Guid ProjectId,
    string ClaimNumber,
    string Title,
    string ClaimDate,
    string? PeriodFrom,
    string? PeriodTo,
    decimal Amount,
    string Status,
    string? SubmittedByName,
    string? Notes
);

public record UpdateClaimRequest(
    string ClaimNumber,
    string Title,
    string ClaimDate,
    string? PeriodFrom,
    string? PeriodTo,
    decimal Amount,
    string Status,
    string? SubmittedByName,
    string? Notes
);

public record InvoiceDto(
    Guid Id,
    string InvoiceNumber,
    string ProjectName,
    string ClientName,
    string Status,
    decimal Amount,
    string InvoiceDate,
    string? DueDate
);

public record FinancialSummaryDto(
    decimal BoqTotal,
    decimal PendingClaims,
    decimal PaidClaims,
    decimal OutstandingInvoices,
    decimal PaidInvoices
);
