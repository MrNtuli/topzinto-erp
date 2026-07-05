namespace Topzinto.Erp.Application.DTOs.Users;

public record UserProfileDto(
    string Id,
    string Email,
    string FirstName,
    string LastName,
    string? Phone,
    string Role,
    string? LastLoginAt
);

public record UpdateMyProfileRequest(string FirstName, string LastName, string? Phone);
