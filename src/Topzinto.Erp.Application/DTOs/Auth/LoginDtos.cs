namespace Topzinto.Erp.Application.DTOs.Auth;

public record LoginRequest(string Email, string Password, bool RememberMe = false);

public enum LoginStatus
{
    Success,
    InvalidCredentials,
    AccountLocked,
    Inactive,
}

public record LoginResult(
    LoginStatus Status,
    LoginResponse? Response = null,
    DateTimeOffset? LockoutEnd = null);

public record LoginResponse(string AccessToken, UserDto User);

public record UserDto(
    string Id,
    string Email,
    string FirstName,
    string LastName,
    string Role
);

public record ChangePasswordRequest(string CurrentPassword, string NewPassword);

public record UpdateProfileRequest(string FirstName, string LastName);

public record ForgotPasswordRequest(string Email);

public record ResetPasswordRequest(string Email, string Token, string NewPassword);

public record ForgotPasswordResponse(string Message, string? DevResetLink);
