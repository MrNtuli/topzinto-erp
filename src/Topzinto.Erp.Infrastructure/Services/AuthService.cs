using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Topzinto.Erp.Application.DTOs.Auth;
using Topzinto.Erp.Application.DTOs.Users;
using Topzinto.Erp.Application.Interfaces;
using Topzinto.Erp.Domain.Entities;
using Topzinto.Erp.Infrastructure.Identity;
using Topzinto.Erp.Infrastructure.Persistence;

namespace Topzinto.Erp.Infrastructure.Services;

public class AuthService : IAuthService
{
    public const string SecurityStampClaimType = "sstamp";

    private readonly UserManager<ApplicationUser> _userManager;
    private readonly AppDbContext _db;
    private readonly IConfiguration _configuration;
    private readonly IEmailService _email;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        UserManager<ApplicationUser> userManager,
        AppDbContext db,
        IConfiguration configuration,
        IEmailService email,
        ILogger<AuthService> logger)
    {
        _userManager = userManager;
        _db = db;
        _configuration = configuration;
        _email = email;
        _logger = logger;
    }

    public async Task<LoginResult> LoginAsync(
        LoginRequest request,
        string? ipAddress = null,
        string? userAgent = null,
        CancellationToken ct = default)
    {
        var user = await _userManager.FindByEmailAsync(request.Email.Trim());
        if (user is null)
            return new LoginResult(LoginStatus.InvalidCredentials);

        if (!user.IsActive)
            return new LoginResult(LoginStatus.Inactive);

        if (await _userManager.IsLockedOutAsync(user))
            return new LoginResult(LoginStatus.AccountLocked, LockoutEnd: await _userManager.GetLockoutEndDateAsync(user));

        if (!await _userManager.CheckPasswordAsync(user, request.Password))
        {
            await _userManager.AccessFailedAsync(user);
            if (await _userManager.IsLockedOutAsync(user))
                return new LoginResult(LoginStatus.AccountLocked, LockoutEnd: await _userManager.GetLockoutEndDateAsync(user));

            return new LoginResult(LoginStatus.InvalidCredentials);
        }

        await _userManager.ResetAccessFailedCountAsync(user);

        user.LastLoginAt = DateTime.UtcNow;
        await _userManager.UpdateAsync(user);

        if (await _userManager.GetTwoFactorEnabledAsync(user))
        {
            var mfaToken = GenerateMfaChallengeToken(user);
            return new LoginResult(LoginStatus.MfaRequired, MfaToken: mfaToken);
        }

        var roles = await _userManager.GetRolesAsync(user);
        var role = roles.FirstOrDefault() ?? "Employee";
        var accessToken = GenerateAccessToken(user, role, request.RememberMe);
        var refreshToken = await IssueRefreshTokenAsync(user.Id, request.RememberMe, ipAddress, userAgent, ct);

        return new LoginResult(
            LoginStatus.Success,
            new LoginResponse(
                accessToken,
                refreshToken,
                new UserDto(
                    user.Id.ToString(),
                    user.Email!,
                    user.FirstName,
                    user.LastName,
                    FormatRole(role)
                )));
    }

    public async Task<RefreshTokenResponse?> RefreshAsync(
        RefreshTokenRequest request,
        string? ipAddress = null,
        string? userAgent = null,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(request.RefreshToken))
            return null;

        var tokenHash = HashToken(request.RefreshToken.Trim());
        var stored = await _db.RefreshTokens
            .FirstOrDefaultAsync(t => t.TokenHash == tokenHash, ct);

        if (stored is null || !stored.IsActive)
            return null;

        var user = await _userManager.FindByIdAsync(stored.UserId.ToString());
        if (user is null || !user.IsActive)
        {
            stored.RevokedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync(ct);
            return null;
        }

        stored.RevokedAt = DateTime.UtcNow;
        var newRefreshToken = await IssueRefreshTokenAsync(
            user.Id,
            stored.RememberMe,
            ipAddress,
            userAgent,
            ct,
            replacedTokenHash: tokenHash);

        var roles = await _userManager.GetRolesAsync(user);
        var role = roles.FirstOrDefault() ?? "Employee";
        var accessToken = GenerateAccessToken(user, role, stored.RememberMe);

        await _db.SaveChangesAsync(ct);

        return new RefreshTokenResponse(accessToken, newRefreshToken);
    }

    public async Task LogoutAsync(Guid userId, LogoutRequest request, CancellationToken ct = default)
    {
        if (!string.IsNullOrWhiteSpace(request.RefreshToken))
        {
            var tokenHash = HashToken(request.RefreshToken.Trim());
            var stored = await _db.RefreshTokens
                .FirstOrDefaultAsync(t => t.TokenHash == tokenHash && t.UserId == userId, ct);

            if (stored is not null && stored.RevokedAt is null)
            {
                stored.RevokedAt = DateTime.UtcNow;
                await _db.SaveChangesAsync(ct);
            }

            return;
        }

        await RevokeAllSessionsAsync(userId, ct);
    }

    public async Task RevokeAllSessionsAsync(Guid userId, CancellationToken ct = default)
    {
        var activeTokens = await _db.RefreshTokens
            .Where(t => t.UserId == userId && t.RevokedAt == null && t.ExpiresAt > DateTime.UtcNow)
            .ToListAsync(ct);

        foreach (var token in activeTokens)
            token.RevokedAt = DateTime.UtcNow;

        if (activeTokens.Count > 0)
            await _db.SaveChangesAsync(ct);
    }

    public async Task<(bool Success, string? Error)> ChangePasswordAsync(
        Guid userId,
        ChangePasswordRequest request,
        CancellationToken ct = default)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user is null) return (false, "User not found.");

        if (request.NewPassword == request.CurrentPassword)
            return (false, "New password must be different from the current password.");

        var result = await _userManager.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword);
        if (!result.Succeeded)
            return (false, string.Join(" ", result.Errors.Select(e => e.Description)));

        await _userManager.ResetAccessFailedCountAsync(user);
        await _userManager.UpdateSecurityStampAsync(user);
        await RevokeAllSessionsAsync(userId, ct);
        return (true, null);
    }

    public async Task<UserDto?> UpdateProfileAsync(
        Guid userId,
        UpdateProfileRequest request,
        CancellationToken ct = default)
    {
        var profile = await UpdateMyProfileAsync(
            userId,
            new UpdateMyProfileRequest(request.FirstName, request.LastName, null),
            ct);

        if (profile is null) return null;

        return new UserDto(
            profile.Id,
            profile.Email,
            profile.FirstName,
            profile.LastName,
            profile.Role
        );
    }

    public async Task<UserProfileDto?> GetMyProfileAsync(Guid userId, CancellationToken ct = default)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user is null) return null;

        var roles = await _userManager.GetRolesAsync(user);
        var role = roles.FirstOrDefault() ?? "Employee";

        return MapProfile(user, FormatRole(role));
    }

    public async Task<UserProfileDto?> UpdateMyProfileAsync(
        Guid userId,
        UpdateMyProfileRequest request,
        CancellationToken ct = default)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user is null) return null;

        user.FirstName = request.FirstName.Trim();
        user.LastName = request.LastName.Trim();
        user.PhoneNumber = string.IsNullOrWhiteSpace(request.Phone) ? null : request.Phone.Trim();

        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded) return null;

        var roles = await _userManager.GetRolesAsync(user);
        var role = roles.FirstOrDefault() ?? "Employee";

        return MapProfile(user, FormatRole(role));
    }

    private static UserProfileDto MapProfile(ApplicationUser user, string role) => new(
        user.Id.ToString(),
        user.Email!,
        user.FirstName,
        user.LastName,
        user.PhoneNumber,
        role,
        user.LastLoginAt?.ToString("yyyy-MM-dd HH:mm")
    );

    public async Task<ForgotPasswordResponse> RequestPasswordResetAsync(
        ForgotPasswordRequest request,
        bool includeDevLink,
        CancellationToken ct = default)
    {
        const string message = "If an account exists with that email, reset instructions have been sent.";
        var user = await _userManager.FindByEmailAsync(request.Email.Trim());
        if (user is null || !user.IsActive)
            return new ForgotPasswordResponse(message, null);

        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        var resetLink = BuildResetLink(user.Email!, token);

        try
        {
            await _email.SendAsync(
                user.Email!,
                "Reset your TopZinto ERP password",
                $"""
                <p>Hi {System.Net.WebUtility.HtmlEncode(user.FirstName)},</p>
                <p>We received a request to reset your TopZinto ERP password.</p>
                <p><a href="{resetLink}">Reset Password</a></p>
                <p>If you did not request this, you can ignore this email.</p>
                <p>This link expires when used or after a period of time.</p>
                """,
                ct);
        }
        catch
        {
            // Do not reveal whether the account exists.
        }

        var devLink = includeDevLink && !_email.IsEnabled ? resetLink : null;
        if (devLink is not null)
            _logger.LogInformation("Password reset link (dev, SMTP off) for {Email}: {Link}", user.Email, resetLink);

        return new ForgotPasswordResponse(message, devLink);
    }

    public async Task<(bool Success, string? Error)> ResetPasswordWithTokenAsync(
        ResetPasswordRequest request,
        CancellationToken ct = default)
    {
        var user = await _userManager.FindByEmailAsync(request.Email.Trim());
        if (user is null || !user.IsActive)
            return (false, "Invalid or expired reset link.");

        var result = await _userManager.ResetPasswordAsync(user, request.Token, request.NewPassword);
        if (!result.Succeeded)
        {
            if (result.Errors.Any(e => e.Code is "InvalidToken"))
                return (false, "Invalid or expired reset link.");

            return (false, string.Join(" ", result.Errors.Select(e => e.Description)));
        }

        await _userManager.ResetAccessFailedCountAsync(user);
        await _userManager.UpdateSecurityStampAsync(user);
        await RevokeAllSessionsAsync(user.Id, ct);
        return (true, null);
    }

    public async Task<MfaStatusDto> GetMfaStatusAsync(Guid userId, CancellationToken ct = default)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user is null) return new MfaStatusDto(false);
        return new MfaStatusDto(await _userManager.GetTwoFactorEnabledAsync(user));
    }

    public async Task<MfaSetupDto?> BeginMfaSetupAsync(Guid userId, CancellationToken ct = default)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user is null) return null;

        await _userManager.ResetAuthenticatorKeyAsync(user);
        var key = await _userManager.GetAuthenticatorKeyAsync(user);
        if (string.IsNullOrWhiteSpace(key)) return null;

        return new MfaSetupDto(FormatAuthenticatorKey(key), BuildAuthenticatorUri(user.Email!, key));
    }

    public async Task<(bool Success, string? Error)> EnableMfaAsync(
        Guid userId,
        MfaEnableRequest request,
        CancellationToken ct = default)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user is null) return (false, "User not found.");

        var valid = await _userManager.VerifyTwoFactorTokenAsync(
            user,
            TokenOptions.DefaultAuthenticatorProvider,
            request.Code.Trim());

        if (!valid) return (false, "Invalid verification code.");

        await _userManager.SetTwoFactorEnabledAsync(user, true);
        await _userManager.UpdateSecurityStampAsync(user);
        await RevokeAllSessionsAsync(userId, ct);
        return (true, null);
    }

    public async Task<(bool Success, string? Error)> DisableMfaAsync(
        Guid userId,
        MfaDisableRequest request,
        CancellationToken ct = default)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user is null) return (false, "User not found.");

        if (!await _userManager.GetTwoFactorEnabledAsync(user))
            return (false, "Two-factor authentication is not enabled.");

        var valid = await _userManager.VerifyTwoFactorTokenAsync(
            user,
            TokenOptions.DefaultAuthenticatorProvider,
            request.Code.Trim());

        if (!valid) return (false, "Invalid verification code.");

        await _userManager.SetTwoFactorEnabledAsync(user, false);
        await _userManager.ResetAuthenticatorKeyAsync(user);
        await _userManager.UpdateSecurityStampAsync(user);
        await RevokeAllSessionsAsync(userId, ct);
        return (true, null);
    }

    public async Task<LoginResult> VerifyMfaAsync(
        MfaVerifyRequest request,
        string? ipAddress = null,
        string? userAgent = null,
        CancellationToken ct = default)
    {
        var userId = ValidateMfaChallengeToken(request.MfaToken);
        if (userId is null)
            return new LoginResult(LoginStatus.InvalidCredentials);

        var user = await _userManager.FindByIdAsync(userId.Value.ToString());
        if (user is null || !user.IsActive || !await _userManager.GetTwoFactorEnabledAsync(user))
            return new LoginResult(LoginStatus.InvalidCredentials);

        var valid = await _userManager.VerifyTwoFactorTokenAsync(
            user,
            TokenOptions.DefaultAuthenticatorProvider,
            request.Code.Trim());

        if (!valid)
            return new LoginResult(LoginStatus.InvalidCredentials);

        await _userManager.ResetAccessFailedCountAsync(user);
        user.LastLoginAt = DateTime.UtcNow;
        await _userManager.UpdateAsync(user);

        var roles = await _userManager.GetRolesAsync(user);
        var role = roles.FirstOrDefault() ?? "Employee";
        var accessToken = GenerateAccessToken(user, role, request.RememberMe);
        var refreshToken = await IssueRefreshTokenAsync(user.Id, request.RememberMe, ipAddress, userAgent, ct);

        return new LoginResult(
            LoginStatus.Success,
            new LoginResponse(
                accessToken,
                refreshToken,
                new UserDto(
                    user.Id.ToString(),
                    user.Email!,
                    user.FirstName,
                    user.LastName,
                    FormatRole(role)
                )));
    }

    private string GenerateMfaChallengeToken(ApplicationUser user)
    {
        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_configuration["Jwt:Key"] ?? throw new InvalidOperationException("Jwt:Key not configured")));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<System.Security.Claims.Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new("purpose", "mfa"),
        };

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(5),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private Guid? ValidateMfaChallengeToken(string token)
    {
        try
        {
            var handler = new JwtSecurityTokenHandler();
            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_configuration["Jwt:Key"] ?? throw new InvalidOperationException("Jwt:Key not configured")));

            var principal = handler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = _configuration["Jwt:Issuer"],
                ValidAudience = _configuration["Jwt:Audience"],
                IssuerSigningKey = key,
                ClockSkew = TimeSpan.FromMinutes(1),
            }, out _);

            if (principal.FindFirst("purpose")?.Value != "mfa")
                return null;

            var userId = principal.FindFirstValue(ClaimTypes.NameIdentifier);
            return userId is not null && Guid.TryParse(userId, out var id) ? id : null;
        }
        catch
        {
            return null;
        }
    }

    private static string FormatAuthenticatorKey(string key)
    {
        var trimmed = key.Replace(" ", string.Empty);
        return string.Join(" ", Enumerable.Range(0, trimmed.Length / 4 + (trimmed.Length % 4 == 0 ? 0 : 1))
            .Select(i => trimmed.Substring(i * 4, Math.Min(4, trimmed.Length - i * 4))));
    }

    private static string BuildAuthenticatorUri(string email, string key) =>
        $"otpauth://totp/TopZinto%20ERP:{Uri.EscapeDataString(email)}?secret={key.Replace(" ", string.Empty)}&issuer=TopZinto%20ERP&digits=6";

    private async Task<string> IssueRefreshTokenAsync(
        Guid userId,
        bool rememberMe,
        string? ipAddress,
        string? userAgent,
        CancellationToken ct,
        string? replacedTokenHash = null)
    {
        var plainToken = GenerateRefreshToken();
        var tokenHash = HashToken(plainToken);

        if (replacedTokenHash is not null)
        {
            var replaced = await _db.RefreshTokens
                .FirstOrDefaultAsync(t => t.TokenHash == replacedTokenHash, ct);
            if (replaced is not null)
                replaced.ReplacedByTokenHash = tokenHash;
        }

        var expiry = rememberMe
            ? DateTime.UtcNow.AddDays(_configuration.GetValue("Jwt:RefreshTokenRememberDays", 30))
            : DateTime.UtcNow.AddDays(_configuration.GetValue("Jwt:RefreshTokenSessionDays", 1));

        _db.RefreshTokens.Add(new RefreshToken
        {
            UserId = userId,
            TokenHash = tokenHash,
            ExpiresAt = expiry,
            RememberMe = rememberMe,
            CreatedByIp = ipAddress,
            UserAgent = userAgent,
        });

        await _db.SaveChangesAsync(ct);
        return plainToken;
    }

    private static string GenerateRefreshToken()
    {
        var bytes = new byte[64];
        RandomNumberGenerator.Fill(bytes);
        return Convert.ToBase64String(bytes);
    }

    private static string HashToken(string token)
    {
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(token));
        return Convert.ToBase64String(hash);
    }

    private string BuildResetLink(string email, string token)
    {
        var baseUrl = (_configuration["App:BaseUrl"] ?? "http://localhost:5173").TrimEnd('/');
        var encodedEmail = Uri.EscapeDataString(email);
        var encodedToken = Uri.EscapeDataString(token);
        return $"{baseUrl}/reset-password?email={encodedEmail}&token={encodedToken}";
    }

    private string GenerateAccessToken(ApplicationUser user, string role, bool rememberMe)
    {
        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_configuration["Jwt:Key"] ?? throw new InvalidOperationException("Jwt:Key not configured")));

        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<System.Security.Claims.Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Email, user.Email!),
            new(ClaimTypes.Name, $"{user.FirstName} {user.LastName}"),
            new(ClaimTypes.Role, role),
            new(SecurityStampClaimType, user.SecurityStamp ?? string.Empty),
        };

        var expiry = rememberMe
            ? DateTime.UtcNow.AddDays(_configuration.GetValue("Jwt:RememberMeDays", 7))
            : DateTime.UtcNow.AddHours(_configuration.GetValue("Jwt:SessionHours", 8));

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: expiry,
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private static string FormatRole(string role) => role switch
    {
        "Director" => "Managing Director",
        "OperationsManager" => "Operations Manager",
        "ProjectManager" => "Project Manager",
        "ContractManager" => "Contract Manager",
        "QuantitySurveyor" => "Quantity Surveyor",
        "FleetManager" => "Fleet Manager",
        "EquipmentManager" => "Equipment Manager",
        "SafetyOfficer" => "Safety Officer",
        "StoreController" => "Store Controller",
        "SuperAdmin" => "Super Admin",
        _ => role,
    };
}
