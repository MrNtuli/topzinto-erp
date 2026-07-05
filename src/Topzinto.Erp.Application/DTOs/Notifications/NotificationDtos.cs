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

public record EmailSettingsDto(
    bool SmtpEnabled,
    string? SmtpHost,
    int? SmtpPort,
    bool SmtpUseSsl,
    string? SmtpUsername,
    bool HasPassword,
    string? EmailFromAddress,
    string? EmailFromName
);

public record UpdateEmailSettingsRequest(
    bool SmtpEnabled,
    string? SmtpHost,
    int SmtpPort,
    bool SmtpUseSsl,
    string? SmtpUsername,
    string? SmtpPassword,
    string? EmailFromAddress,
    string? EmailFromName
);

public record TestEmailRequest(string ToEmail);
