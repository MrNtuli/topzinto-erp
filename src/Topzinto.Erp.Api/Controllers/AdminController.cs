using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Topzinto.Erp.Application.DTOs.Admin;
using Topzinto.Erp.Application.DTOs.Notifications;
using Topzinto.Erp.Application.Interfaces;
using Topzinto.Erp.Api.Authorization;

namespace Topzinto.Erp.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AdminController : ControllerBase
{
    private readonly IAdminService _service;
    private readonly IBackupService _backup;
    private readonly IAppCacheInvalidator _cacheInvalidator;
    private readonly IAuditService _audit;
    private readonly ICompanySettingsService _companySettings;

    public AdminController(
        IAdminService service,
        IBackupService backup,
        IAppCacheInvalidator cacheInvalidator,
        IAuditService audit,
        ICompanySettingsService companySettings)
    {
        _service = service;
        _backup = backup;
        _cacheInvalidator = cacheInvalidator;
        _audit = audit;
        _companySettings = companySettings;
    }

    private Guid ActingUserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    private string ActingUserEmail => User.FindFirstValue(ClaimTypes.Email) ?? "";

    [HttpGet("audit")]
    public async Task<IActionResult> GetAuditLogs([FromQuery] int count = 100, CancellationToken ct = default) =>
        Ok(await _service.GetAuditLogsAsync(count, ct));

    [HttpGet("audit/export")]
    [Authorize(Roles = "Director,SuperAdmin")]
    public async Task<IActionResult> ExportAuditLogs([FromQuery] int count = 1000, CancellationToken ct = default) =>
        File(
            await _service.ExportAuditLogsCsvAsync(count, ct),
            "text/csv",
            $"audit_log_{DateTime.UtcNow:yyyyMMdd}.csv");

    [HttpGet("backups")]
    public IActionResult GetBackups() => Ok(_backup.GetStatus());

    [HttpPost("backup")]
    public async Task<IActionResult> CreateBackup(CancellationToken ct)
    {
        var fileName = await _backup.CreateBackupAsync(ct);
        return Ok(new { fileName, message = "Backup created successfully." });
    }

    [HttpGet("backups/{fileName}/download")]
    public IActionResult DownloadBackup(string fileName)
    {
        var file = _backup.OpenBackup(fileName);
        if (file is null) return NotFound();
        return File(file.Value.Stream, file.Value.ContentType, file.Value.FileName);
    }

    [HttpPost("cache/invalidate")]
    public async Task<IActionResult> InvalidateCache(CancellationToken ct)
    {
        await _cacheInvalidator.InvalidateDashboardAndReportsAsync(ct);
        return Ok(new { message = "Dashboard and reports cache cleared." });
    }

    [HttpPost("alerts/scan")]
    [Authorize(Roles = "Director,SuperAdmin")]
    public async Task<IActionResult> ScanSystemAlerts(
        [FromServices] INotificationService notifications,
        CancellationToken ct)
    {
        var count = await notifications.GenerateSystemAlertsAsync(ct);
        return Ok(new { count, message = count > 0 ? $"{count} new alert(s) created." : "No new alerts." });
    }

    [HttpGet("users")]
    [Authorize(Roles = "Director,SuperAdmin")]
    public async Task<IActionResult> GetUsers(CancellationToken ct) =>
        Ok(await _service.GetUsersAsync(ct));

    [HttpGet("roles")]
    [Authorize(Roles = "Director,SuperAdmin")]
    public async Task<IActionResult> GetRoles(CancellationToken ct) =>
        Ok(await _service.GetRolesAsync(ct));

    [HttpGet("roles/matrix")]
    [Authorize(Roles = "Director,SuperAdmin")]
    public async Task<IActionResult> GetRoleMatrix(CancellationToken ct)
    {
        var roles = await _service.GetRolesAsync(ct);
        return Ok(new RoleMatrixDto(roles, ModuleRoleMatrix.BuildAccessMatrix()));
    }

    [HttpGet("email-settings")]
    [Authorize(Roles = "Director,SuperAdmin")]
    public async Task<IActionResult> GetEmailSettings(CancellationToken ct) =>
        Ok(await _companySettings.GetEmailSettingsAsync(ct));

    [HttpPut("email-settings")]
    [Authorize(Roles = "Director,SuperAdmin")]
    public async Task<IActionResult> UpdateEmailSettings([FromBody] UpdateEmailSettingsRequest request, CancellationToken ct)
    {
        var userId = Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : (Guid?)null;
        return Ok(await _companySettings.UpdateEmailSettingsAsync(request, userId, ct));
    }

    [HttpPost("email/test")]
    [Authorize(Roles = "Director,SuperAdmin")]
    public async Task<IActionResult> TestEmail([FromBody] TestEmailRequest request, CancellationToken ct)
    {
        var message = await _companySettings.TestEmailAsync(request.ToEmail, ct);
        return Ok(new { message });
    }

    [HttpPost("users")]
    [Authorize(Roles = "Director,SuperAdmin")]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserRequest request, CancellationToken ct)
    {
        var (user, error) = await _service.CreateUserAsync(request, ct);
        if (user is null)
            return BadRequest(new { message = error ?? "Unable to create user." });

        await _audit.LogAsync(
            ActingUserId,
            ActingUserEmail,
            "CreateUser",
            "Admin",
            "User",
            user.Id.ToString(),
            newValues: $"{user.Email} · {user.Role}",
            ipAddress: HttpContext.Connection.RemoteIpAddress?.ToString(),
            userAgent: Request.Headers.UserAgent.ToString(),
            ct: ct);

        return Ok(user);
    }

    [HttpPut("users/{id:guid}")]
    [Authorize(Roles = "Director,SuperAdmin")]
    public async Task<IActionResult> UpdateUser(Guid id, [FromBody] UpdateUserRequest request, CancellationToken ct)
    {
        var (user, error) = await _service.UpdateUserAsync(id, request, ActingUserId, ct);
        if (user is null)
            return BadRequest(new { message = error ?? "Unable to update user." });

        await _audit.LogAsync(
            ActingUserId,
            ActingUserEmail,
            "UpdateUser",
            "Admin",
            "User",
            user.Id.ToString(),
            newValues: $"{user.Email} · {user.Role} · Active={user.IsActive}",
            ipAddress: HttpContext.Connection.RemoteIpAddress?.ToString(),
            userAgent: Request.Headers.UserAgent.ToString(),
            ct: ct);

        return Ok(user);
    }

    [HttpPost("users/{id:guid}/reset-password")]
    [Authorize(Roles = "Director,SuperAdmin")]
    public async Task<IActionResult> ResetPassword(Guid id, [FromBody] AdminResetPasswordRequest request, CancellationToken ct)
    {
        var (success, error) = await _service.ResetPasswordAsync(id, request, ct);
        if (!success)
            return BadRequest(new { message = error ?? "Unable to reset password." });

        await _audit.LogAsync(
            ActingUserId,
            ActingUserEmail,
            "ResetPassword",
            "Admin",
            "User",
            id.ToString(),
            ipAddress: HttpContext.Connection.RemoteIpAddress?.ToString(),
            userAgent: Request.Headers.UserAgent.ToString(),
            ct: ct);

        return Ok(new { message = "Password reset successfully." });
    }

    [HttpPost("users/{id:guid}/unlock")]
    [Authorize(Roles = "Director,SuperAdmin")]
    public async Task<IActionResult> UnlockUser(Guid id, CancellationToken ct)
    {
        var (success, error) = await _service.UnlockUserAsync(id, ct);
        if (!success)
            return BadRequest(new { message = error ?? "Unable to unlock user." });

        await _audit.LogAsync(
            ActingUserId,
            ActingUserEmail,
            "UnlockUser",
            "Admin",
            "User",
            id.ToString(),
            ipAddress: HttpContext.Connection.RemoteIpAddress?.ToString(),
            userAgent: Request.Headers.UserAgent.ToString(),
            ct: ct);

        return Ok(new { message = "Account unlocked successfully." });
    }
}
