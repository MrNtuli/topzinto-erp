namespace Topzinto.Erp.Application.DTOs.Admin;



public record AuditLogDto(

    Guid Id,

    string UserEmail,

    string Action,

    string Module,

    string EntityType,

    string EntityId,

    string? NewValues,

    string CreatedAt

);



public record UserAdminDto(

    Guid Id,

    string Email,

    string FirstName,

    string LastName,

    string Role,

    string SystemRole,

    bool IsActive,

    bool IsLockedOut,

    string? LastLoginAt

);



public record RoleOptionDto(string Value, string Label);

public record RoleMatrixDto(
    IReadOnlyList<RoleOptionDto> Roles,
    IReadOnlyDictionary<string, string[]> Matrix
);



public record CreateUserRequest(

    string Email,

    string FirstName,

    string LastName,

    string Role,

    string Password

);



public record UpdateUserRequest(

    string FirstName,

    string LastName,

    string Role,

    bool IsActive

);



public record AdminResetPasswordRequest(string NewPassword);


