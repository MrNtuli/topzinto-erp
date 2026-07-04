using Topzinto.Erp.Domain.Common;
using Topzinto.Erp.Domain.Enums;

namespace Topzinto.Erp.Domain.Entities;

public class SiteReport : BaseEntity
{
    public Guid ProjectId { get; set; }
    public Project Project { get; set; } = null!;
    public DateTime ReportDate { get; set; }
    public string? Weather { get; set; }
    public string? Temperature { get; set; }
    public string? WindSpeed { get; set; }
    public int? PersonnelCount { get; set; }
    public string WorkCompleted { get; set; } = string.Empty;
    public string? WorkPlanned { get; set; }
    public string? DelaysIssues { get; set; }
    public string? Notes { get; set; }
    public SiteReportStatus Status { get; set; } = SiteReportStatus.Draft;
    public string? SubmittedByName { get; set; }
    public DateTime? SubmittedAt { get; set; }
    public ICollection<SiteReportPhoto> Photos { get; set; } = [];
}

public class SiteReportPhoto : BaseEntity
{
    public Guid SiteReportId { get; set; }
    public SiteReport SiteReport { get; set; } = null!;
    public string FileName { get; set; } = string.Empty;
    public string StoragePath { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long SizeBytes { get; set; }
    public string? Caption { get; set; }
}

public class ProjectMilestone : BaseEntity
{
    public Guid ProjectId { get; set; }
    public Project Project { get; set; } = null!;
    public string Name { get; set; } = string.Empty;
    public DateTime? StartDate { get; set; }
    public DateTime DueDate { get; set; }
    public DateTime? CompletedDate { get; set; }
    public MilestoneStatus Status { get; set; } = MilestoneStatus.Planned;
    public int SortOrder { get; set; }
    public int Progress { get; set; }
}

public class ProjectTask : BaseEntity
{
    public Guid ProjectId { get; set; }
    public Project Project { get; set; } = null!;
    public Guid? MilestoneId { get; set; }
    public ProjectMilestone? Milestone { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Guid? AssignedToUserId { get; set; }
    public string? AssignedToName { get; set; }
    public DateTime? DueDate { get; set; }
    public TaskPriority Priority { get; set; } = TaskPriority.Medium;
    public ProjectTaskStatus Status { get; set; } = ProjectTaskStatus.NotStarted;
}
