using Topzinto.Erp.Domain.Common;
using Topzinto.Erp.Domain.Enums;

namespace Topzinto.Erp.Domain.Entities;

public class Client : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public ClientType Type { get; set; } = ClientType.Private;
    public string? RegistrationNumber { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? Province { get; set; }
    public string? Notes { get; set; }

    public ICollection<ClientContact> Contacts { get; set; } = [];
    public ICollection<Project> Projects { get; set; } = [];
}

public class ClientContact : BaseEntity
{
    public Guid ClientId { get; set; }
    public Client Client { get; set; } = null!;
    public string Name { get; set; } = string.Empty;
    public string? Title { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public bool IsPrimary { get; set; }
}
