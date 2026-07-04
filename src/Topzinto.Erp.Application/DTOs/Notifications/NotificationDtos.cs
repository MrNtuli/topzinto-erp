namespace Topzinto.Erp.Application.DTOs.Notifications;

public record NotificationDto(
    Guid Id,
    string Title,
    string Message,
    string Category,
    string Severity,
    string? LinkPath,
    bool IsRead,
    string CreatedAt
);

public record NotificationSummaryDto(int UnreadCount, int Total);

public record CompanySettingsDto(
    string CompanyName,
    string Tagline,
    string? Address,
    string? City,
    string? Province,
    string? Phone,
    string? Email,
    string? VatNumber,
    string? CidbNumber
);

public record UpdateCompanySettingsRequest(
    string CompanyName,
    string Tagline,
    string? Address,
    string? City,
    string? Province,
    string? Phone,
    string? Email,
    string? VatNumber,
    string? CidbNumber
);
