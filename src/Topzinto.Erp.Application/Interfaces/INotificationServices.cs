using Topzinto.Erp.Application.DTOs.Notifications;

namespace Topzinto.Erp.Application.Interfaces;

public interface INotificationService
{
    Task<NotificationSummaryDto> GetSummaryAsync(Guid? userId, CancellationToken ct = default);
    Task<IReadOnlyList<NotificationDto>> GetAllAsync(Guid? userId, bool? unreadOnly = null, CancellationToken ct = default);
    Task MarkAsReadAsync(Guid id, Guid? userId, CancellationToken ct = default);
    Task MarkAllAsReadAsync(Guid? userId, CancellationToken ct = default);
    Task<int> GenerateSystemAlertsAsync(CancellationToken ct = default);
}

public interface ICompanySettingsService
{
    Task<CompanySettingsDto> GetAsync(CancellationToken ct = default);
    Task<CompanySettingsDto> UpdateAsync(UpdateCompanySettingsRequest request, Guid? userId, CancellationToken ct = default);
    Task<EmailSettingsDto> GetEmailSettingsAsync(CancellationToken ct = default);
    Task<EmailSettingsDto> UpdateEmailSettingsAsync(UpdateEmailSettingsRequest request, Guid? userId, CancellationToken ct = default);
    Task<string> TestEmailAsync(string toEmail, CancellationToken ct = default);
}
