namespace Topzinto.Erp.Application.Interfaces;

public interface IEmailService
{
    Task SendAsync(string to, string subject, string htmlBody, CancellationToken ct = default);
    bool IsEnabled { get; }
}
