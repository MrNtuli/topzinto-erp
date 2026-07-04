using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Topzinto.Erp.Application.DTOs.Notifications;
using Topzinto.Erp.Application.Interfaces;
using Topzinto.Erp.Domain.Entities;
using Topzinto.Erp.Domain.Enums;
using Topzinto.Erp.Infrastructure.Identity;
using Topzinto.Erp.Infrastructure.Persistence;

namespace Topzinto.Erp.Infrastructure.Services;

public class NotificationService : INotificationService
{
    private static readonly Dictionary<string, string[]> CategoryRoles = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Tender"] = [SystemRoles.Director, SystemRoles.SuperAdmin, SystemRoles.ContractManager, SystemRoles.ProjectManager, SystemRoles.OperationsManager, SystemRoles.Estimator],
        ["Document"] = [SystemRoles.Director, SystemRoles.SuperAdmin, SystemRoles.OperationsManager, SystemRoles.HR, SystemRoles.SafetyOfficer],
        ["Task"] = [SystemRoles.Director, SystemRoles.SuperAdmin, SystemRoles.ProjectManager, SystemRoles.OperationsManager, SystemRoles.Supervisor, SystemRoles.Foreman],
        ["Fleet"] = [SystemRoles.Director, SystemRoles.SuperAdmin, SystemRoles.FleetManager, SystemRoles.EquipmentManager, SystemRoles.OperationsManager],
    };

    private static readonly string[] AdminRoles = [SystemRoles.Director, SystemRoles.SuperAdmin];

    private readonly AppDbContext _db;
    private readonly IEmailService _email;
    private readonly IConfiguration _config;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<NotificationService> _logger;

    public NotificationService(
        AppDbContext db,
        IEmailService email,
        IConfiguration config,
        UserManager<ApplicationUser> userManager,
        ILogger<NotificationService> logger)
    {
        _db = db;
        _email = email;
        _config = config;
        _userManager = userManager;
        _logger = logger;
    }
    public async Task<NotificationSummaryDto> GetSummaryAsync(Guid? userId, CancellationToken ct = default)
    {
        var list = await QueryForUser(userId).ToListAsync(ct);
        return new NotificationSummaryDto(list.Count(n => !n.IsRead), list.Count);
    }

    public async Task<IReadOnlyList<NotificationDto>> GetAllAsync(Guid? userId, bool? unreadOnly = null, CancellationToken ct = default)
    {
        var query = QueryForUser(userId);
        if (unreadOnly == true)
            query = query.Where(n => !n.IsRead);

        var list = await query.OrderByDescending(n => n.CreatedAt).Take(50).ToListAsync(ct);
        return list.Select(Map).ToList();
    }

    public async Task MarkAsReadAsync(Guid id, Guid? userId, CancellationToken ct = default)
    {
        var n = await QueryForUser(userId).FirstOrDefaultAsync(x => x.Id == id, ct);
        if (n is null) return;
        n.IsRead = true;
        n.ReadAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);
    }

    public async Task MarkAllAsReadAsync(Guid? userId, CancellationToken ct = default)
    {
        var list = await QueryForUser(userId).Where(n => !n.IsRead).ToListAsync(ct);
        foreach (var n in list)
        {
            n.IsRead = true;
            n.ReadAt = DateTime.UtcNow;
        }
        await _db.SaveChangesAsync(ct);
    }

    public async Task<int> GenerateSystemAlertsAsync(CancellationToken ct = default)
    {
        var soon = DateTime.UtcNow.Date.AddDays(30);
        var tenderSoon = DateTime.UtcNow.Date.AddDays(14);
        var today = DateTime.UtcNow.Date;

        var alerts = new List<(string Key, string Title, string Message, string Category, string Severity, string? Link)>();

        var tenders = await _db.Tenders
            .Where(t => t.Status == TenderStatus.Preparing || t.Status == TenderStatus.Identified)
            .Where(t => t.ClosingDate.Date <= tenderSoon && t.ClosingDate.Date >= today)
            .ToListAsync(ct);
        foreach (var t in tenders)
            alerts.Add(($"tender:{t.Id}", "Tender closing soon", $"{t.Title} closes {t.ClosingDate:dd MMM yyyy}", "Tender", "Warning", "/tenders"));

        var docs = await _db.Documents
            .Where(d => d.ExpiryDate != null && d.ExpiryDate.Value.Date <= soon && d.ExpiryDate.Value.Date >= today)
            .ToListAsync(ct);
        foreach (var d in docs)
            alerts.Add(($"doc:{d.Id}", "Document expiring", $"{d.Title} expires {d.ExpiryDate:yyyy-MM-dd}", "Document", "Warning", "/documents"));

        var overdueTasks = await _db.ProjectTasks
            .Include(t => t.Project)
            .Where(t => t.Status != ProjectTaskStatus.Completed && t.DueDate != null && t.DueDate.Value.Date < today)
            .Take(10)
            .ToListAsync(ct);
        foreach (var t in overdueTasks)
            alerts.Add(($"task:{t.Id}", "Overdue task", $"{t.Title} on {t.Project.Name}", "Task", "Critical", "/schedule"));

        var vehicles = await _db.Vehicles
            .Where(v => (v.LicenseExpiryDate != null && v.LicenseExpiryDate.Value.Date <= soon) ||
                        (v.InsuranceExpiryDate != null && v.InsuranceExpiryDate.Value.Date <= soon))
            .ToListAsync(ct);
        foreach (var v in vehicles)
            alerts.Add(($"vehicle:{v.Id}", "Fleet compliance", $"{v.RegistrationNumber} has expiring licence or insurance", "Fleet", "Warning", "/fleet"));

        var existingKeys = await _db.Notifications
            .Where(n => n.ReferenceKey != null)
            .Select(n => n.ReferenceKey!)
            .ToListAsync(ct);

        var created = new List<(string Category, string Title, string Message, string? Link)>();
        foreach (var (key, title, message, category, severity, link) in alerts)
        {
            if (existingKeys.Contains(key)) continue;
            _db.Notifications.Add(new Notification
            {
                Title = title,
                Message = message,
                Category = category,
                Severity = severity,
                ReferenceKey = key,
                LinkPath = link,
            });
            created.Add((category, title, message, link));
        }

        if (created.Count == 0)
            return 0;

        await _db.SaveChangesAsync(ct);
        await SendSystemAlertEmailsAsync(created, ct);
        return created.Count;
    }

    private async Task SendSystemAlertEmailsAsync(
        IReadOnlyList<(string Category, string Title, string Message, string? Link)> alerts,
        CancellationToken ct)
    {
        if (!_config.GetValue("Email:SystemAlerts", true))
            return;

        var baseUrl = (_config["App:BaseUrl"] ?? "http://localhost:5173").TrimEnd('/');
        var users = await _userManager.Users.Where(u => u.IsActive).ToListAsync(ct);

        foreach (var user in users)
        {
            if (string.IsNullOrWhiteSpace(user.Email)) continue;

            var roles = await _userManager.GetRolesAsync(user);
            var relevant = alerts.Where(a => ShouldReceiveAlert(roles, a.Category)).ToList();
            if (relevant.Count == 0) continue;

            var items = string.Join(
                "",
                relevant.Select(a =>
                    $"<li><strong>{System.Net.WebUtility.HtmlEncode(a.Title)}</strong>: {System.Net.WebUtility.HtmlEncode(a.Message)}" +
                    (a.Link is null ? "" : $" — <a href=\"{baseUrl}{a.Link}\">View</a>") +
                    "</li>"));

            try
            {
                await _email.SendAsync(
                    user.Email,
                    $"TopZinto ERP — {relevant.Count} new alert(s)",
                    $"""
                    <p>Hi {System.Net.WebUtility.HtmlEncode(user.FirstName)},</p>
                    <p>New items need your attention:</p>
                    <ul>{items}</ul>
                    <p><a href="{baseUrl}/notifications">Open Notifications</a></p>
                    """,
                    ct);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to send system alert email to {Email}", user.Email);
            }
        }
    }

    private static bool ShouldReceiveAlert(IList<string> roles, string category)
    {
        if (roles.Any(r => AdminRoles.Contains(r)))
            return true;

        if (!CategoryRoles.TryGetValue(category, out var allowed))
            return false;

        return roles.Any(r => allowed.Contains(r));
    }
    private IQueryable<Notification> QueryForUser(Guid? userId) =>
        _db.Notifications.Where(n => n.UserId == null || n.UserId == userId);

    private static NotificationDto Map(Notification n) => new(
        n.Id, n.Title, n.Message, n.Category, n.Severity, n.LinkPath,
        n.IsRead, n.CreatedAt.ToString("yyyy-MM-dd HH:mm")
    );
}

public class CompanySettingsService : ICompanySettingsService
{
    private readonly AppDbContext _db;
    private readonly IAuditService _audit;
    private static readonly Guid SettingsId = Guid.Parse("00000000-0000-0000-0000-000000000001");

    public CompanySettingsService(AppDbContext db, IAuditService audit)
    {
        _db = db;
        _audit = audit;
    }

    public async Task<CompanySettingsDto> GetAsync(CancellationToken ct = default)
    {
        var s = await EnsureExistsAsync(ct);
        return Map(s);
    }

    public async Task<CompanySettingsDto> UpdateAsync(UpdateCompanySettingsRequest request, Guid? userId, CancellationToken ct = default)
    {
        var s = await EnsureExistsAsync(ct);
        s.CompanyName = request.CompanyName;
        s.Tagline = request.Tagline;
        s.Address = request.Address;
        s.City = request.City;
        s.Province = request.Province;
        s.Phone = request.Phone;
        s.Email = request.Email;
        s.VatNumber = request.VatNumber;
        s.CidbNumber = request.CidbNumber;
        s.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);
        await _audit.LogAsync(userId, "", "Update", "Admin", "CompanySettings", s.Id.ToString(), newValues: s.CompanyName, ct: ct);
        return Map(s);
    }

    private async Task<CompanySetting> EnsureExistsAsync(CancellationToken ct)
    {
        var s = await _db.CompanySettings.FindAsync([SettingsId], ct);
        if (s is not null) return s;
        s = new CompanySetting
        {
            Id = SettingsId,
            Address = "Durban, KwaZulu-Natal",
            City = "Durban",
            Province = "KZN",
            Phone = "+27 31 000 0000",
            Email = "info@topzinto.com",
            VatNumber = "4123456789",
            CidbNumber = "7CE",
        };
        _db.CompanySettings.Add(s);
        await _db.SaveChangesAsync(ct);
        return s;
    }

    private static CompanySettingsDto Map(CompanySetting s) => new(
        s.CompanyName, s.Tagline, s.Address, s.City, s.Province,
        s.Phone, s.Email, s.VatNumber, s.CidbNumber
    );
}

public class ScheduledSystemAlertService : BackgroundService
{
    private readonly IServiceProvider _services;
    private readonly IConfiguration _config;
    private readonly ILogger<ScheduledSystemAlertService> _logger;

    public ScheduledSystemAlertService(
        IServiceProvider services,
        IConfiguration config,
        ILogger<ScheduledSystemAlertService> logger)
    {
        _services = services;
        _config = config;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _services.CreateScope();
                var notifier = scope.ServiceProvider.GetRequiredService<INotificationService>();
                var count = await notifier.GenerateSystemAlertsAsync(stoppingToken);
                if (count > 0)
                    _logger.LogInformation("System alert scan created {Count} new notification(s).", count);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "System alert scan failed.");
            }

            var intervalHours = Math.Max(1, _config.GetValue("Notifications:ScanIntervalHours", 6));
            await Task.Delay(TimeSpan.FromHours(intervalHours), stoppingToken);
        }
    }
}