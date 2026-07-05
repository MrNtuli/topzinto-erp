using Topzinto.Erp.Application.DTOs.Auth;

namespace Topzinto.Erp.Application.Interfaces;

public interface IAuthService
{
    Task<LoginResult> LoginAsync(LoginRequest request, CancellationToken ct = default);
    Task<(bool Success, string? Error)> ChangePasswordAsync(Guid userId, ChangePasswordRequest request, CancellationToken ct = default);
    Task<UserDto?> UpdateProfileAsync(Guid userId, UpdateProfileRequest request, CancellationToken ct = default);
    Task<ForgotPasswordResponse> RequestPasswordResetAsync(ForgotPasswordRequest request, bool includeDevLink, CancellationToken ct = default);
    Task<(bool Success, string? Error)> ResetPasswordWithTokenAsync(ResetPasswordRequest request, CancellationToken ct = default);
}
