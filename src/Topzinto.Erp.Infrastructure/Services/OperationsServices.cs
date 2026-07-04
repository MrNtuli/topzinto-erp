using Microsoft.EntityFrameworkCore;
using Topzinto.Erp.Application.DTOs.Tasks;
using Topzinto.Erp.Application.Interfaces;
using Topzinto.Erp.Domain.Entities;
using Topzinto.Erp.Domain.Enums;
using Topzinto.Erp.Infrastructure.Persistence;

namespace Topzinto.Erp.Infrastructure.Services;

public class ProjectTaskService : IProjectTaskService
{
    private readonly AppDbContext _db;

    public ProjectTaskService(AppDbContext db) => _db = db;

    public async Task<IReadOnlyList<ProjectTaskDto>> GetByProjectAsync(Guid projectId, CancellationToken ct = default) =>
        await Query().Where(t => t.ProjectId == projectId).ToListAsync(ct);

    public async Task<IReadOnlyList<ProjectTaskDto>> GetOverdueAsync(CancellationToken ct = default)
    {
        var today = DateTime.UtcNow.Date;
        return await _db.ProjectTasks
            .Include(t => t.Project)
            .Where(t => t.DueDate < today && t.Status != ProjectTaskStatus.Completed)
            .OrderBy(t => t.DueDate)
            .Select(t => new ProjectTaskDto(
                t.Id, t.ProjectId, t.Project.Name, t.Title,
                FormatProjectTaskStatus(t.Status), FormatPriority(t.Priority),
                t.DueDate.HasValue ? t.DueDate.Value.ToString("dd MMM yyyy") : null,
                t.AssignedToName
            ))
            .ToListAsync(ct);
    }

    public async Task<ProjectTaskDto> CreateAsync(CreateProjectTaskRequest request, Guid? userId, CancellationToken ct = default)
    {
        var task = new ProjectTask
        {
            ProjectId = request.ProjectId,
            MilestoneId = request.MilestoneId,
            Title = request.Title,
            Description = request.Description,
            AssignedToName = request.AssignedToName,
            DueDate = request.DueDate,
            Priority = ParsePriority(request.Priority),
            Status = ParseStatus(request.Status),
            CreatedBy = userId,
        };
        _db.ProjectTasks.Add(task);
        await _db.SaveChangesAsync(ct);
        return (await Query().FirstAsync(t => t.Id == task.Id, ct));
    }

    private IQueryable<ProjectTaskDto> Query() =>
        _db.ProjectTasks
            .Include(t => t.Project)
            .OrderBy(t => t.DueDate)
            .Select(t => new ProjectTaskDto(
                t.Id, t.ProjectId, t.Project.Name, t.Title,
                FormatProjectTaskStatus(t.Status), FormatPriority(t.Priority),
                t.DueDate.HasValue ? t.DueDate.Value.ToString("dd MMM yyyy") : null,
                t.AssignedToName
            ));

    private static TaskPriority ParsePriority(string? v) => v?.ToLowerInvariant() switch
    {
        "low" => TaskPriority.Low,
        "high" => TaskPriority.High,
        "critical" => TaskPriority.Critical,
        _ => TaskPriority.Medium,
    };

    private static ProjectTaskStatus ParseStatus(string? v) => v?.ToLowerInvariant() switch
    {
        "in progress" => ProjectTaskStatus.InProgress,
        "completed" => ProjectTaskStatus.Completed,
        "overdue" => ProjectTaskStatus.Overdue,
        _ => ProjectTaskStatus.NotStarted,
    };

    private static string FormatProjectTaskStatus(ProjectTaskStatus s) => s switch
    {
        ProjectTaskStatus.NotStarted => "Not Started",
        ProjectTaskStatus.InProgress => "In Progress",
        ProjectTaskStatus.Completed => "Completed",
        ProjectTaskStatus.Overdue => "Overdue",
        _ => s.ToString(),
    };

    private static string FormatPriority(TaskPriority p) => p.ToString();
}

public class ProjectMilestoneService : IProjectMilestoneService
{
    private readonly AppDbContext _db;

    public ProjectMilestoneService(AppDbContext db) => _db = db;

    public async Task<IReadOnlyList<ProjectMilestoneDto>> GetByProjectAsync(Guid projectId, CancellationToken ct = default) =>
        await _db.ProjectMilestones
            .Include(m => m.Project)
            .Where(m => m.ProjectId == projectId)
            .OrderBy(m => m.SortOrder)
            .Select(m => new ProjectMilestoneDto(
                m.Id, m.ProjectId, m.Project.Name, m.Name,
                m.StartDate.HasValue ? m.StartDate.Value.ToString("dd MMM yyyy") : null,
                m.DueDate.ToString("dd MMM yyyy"),
                FormatMilestoneStatus(m.Status), m.Progress
            ))
            .ToListAsync(ct);

    public async Task<ProjectMilestoneDto> CreateAsync(CreateMilestoneRequest request, Guid? userId, CancellationToken ct = default)
    {
        var maxOrder = await _db.ProjectMilestones.Where(m => m.ProjectId == request.ProjectId).MaxAsync(m => (int?)m.SortOrder, ct) ?? 0;
        var milestone = new ProjectMilestone
        {
            ProjectId = request.ProjectId,
            Name = request.Name,
            StartDate = request.StartDate,
            DueDate = request.DueDate,
            Status = ParseMilestoneStatus(request.Status),
            Progress = Math.Clamp(request.Progress, 0, 100),
            SortOrder = maxOrder + 1,
            CreatedBy = userId,
        };
        _db.ProjectMilestones.Add(milestone);
        await _db.SaveChangesAsync(ct);
        await _db.Entry(milestone).Reference(m => m.Project).LoadAsync(ct);
        return new ProjectMilestoneDto(
            milestone.Id, milestone.ProjectId, milestone.Project.Name, milestone.Name,
            milestone.StartDate?.ToString("dd MMM yyyy"),
            milestone.DueDate.ToString("dd MMM yyyy"),
            FormatMilestoneStatus(milestone.Status), milestone.Progress
        );
    }

    private static MilestoneStatus ParseMilestoneStatus(string? v) => v?.ToLowerInvariant() switch
    {
        "in progress" => MilestoneStatus.InProgress,
        "completed" => MilestoneStatus.Completed,
        "delayed" => MilestoneStatus.Delayed,
        _ => MilestoneStatus.Planned,
    };

    private static string FormatMilestoneStatus(MilestoneStatus s) => s switch
    {
        MilestoneStatus.Planned => "Planned",
        MilestoneStatus.InProgress => "In Progress",
        MilestoneStatus.Completed => "Completed",
        MilestoneStatus.Delayed => "Delayed",
        _ => s.ToString(),
    };
}

public class ScheduleService : IScheduleService
{
    private readonly AppDbContext _db;

    public ScheduleService(AppDbContext db) => _db = db;

    public async Task<IReadOnlyList<ScheduleEventDto>> GetEventsAsync(DateTime? from = null, DateTime? to = null, CancellationToken ct = default)
    {
        var start = from ?? DateTime.UtcNow.Date.AddMonths(-1);
        var end = to ?? DateTime.UtcNow.Date.AddMonths(3);
        var events = new List<ScheduleEventDto>();

        var milestones = await _db.ProjectMilestones
            .Include(m => m.Project)
            .Where(m => m.DueDate >= start && m.DueDate <= end)
            .ToListAsync(ct);
        events.AddRange(milestones.Select(m => new ScheduleEventDto(
            m.Id, m.Name, "Milestone", m.DueDate.ToString("yyyy-MM-dd"),
            null, m.Project.Name, FormatMilestoneStatus(m.Status))));

        var tasks = await _db.ProjectTasks
            .Include(t => t.Project)
            .Where(t => t.DueDate >= start && t.DueDate <= end)
            .ToListAsync(ct);
        events.AddRange(tasks.Select(t => new ScheduleEventDto(
            t.Id, t.Title, "Task", t.DueDate!.Value.ToString("yyyy-MM-dd"),
            null, t.Project.Name, t.Status.ToString())));

        var tenders = await _db.Tenders
            .Include(t => t.Client)
            .Where(t => t.ClosingDate >= start && t.ClosingDate <= end)
            .ToListAsync(ct);
        events.AddRange(tenders.Select(t => new ScheduleEventDto(
            t.Id, t.Title, "Tender", t.ClosingDate.ToString("yyyy-MM-dd"),
            null, t.Client.Name, t.Status.ToString())));

        return events.OrderBy(e => e.Date).ToList();
    }

    private static string FormatMilestoneStatus(MilestoneStatus s) => s switch
    {
        MilestoneStatus.InProgress => "In Progress",
        MilestoneStatus.Completed => "Completed",
        MilestoneStatus.Delayed => "Delayed",
        _ => "Planned",
    };
}
