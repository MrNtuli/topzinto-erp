using Topzinto.Erp.Application.DTOs.Auth;
using Topzinto.Erp.Application.DTOs.Users;

namespace Topzinto.Erp.Application.Interfaces;

public interface IAuthService
{
    Task<LoginResult> LoginAsync(LoginRequest request, string? ipAddress = null, string? userAgent = null, CancellationToken ct = default);
    Task<RefreshTokenResponse?> RefreshAsync(RefreshTokenRequest request, string? ipAddress = null, string? userAgent = null, CancellationToken ct = default);
    Task LogoutAsync(Guid userId, LogoutRequest request, CancellationToken ct = default);
    Task RevokeAllSessionsAsync(Guid userId, CancellationToken ct = default);
    Task<(bool Success, string? Error)> ChangePasswordAsync(Guid userId, ChangePasswordRequest request, CancellationToken ct = default);
    Task<UserDto?> UpdateProfileAsync(Guid userId, UpdateProfileRequest request, CancellationToken ct = default);
    Task<UserProfileDto?> GetMyProfileAsync(Guid userId, CancellationToken ct = default);
    Task<UserProfileDto?> UpdateMyProfileAsync(Guid userId, UpdateMyProfileRequest request, CancellationToken ct = default);
    Task<ForgotPasswordResponse> RequestPasswordResetAsync(ForgotPasswordRequest request, bool includeDevLink, CancellationToken ct = default);
    Task<(bool Success, string? Error)> ResetPasswordWithTokenAsync(ResetPasswordRequest request, CancellationToken ct = default);
    Task<MfaStatusDto> GetMfaStatusAsync(Guid userId, CancellationToken ct = default);
    Task<MfaSetupDto?> BeginMfaSetupAsync(Guid userId, CancellationToken ct = default);
    Task<(bool Success, string? Error)> EnableMfaAsync(Guid userId, MfaEnableRequest request, CancellationToken ct = default);
    Task<(bool Success, string? Error)> DisableMfaAsync(Guid userId, MfaDisableRequest request, CancellationToken ct = default);
    Task<LoginResult> VerifyMfaAsync(MfaVerifyRequest request, string? ipAddress = null, string? userAgent = null, CancellationToken ct = default);
}
