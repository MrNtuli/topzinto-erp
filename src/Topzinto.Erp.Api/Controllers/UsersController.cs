using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Topzinto.Erp.Application.DTOs.Users;
using Topzinto.Erp.Application.Interfaces;

namespace Topzinto.Erp.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly IAuditService _auditService;

    public UsersController(IAuthService authService, IAuditService auditService)
    {
        _authService = authService;
        _auditService = auditService;
    }

    [HttpGet("me")]
    public async Task<IActionResult> GetMyProfile(CancellationToken ct)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var profile = await _authService.GetMyProfileAsync(userId, ct);
        if (profile is null)
            return NotFound(new { message = "User not found." });

        return Ok(profile);
    }

    [HttpPatch("me")]
    public async Task<IActionResult> UpdateMyProfile([FromBody] UpdateMyProfileRequest request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.FirstName) || string.IsNullOrWhiteSpace(request.LastName))
            return BadRequest(new { message = "First name and last name are required." });

        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var profile = await _authService.UpdateMyProfileAsync(userId, request, ct);
        if (profile is null)
            return BadRequest(new { message = "Unable to update profile." });

        await _auditService.LogAsync(
            userId,
            profile.Email,
            "UpdateProfile",
            "Users",
            "User",
            userId.ToString(),
            newValues: $"{profile.FirstName} {profile.LastName}" + (profile.Phone is not null ? $" · {profile.Phone}" : ""),
            ipAddress: HttpContext.Connection.RemoteIpAddress?.ToString(),
            userAgent: Request.Headers.UserAgent.ToString(),
            ct: ct);

        return Ok(profile);
    }
}
