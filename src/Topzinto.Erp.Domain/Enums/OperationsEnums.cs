namespace Topzinto.Erp.Domain.Enums;

public enum SiteReportStatus
{
    Draft = 0,
    Submitted = 1,
    Approved = 2,
}

public enum ProjectTaskStatus
{
    NotStarted = 0,
    InProgress = 1,
    Completed = 2,
    Overdue = 3,
}

public enum TaskPriority
{
    Low = 0,
    Medium = 1,
    High = 2,
    Critical = 3,
}

public enum MilestoneStatus
{
    Planned = 0,
    InProgress = 1,
    Completed = 2,
    Delayed = 3,
}
