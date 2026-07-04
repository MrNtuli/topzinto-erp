namespace Topzinto.Erp.Application.DTOs.Clients;

public record ClientDto(
    Guid Id,
    string Name,
    string Type,
    string? City,
    string? Province,
    int ProjectCount,
    string? PrimaryContact
);

public record ClientDetailDto(
    Guid Id,
    string Name,
    string Type,
    string? RegistrationNumber,
    string? Address,
    string? City,
    string? Province,
    string? Notes,
    IReadOnlyList<ClientContactDto> Contacts
);

public record ClientContactDto(Guid Id, string Name, string? Title, string? Phone, string? Email, bool IsPrimary);

public record CreateClientRequest(
    string Name,
    string Type,
    string? RegistrationNumber,
    string? Address,
    string? City,
    string? Province,
    string? Notes,
    IReadOnlyList<CreateClientContactRequest>? Contacts
);

public record CreateClientContactRequest(string Name, string? Title, string? Phone, string? Email, bool IsPrimary);

public record UpdateClientRequest(
    string Name,
    string Type,
    string? RegistrationNumber,
    string? Address,
    string? City,
    string? Province,
    string? Notes
);
