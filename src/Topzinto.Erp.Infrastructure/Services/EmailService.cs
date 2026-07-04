using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Topzinto.Erp.Application.Interfaces;

namespace Topzinto.Erp.Infrastructure.Services;

public class EmailService : IEmailService
{
    private readonly IConfiguration _config;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IConfiguration config, ILogger<EmailService> logger)
    {
        _config = config;
        _logger = logger;
    }

    public bool IsEnabled => _config.GetValue("Email:Enabled", false);

    public async Task SendAsync(string to, string subject, string htmlBody, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(to)) return;

        if (!IsEnabled)
        {
            _logger.LogInformation("Email skipped (disabled): To={To} Subject={Subject}", to, subject);
            return;
        }

        var host = _config["Email:Smtp:Host"] ?? throw new InvalidOperationException("Email:Smtp:Host not configured");
        var port = _config.GetValue("Email:Smtp:Port", 587);
        var useSsl = _config.GetValue("Email:Smtp:UseSsl", true);
        var from = _config["Email:FromAddress"] ?? throw new InvalidOperationException("Email:FromAddress not configured");
        var fromName = _config["Email:FromName"] ?? "TopZinto ERP";
        var username = _config["Email:Smtp:Username"];
        var password = _config["Email:Smtp:Password"];

        using var message = new MailMessage
        {
            From = new MailAddress(from, fromName),
            Subject = subject,
            Body = htmlBody,
            IsBodyHtml = true,
        };
        message.To.Add(to);

        using var client = new SmtpClient(host, port)
        {
            EnableSsl = useSsl,
        };

        if (!string.IsNullOrWhiteSpace(username))
            client.Credentials = new NetworkCredential(username, password);

        await client.SendMailAsync(message, ct);
        _logger.LogInformation("Email sent: To={To} Subject={Subject}", to, subject);
    }
}
