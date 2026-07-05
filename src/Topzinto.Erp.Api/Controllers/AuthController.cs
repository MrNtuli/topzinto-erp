using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Topzinto.Erp.Application.DTOs.Auth;
using Topzinto.Erp.Application.Interfaces;

namespace Topzinto.Erp.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly IAuditService _auditService;

    public AuthController(IAuthService authService, IAuditService auditService)
    {
        _authService = authService;
        _auditService = auditService;
    }

    [HttpPost("login")]
    [AllowAnonymous]
    [EnableRateLimiting("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken ct)
    {
        var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
        var userAgent = Request.Headers.UserAgent.ToString();
        var result = await _authService.LoginAsync(request, ip, userAgent, ct);

        return result.Status switch
        {
            LoginStatus.Success => await LogLoginSuccess(result.Response!, ct),
            LoginStatus.MfaRequired => Ok(new MfaChallengeResponse(
                result.MfaToken!,
                "Enter the 6-digit code from your authenticator app.")),
            LoginStatus.Inactive => Unauthorized(new { message = "Your account is inactive. Contact an administrator.", code = "inactive" }),
            LoginStatus.AccountLocked => Unauthorized(new
            {
                message = FormatLockoutMessage(result.LockoutEnd),
                code = "account_locked",
                lockoutEnd = result.LockoutEnd,
            }),
            _ => Unauthorized(new { message = "Invalid email or password", code = "invalid_credentials" }),
        };
    }

    private async Task<IActionResult> LogLoginSuccess(LoginResponse response, CancellationToken ct)
    {
        await _auditService.LogAsync(
            Guid.Parse(response.User.Id),
            response.User.Email,
            "Login",
            "Auth",
            "User",
            response.User.Id,
            ipAddress: HttpContext.Connection.RemoteIpAddress?.ToString(),
            userAgent: Request.Headers.UserAgent.ToString(),
            ct: ct);

        return Ok(response);
    }

    private static string FormatLockoutMessage(DateTimeOffset? lockoutEnd)
    {
        if (lockoutEnd is null || lockoutEnd <= DateTimeOffset.UtcNow)
            return "Account temporarily locked due to too many failed attempts. Try again later.";

        var minutes = Math.Max(1, (int)Math.Ceiling((lockoutEnd.Value - DateTimeOffset.UtcNow).TotalMinutes));
        return $"Account locked after too many failed attempts. Try again in about {minutes} minute(s).";
    }

    [HttpGet("me")]
    [Authorize]
    public IActionResult Me()
    {
        var id = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var email = User.FindFirstValue(ClaimTypes.Email);
        var name = User.FindFirstValue(ClaimTypes.Name);
        var role = User.FindFirstValue(ClaimTypes.Role);

        var parts = name?.Split(' ', 2) ?? ["", ""];
        return Ok(new UserDto(id!, email!, parts[0], parts.ElementAtOrDefault(1) ?? "", role ?? ""));
    }

    [HttpPost("change-password")]
    [Authorize]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.CurrentPassword) || string.IsNullOrWhiteSpace(request.NewPassword))
            return BadRequest(new { message = "Current password and new password are required." });

        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var (success, error) = await _authService.ChangePasswordAsync(userId, request, ct);
        if (!success)
            return BadRequest(new { message = error ?? "Password change failed." });

        await _auditService.LogAsync(
            userId,
            User.FindFirstValue(ClaimTypes.Email) ?? "",
            "ChangePassword",
            "Auth",
            "User",
            userId.ToString(),
            ipAddress: HttpContext.Connection.RemoteIpAddress?.ToString(),
            userAgent: Request.Headers.UserAgent.ToString(),
            ct: ct);

        return Ok(new { message = "Password updated successfully. Other sessions have been signed out." });
    }

    [HttpPost("refresh")]
    [AllowAnonymous]
    public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequest request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.RefreshToken))
            return Unauthorized(new { message = "Invalid or expired refresh token.", code = "invalid_refresh_token" });

        var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
        var userAgent = Request.Headers.UserAgent.ToString();
        var result = await _authService.RefreshAsync(request, ip, userAgent, ct);
        if (result is null)
            return Unauthorized(new { message = "Invalid or expired refresh token.", code = "invalid_refresh_token" });

        return Ok(result);
    }

    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout([FromBody] LogoutRequest request, CancellationToken ct)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        await _authService.LogoutAsync(userId, request, ct);

        await _auditService.LogAsync(
            userId,
            User.FindFirstValue(ClaimTypes.Email) ?? "",
            "Logout",
            "Auth",
            "User",
            userId.ToString(),
            ipAddress: HttpContext.Connection.RemoteIpAddress?.ToString(),
            userAgent: Request.Headers.UserAgent.ToString(),
            ct: ct);

        return Ok(new { message = "Signed out successfully." });
    }

    [HttpGet("mfa/status")]
    [Authorize]
    public async Task<IActionResult> GetMfaStatus(CancellationToken ct)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        return Ok(await _authService.GetMfaStatusAsync(userId, ct));
    }

    [HttpPost("mfa/setup")]
    [Authorize]
    public async Task<IActionResult> BeginMfaSetup(CancellationToken ct)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var setup = await _authService.BeginMfaSetupAsync(userId, ct);
        if (setup is null) return BadRequest(new { message = "Unable to start MFA setup." });
        return Ok(setup);
    }

    [HttpPost("mfa/enable")]
    [Authorize]
    public async Task<IActionResult> EnableMfa([FromBody] MfaEnableRequest request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Code))
            return BadRequest(new { message = "Verification code is required." });

        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var (success, error) = await _authService.EnableMfaAsync(userId, request, ct);
        if (!success) return BadRequest(new { message = error ?? "Unable to enable MFA." });

        await _auditService.LogAsync(
            userId,
            User.FindFirstValue(ClaimTypes.Email) ?? "",
            "EnableMfa",
            "Auth",
            "User",
            userId.ToString(),
            ipAddress: HttpContext.Connection.RemoteIpAddress?.ToString(),
            userAgent: Request.Headers.UserAgent.ToString(),
            ct: ct);

        return Ok(new { message = "Two-factor authentication enabled." });
    }

    [HttpPost("mfa/disable")]
    [Authorize]
    public async Task<IActionResult> DisableMfa([FromBody] MfaDisableRequest request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Code))
            return BadRequest(new { message = "Verification code is required." });

        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var (success, error) = await _authService.DisableMfaAsync(userId, request, ct);
        if (!success) return BadRequest(new { message = error ?? "Unable to disable MFA." });

        await _auditService.LogAsync(
            userId,
            User.FindFirstValue(ClaimTypes.Email) ?? "",
            "DisableMfa",
            "Auth",
            "User",
            userId.ToString(),
            ipAddress: HttpContext.Connection.RemoteIpAddress?.ToString(),
            userAgent: Request.Headers.UserAgent.ToString(),
            ct: ct);

        return Ok(new { message = "Two-factor authentication disabled." });
    }

    [HttpPost("mfa/verify")]
    [AllowAnonymous]
    public async Task<IActionResult> VerifyMfa([FromBody] MfaVerifyRequest request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.MfaToken) || string.IsNullOrWhiteSpace(request.Code))
            return BadRequest(new { message = "MFA token and verification code are required." });

        var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
        var userAgent = Request.Headers.UserAgent.ToString();
        var result = await _authService.VerifyMfaAsync(request, ip, userAgent, ct);

        if (result.Status != LoginStatus.Success || result.Response is null)
            return Unauthorized(new { message = "Invalid verification code.", code = "invalid_mfa_code" });

        return await LogLoginSuccess(result.Response, ct);
    }

    [HttpPut("profile")]
    [Authorize]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequest request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.FirstName) || string.IsNullOrWhiteSpace(request.LastName))
            return BadRequest(new { message = "First name and last name are required." });

        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var user = await _authService.UpdateProfileAsync(userId, request, ct);
        if (user is null)
            return BadRequest(new { message = "Unable to update profile." });

        await _auditService.LogAsync(
            userId,
            user.Email,
            "UpdateProfile",
            "Auth",
            "User",
            userId.ToString(),
            newValues: $"{user.FirstName} {user.LastName}",
            ipAddress: HttpContext.Connection.RemoteIpAddress?.ToString(),
            userAgent: Request.Headers.UserAgent.ToString(),
            ct: ct);

        return Ok(user);
    }

    [HttpPost("forgot-password")]
    [AllowAnonymous]
    [EnableRateLimiting("forgot-password")]
    public async Task<IActionResult> ForgotPassword(
        [FromBody] ForgotPasswordRequest request,
        [FromServices] IWebHostEnvironment env,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Email))
            return Ok(new ForgotPasswordResponse(
                "If an account exists with that email, reset instructions have been sent.",
                null));

        var includeDevLink = env.IsDevelopment() || env.EnvironmentName == "Testing";
        var result = await _authService.RequestPasswordResetAsync(request, includeDevLink, ct);

        await _auditService.LogAsync(
            null,
            request.Email.Trim(),
            "ForgotPassword",
            "Auth",
            "User",
            request.Email.Trim(),
            ipAddress: HttpContext.Connection.RemoteIpAddress?.ToString(),
            userAgent: Request.Headers.UserAgent.ToString(),
            ct: ct);

        return Ok(result);
    }

    [HttpPost("reset-password")]
    [AllowAnonymous]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Email)
            || string.IsNullOrWhiteSpace(request.Token)
            || string.IsNullOrWhiteSpace(request.NewPassword))
            return BadRequest(new { message = "Email, token, and new password are required." });

        var (success, error) = await _authService.ResetPasswordWithTokenAsync(request, ct);
        if (!success)
            return BadRequest(new { message = error ?? "Unable to reset password." });

        await _auditService.LogAsync(
            null,
            request.Email.Trim(),
            "ResetPassword",
            "Auth",
            "User",
            request.Email.Trim(),
            ipAddress: HttpContext.Connection.RemoteIpAddress?.ToString(),
            userAgent: Request.Headers.UserAgent.ToString(),
            ct: ct);

        return Ok(new { message = "Password reset successfully. You can sign in now." });
    }
}
