using Topzinto.Erp.Domain.Common;
using Topzinto.Erp.Domain.Enums;

namespace Topzinto.Erp.Domain.Entities;

public class ComplianceRecord : BaseEntity
{
    public string Title { get; set; } = string.Empty;
    public ComplianceRecordType Type { get; set; } = ComplianceRecordType.Other;
    public string? EntityType { get; set; }
    public Guid? EntityId { get; set; }
    public Guid? ProjectId { get; set; }
    public Project? Project { get; set; }
    public DateTime IssueDate { get; set; } = DateTime.UtcNow.Date;
    public DateTime? ExpiryDate { get; set; }
    public ComplianceRecordStatus Status { get; set; } = ComplianceRecordStatus.Valid;
    public string? ResponsiblePerson { get; set; }
    public string? Notes { get; set; }
}
