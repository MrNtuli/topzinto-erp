using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken ct)
    {
        var result = await _authService.LoginAsync(request, ct);
        if (result is null)
            return Unauthorized(new { message = "Invalid email or password" });

        await _auditService.LogAsync(
            Guid.Parse(result.User.Id),
            result.User.Email,
            "Login",
            "Auth",
            "User",
            result.User.Id,
            ipAddress: HttpContext.Connection.RemoteIpAddress?.ToString(),
            userAgent: Request.Headers.UserAgent.ToString(),
            ct: ct);

        return Ok(result);
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

        return Ok(new { message = "Password updated successfully." });
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
    public async Task<IActionResult> ForgotPassword(
        [FromBody] ForgotPasswordRequest request,
        [FromServices] IWebHostEnvironment env,
        CancellationToken ct)
    {
        var result = await _authService.RequestPasswordResetAsync(request, env.IsDevelopment(), ct);

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
