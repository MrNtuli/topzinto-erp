namespace Topzinto.Erp.Application.DTOs.Documents;

public record DocumentSummaryDto(int Total, int ExpiringSoon, int Expired);

public record DocumentDto(
    Guid Id,
    string Title,
    string Category,
    string ParentType,
    string? ParentName,
    string? FileName,
    int Version,
    string Status,
    string? IssueDate,
    string? ExpiryDate,
    bool IsExpiringSoon,
    bool HasFile,
    long? FileSizeBytes
);

public record CreateDocumentRequest(
    string Title,
    string Category,
    string ParentType,
    Guid? ParentId,
    string? ParentName,
    string? FileName,
    DateTime? IssueDate,
    DateTime? ExpiryDate,
    string Status,
    string? Notes
);
