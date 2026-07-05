using Topzinto.Erp.Domain.Common;
using Topzinto.Erp.Domain.Enums;

namespace Topzinto.Erp.Domain.Entities;

public class SafetyIncident : BaseEntity
{
    public Guid ProjectId { get; set; }
    public Project Project { get; set; } = null!;
    public DateTime IncidentDate { get; set; } = DateTime.UtcNow.Date;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public SafetyIncidentSeverity Severity { get; set; } = SafetyIncidentSeverity.Medium;
    public SafetyIncidentStatus Status { get; set; } = SafetyIncidentStatus.Reported;
    public string? Location { get; set; }
    public string? ReportedByName { get; set; }
    public string? CorrectiveAction { get; set; }
}
