using Topzinto.Erp.Application.DTOs.Admin;



namespace Topzinto.Erp.Application.Interfaces;



public interface IAdminService

{

    Task<IReadOnlyList<AuditLogDto>> GetAuditLogsAsync(int count = 100, CancellationToken ct = default);
    Task<byte[]> ExportAuditLogsCsvAsync(int count = 1000, CancellationToken ct = default);

    Task<IReadOnlyList<UserAdminDto>> GetUsersAsync(CancellationToken ct = default);

    Task<IReadOnlyList<RoleOptionDto>> GetRolesAsync(CancellationToken ct = default);

    Task<(UserAdminDto? User, string? Error)> CreateUserAsync(CreateUserRequest request, CancellationToken ct = default);

    Task<(UserAdminDto? User, string? Error)> UpdateUserAsync(Guid id, UpdateUserRequest request, Guid actingUserId, CancellationToken ct = default);

    Task<(bool Success, string? Error)> ResetPasswordAsync(Guid id, AdminResetPasswordRequest request, CancellationToken ct = default);

    Task<(bool Success, string? Error)> UnlockUserAsync(Guid id, CancellationToken ct = default);

}


