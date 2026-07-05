using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Topzinto.Erp.Application.DTOs.Auth;
using Topzinto.Erp.Application.Interfaces;
using Topzinto.Erp.Infrastructure.Identity;

namespace Topzinto.Erp.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IConfiguration _configuration;
    private readonly IEmailService _email;

    public AuthService(
        UserManager<ApplicationUser> userManager,
        IConfiguration configuration,
        IEmailService email)
    {
        _userManager = userManager;
        _configuration = configuration;
        _email = email;
    }

    public async Task<LoginResult> LoginAsync(LoginRequest request, CancellationToken ct = default)
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

        var roles = await _userManager.GetRolesAsync(user);
        var role = roles.FirstOrDefault() ?? "Employee";
        var token = GenerateToken(user, role);

        return new LoginResult(
            LoginStatus.Success,
            new LoginResponse(
                token,
                new UserDto(
                    user.Id.ToString(),
                    user.Email!,
                    user.FirstName,
                    user.LastName,
                    FormatRole(role)
                )));
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
        return (true, null);
    }

    public async Task<UserDto?> UpdateProfileAsync(
        Guid userId,
        UpdateProfileRequest request,
        CancellationToken ct = default)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user is null) return null;

        user.FirstName = request.FirstName.Trim();
        user.LastName = request.LastName.Trim();
        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded) return null;

        var roles = await _userManager.GetRolesAsync(user);
        var role = roles.FirstOrDefault() ?? "Employee";

        return new UserDto(
            user.Id.ToString(),
            user.Email!,
            user.FirstName,
            user.LastName,
            FormatRole(role)
        );
    }

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
            return (false, string.Join(" ", result.Errors.Select(e => e.Description)));

        await _userManager.ResetAccessFailedCountAsync(user);
        return (true, null);
    }

    private string BuildResetLink(string email, string token)
    {
        var baseUrl = (_configuration["App:BaseUrl"] ?? "http://localhost:5173").TrimEnd('/');
        var encodedEmail = Uri.EscapeDataString(email);
        var encodedToken = Uri.EscapeDataString(token);
        return $"{baseUrl}/reset-password?email={encodedEmail}&token={encodedToken}";
    }

    private string GenerateToken(ApplicationUser user, string role)
    {
        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_configuration["Jwt:Key"] ?? throw new InvalidOperationException("Jwt:Key not configured")));

        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Email, user.Email!),
            new(ClaimTypes.Name, $"{user.FirstName} {user.LastName}"),
            new(ClaimTypes.Role, role),
        };

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddHours(8),
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
