using System.Net;
using System.Net.Mail;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Topzinto.Erp.Application.Interfaces;
using Topzinto.Erp.Infrastructure.Persistence;

namespace Topzinto.Erp.Infrastructure.Services;

public class EmailService : IEmailService
{
    private static readonly Guid SettingsId = Guid.Parse("00000000-0000-0000-0000-000000000001");

    private readonly AppDbContext _db;
    private readonly IConfiguration _config;
    private readonly ILogger<EmailService> _logger;

    public EmailService(AppDbContext db, IConfiguration config, ILogger<EmailService> logger)
    {
        _db = db;
        _config = config;
        _logger = logger;
    }

    public bool IsEnabled => _config.GetValue("Email:Enabled", false);

    public async Task SendAsync(string to, string subject, string htmlBody, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(to)) return;

        var settings = await ResolveSettingsAsync(ct);
        if (!settings.Enabled)
        {
            _logger.LogInformation("Email skipped (disabled): To={To} Subject={Subject}", to, subject);
            return;
        }

        using var message = new MailMessage
        {
            From = new MailAddress(settings.FromAddress, settings.FromName),
            Subject = subject,
            Body = htmlBody,
            IsBodyHtml = true,
        };
        message.To.Add(to);

        using var client = new SmtpClient(settings.Host, settings.Port)
        {
            EnableSsl = settings.UseSsl,
        };

        if (!string.IsNullOrWhiteSpace(settings.Username))
            client.Credentials = new NetworkCredential(settings.Username, settings.Password);

        await client.SendMailAsync(message, ct);
        _logger.LogInformation("Email sent: To={To} Subject={Subject}", to, subject);
    }

    private async Task<ResolvedEmailSettings> ResolveSettingsAsync(CancellationToken ct)
    {
        var dbSettings = await _db.CompanySettings.AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == SettingsId, ct);

        var configEnabled = _config.GetValue("Email:Enabled", false);
        var hasDbHost = !string.IsNullOrWhiteSpace(dbSettings?.SmtpHost);
        var hasDbFrom = !string.IsNullOrWhiteSpace(dbSettings?.EmailFromAddress);
        var hasDbConfig = hasDbHost && hasDbFrom;
        var enabled = hasDbConfig ? dbSettings!.SmtpEnabled : configEnabled;

        if (hasDbHost && !string.IsNullOrWhiteSpace(dbSettings!.EmailFromAddress))
        {
            return new ResolvedEmailSettings(
                enabled,
                dbSettings.SmtpHost!,
                dbSettings.SmtpPort ?? _config.GetValue("Email:Smtp:Port", 587),
                dbSettings.SmtpUseSsl,
                dbSettings.EmailFromAddress!,
                dbSettings.EmailFromName ?? "TopZinto ERP",
                dbSettings.SmtpUsername,
                dbSettings.SmtpPassword);
        }

        return new ResolvedEmailSettings(
            enabled,
            _config["Email:Smtp:Host"] ?? "",
            _config.GetValue("Email:Smtp:Port", 587),
            _config.GetValue("Email:Smtp:UseSsl", true),
            _config["Email:FromAddress"] ?? "",
            _config["Email:FromName"] ?? "TopZinto ERP",
            _config["Email:Smtp:Username"],
            _config["Email:Smtp:Password"]);
    }

    private sealed record ResolvedEmailSettings(
        bool Enabled,
        string Host,
        int Port,
        bool UseSsl,
        string FromAddress,
        string FromName,
        string? Username,
        string? Password);
}
