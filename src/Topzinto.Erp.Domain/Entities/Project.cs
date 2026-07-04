using Topzinto.Erp.Domain.Common;
using Topzinto.Erp.Domain.Enums;

namespace Topzinto.Erp.Domain.Entities;

public class Project : BaseEntity
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public Guid ClientId { get; set; }
    public Client Client { get; set; } = null!;
    public Guid? TenderId { get; set; }
    public Tender? Tender { get; set; }
    public Contract? Contract { get; set; }
    public ProjectStatus Status { get; set; } = ProjectStatus.Planned;
    public int Progress { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public decimal ContractValue { get; set; }
    public decimal Budget { get; set; }
    public string? Description { get; set; }
    public string? SiteLocation { get; set; }
}
