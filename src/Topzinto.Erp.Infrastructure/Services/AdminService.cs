using System.Text;
using Microsoft.AspNetCore.Identity;

using Microsoft.EntityFrameworkCore;

using Topzinto.Erp.Application.DTOs.Admin;

using Topzinto.Erp.Application.Interfaces;

using Topzinto.Erp.Domain.Enums;

using Topzinto.Erp.Infrastructure.Identity;



namespace Topzinto.Erp.Infrastructure.Services;



public class AdminService : IAdminService

{

    private readonly IAuditService _audit;

    private readonly UserManager<ApplicationUser> _userManager;

    public AdminService(
        IAuditService audit,
        UserManager<ApplicationUser> userManager)
    {
        _audit = audit;
        _userManager = userManager;
    }

    public async Task<IReadOnlyList<AuditLogDto>> GetAuditLogsAsync(int count = 100, CancellationToken ct = default)

    {

        var logs = await _audit.GetRecentAsync(count, ct);

        return logs.Select(l => new AuditLogDto(

            l.Id,

            l.UserEmail,

            l.Action,

            l.Module,

            l.EntityType,

            l.EntityId,

            l.NewValues,

            l.CreatedAt.ToString("yyyy-MM-dd HH:mm")

        )).ToList();
    }

    public async Task<byte[]> ExportAuditLogsCsvAsync(int count = 1000, CancellationToken ct = default)
    {
        var logs = await _audit.GetRecentAsync(count, ct);
        var sb = new StringBuilder();
        sb.AppendLine("Timestamp,User Email,Action,Module,Entity Type,Entity ID,Details");
        foreach (var log in logs)
        {
            sb.AppendLine(string.Join(",",
                Csv(log.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss")),
                Csv(log.UserEmail),
                Csv(log.Action),
                Csv(log.Module),
                Csv(log.EntityType),
                Csv(log.EntityId),
                Csv(log.NewValues)));
        }

        return Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(sb.ToString())).ToArray();
    }

    public async Task<IReadOnlyList<UserAdminDto>> GetUsersAsync(CancellationToken ct = default)

    {

        var users = await _userManager.Users.OrderBy(u => u.Email).ToListAsync(ct);

        var result = new List<UserAdminDto>();



        foreach (var user in users)

        {

            var roles = await _userManager.GetRolesAsync(user);

            var systemRole = roles.FirstOrDefault() ?? SystemRoles.Employee;

            var isLockedOut = await _userManager.IsLockedOutAsync(user);

            result.Add(MapUser(user, systemRole, isLockedOut));

        }



        return result;

    }



    public Task<IReadOnlyList<RoleOptionDto>> GetRolesAsync(CancellationToken ct = default) =>

        Task.FromResult<IReadOnlyList<RoleOptionDto>>(

            SystemRoles.All.Select(r => new RoleOptionDto(r, FormatRole(r))).ToList());



    public async Task<(UserAdminDto? User, string? Error)> CreateUserAsync(

        CreateUserRequest request,

        CancellationToken ct = default)

    {

        if (!SystemRoles.All.Contains(request.Role))

            return (null, "Invalid role selected.");



        if (await _userManager.FindByEmailAsync(request.Email.Trim()) is not null)

            return (null, "A user with this email already exists.");



        var user = new ApplicationUser

        {

            Id = Guid.NewGuid(),

            UserName = request.Email.Trim(),

            Email = request.Email.Trim(),

            FirstName = request.FirstName.Trim(),

            LastName = request.LastName.Trim(),

            EmailConfirmed = true,

            IsActive = true,

        };



        var result = await _userManager.CreateAsync(user, request.Password);

        if (!result.Succeeded)

            return (null, string.Join(" ", result.Errors.Select(e => e.Description)));



        await _userManager.AddToRoleAsync(user, request.Role);

        return (MapUser(user, request.Role, false), null);

    }



    public async Task<(UserAdminDto? User, string? Error)> UpdateUserAsync(

        Guid id,

        UpdateUserRequest request,

        Guid actingUserId,

        CancellationToken ct = default)

    {

        if (!SystemRoles.All.Contains(request.Role))

            return (null, "Invalid role selected.");



        if (id == actingUserId && !request.IsActive)

            return (null, "You cannot deactivate your own account.");



        var user = await _userManager.FindByIdAsync(id.ToString());

        if (user is null) return (null, "User not found.");



        user.FirstName = request.FirstName.Trim();

        user.LastName = request.LastName.Trim();

        user.IsActive = request.IsActive;



        var updateResult = await _userManager.UpdateAsync(user);

        if (!updateResult.Succeeded)

            return (null, string.Join(" ", updateResult.Errors.Select(e => e.Description)));



        var currentRoles = await _userManager.GetRolesAsync(user);

        if (!currentRoles.Contains(request.Role))

        {

            if (currentRoles.Count > 0)

                await _userManager.RemoveFromRolesAsync(user, currentRoles);

            await _userManager.AddToRoleAsync(user, request.Role);

        }



        return (MapUser(user, request.Role, await _userManager.IsLockedOutAsync(user)), null);

    }



    public async Task<(bool Success, string? Error)> ResetPasswordAsync(

        Guid id,

        AdminResetPasswordRequest request,

        CancellationToken ct = default)

    {

        var user = await _userManager.FindByIdAsync(id.ToString());

        if (user is null) return (false, "User not found.");



        var token = await _userManager.GeneratePasswordResetTokenAsync(user);

        var result = await _userManager.ResetPasswordAsync(user, token, request.NewPassword);

        if (!result.Succeeded)

            return (false, string.Join(" ", result.Errors.Select(e => e.Description)));



        await _userManager.ResetAccessFailedCountAsync(user);

        return (true, null);

    }



    public async Task<(bool Success, string? Error)> UnlockUserAsync(Guid id, CancellationToken ct = default)

    {

        var user = await _userManager.FindByIdAsync(id.ToString());

        if (user is null) return (false, "User not found.");



        var result = await _userManager.SetLockoutEndDateAsync(user, null);

        if (!result.Succeeded)

            return (false, string.Join(" ", result.Errors.Select(e => e.Description)));



        await _userManager.ResetAccessFailedCountAsync(user);

        return (true, null);

    }



    private static UserAdminDto MapUser(ApplicationUser user, string systemRole, bool isLockedOut) =>

        new(

            user.Id,

            user.Email!,

            user.FirstName,

            user.LastName,

            FormatRole(systemRole),

            systemRole,

            user.IsActive,

            isLockedOut,

            user.LastLoginAt?.ToString("yyyy-MM-dd HH:mm")

        );



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

    private static string Csv(string? value)
    {
        if (string.IsNullOrEmpty(value)) return "";
        if (value.Contains('"') || value.Contains(',') || value.Contains('\n') || value.Contains('\r'))
            return $"\"{value.Replace("\"", "\"\"")}\"";
        return value;
    }
}


