using Microsoft.EntityFrameworkCore;
using Topzinto.Erp.Application.DTOs.Projects;
using Topzinto.Erp.Application.Interfaces;
using Topzinto.Erp.Domain.Entities;
using Topzinto.Erp.Domain.Enums;
using Topzinto.Erp.Infrastructure.Common;
using Topzinto.Erp.Infrastructure.Persistence;

namespace Topzinto.Erp.Infrastructure.Services;

public class ProjectService : IProjectService
{
    private readonly AppDbContext _db;
    private readonly IAuditService _audit;

    public ProjectService(AppDbContext db, IAuditService audit)
    {
        _db = db;
        _audit = audit;
    }

    public async Task<IReadOnlyList<ProjectDto>> GetAllAsync(
        string? search = null, string? status = null, Guid? clientId = null, CancellationToken ct = default)
    {
        var query = _db.Projects.Include(p => p.Client).AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(p => p.Name.Contains(search) || p.Code.Contains(search));

        if (!string.IsNullOrWhiteSpace(status))
        {
            var s = EnumDisplay.ParseProjectStatus(status);
            query = query.Where(p => p.Status == s);
        }

        if (clientId.HasValue)
            query = query.Where(p => p.ClientId == clientId);

        return await query
            .OrderByDescending(p => p.CreatedAt)
            .Select(p => new ProjectDto(
                p.Id,
                p.Code,
                p.Name,
                p.Client.Name,
                p.ClientId,
                EnumDisplay.FormatProjectStatus(p.Status),
                p.Progress,
                p.EndDate.HasValue ? p.EndDate.Value.ToString("dd MMM yyyy") : null,
                p.ContractValue
            ))
            .ToListAsync(ct);
    }

    public async Task<ProjectDetailDto?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var p = await _db.Projects
            .Include(x => x.Client)
            .Include(x => x.Contract)
            .FirstOrDefaultAsync(x => x.Id == id, ct);
        return p is null ? null : MapDetail(p);
    }

    public async Task<ProjectDetailDto> CreateAsync(CreateProjectRequest request, Guid? userId, CancellationToken ct = default)
    {
        var project = new Project
        {
            Code = request.Code,
            Name = request.Name,
            ClientId = request.ClientId,
            Status = EnumDisplay.ParseProjectStatus(request.Status),
            Progress = Math.Clamp(request.Progress, 0, 100),
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            ContractValue = request.ContractValue,
            Budget = request.Budget,
            Description = request.Description,
            SiteLocation = request.SiteLocation,
            CreatedBy = userId,
        };

        _db.Projects.Add(project);
        await _db.SaveChangesAsync(ct);
        await _db.Entry(project).Reference(p => p.Client).LoadAsync(ct);
        await _audit.LogAsync(userId, "", "Create", "Projects", "Project", project.Id.ToString(), newValues: project.Name, ct: ct);
        return MapDetail(project);
    }

    public async Task<ProjectDetailDto?> UpdateAsync(Guid id, UpdateProjectRequest request, Guid? userId, CancellationToken ct = default)
    {
        var project = await _db.Projects
            .Include(p => p.Client)
            .Include(p => p.Contract)
            .FirstOrDefaultAsync(p => p.Id == id, ct);
        if (project is null) return null;

        project.Code = request.Code;
        project.Name = request.Name;
        project.ClientId = request.ClientId;
        project.Status = EnumDisplay.ParseProjectStatus(request.Status);
        project.Progress = Math.Clamp(request.Progress, 0, 100);
        project.StartDate = request.StartDate;
        project.EndDate = request.EndDate;
        project.ContractValue = request.ContractValue;
        project.Budget = request.Budget;
        project.Description = request.Description;
        project.SiteLocation = request.SiteLocation;
        project.UpdatedAt = DateTime.UtcNow;
        project.UpdatedBy = userId;

        await _db.SaveChangesAsync(ct);
        return MapDetail(project);
    }

    public async Task<bool> DeleteAsync(Guid id, Guid? userId, CancellationToken ct = default)
    {
        var project = await _db.Projects.FindAsync([id], ct);
        if (project is null) return false;
        project.IsDeleted = true;
        project.UpdatedAt = DateTime.UtcNow;
        project.UpdatedBy = userId;
        await _db.SaveChangesAsync(ct);
        return true;
    }

    public async Task<IReadOnlyList<ActivityItemDto>?> GetActivityAsync(Guid projectId, CancellationToken ct = default)
    {
        if (!await _db.Projects.AnyAsync(p => p.Id == projectId, ct))
            return null;

        var projectIdStr = projectId.ToString();
        var taskIds = await _db.ProjectTasks.Where(t => t.ProjectId == projectId).Select(t => t.Id.ToString()).ToListAsync(ct);
        var milestoneIds = await _db.ProjectMilestones.Where(m => m.ProjectId == projectId).Select(m => m.Id.ToString()).ToListAsync(ct);
        var siteReportIds = await _db.SiteReports.Where(s => s.ProjectId == projectId).Select(s => s.Id.ToString()).ToListAsync(ct);
        var boqIds = await _db.BoqItems.Where(b => b.ProjectId == projectId).Select(b => b.Id.ToString()).ToListAsync(ct);
        var claimIds = await _db.Claims.Where(c => c.ProjectId == projectId).Select(c => c.Id.ToString()).ToListAsync(ct);
        var documentIds = await _db.Documents
            .Where(d => d.ParentType == DocumentParentType.Project && d.ParentId == projectId)
            .Select(d => d.Id.ToString())
            .ToListAsync(ct);

        var logs = await _db.AuditLogs
            .Where(a =>
                (a.EntityType == "Project" && a.EntityId == projectIdStr) ||
                (a.EntityType == "ProjectTask" && taskIds.Contains(a.EntityId)) ||
                (a.EntityType == "ProjectMilestone" && milestoneIds.Contains(a.EntityId)) ||
                (a.EntityType == "SiteReport" && siteReportIds.Contains(a.EntityId)) ||
                (a.EntityType == "BoqItem" && boqIds.Contains(a.EntityId)) ||
                (a.EntityType == "Claim" && claimIds.Contains(a.EntityId)) ||
                ((a.EntityType == "DocumentRecord" || a.EntityType == "Document") && documentIds.Contains(a.EntityId)))
            .OrderByDescending(a => a.CreatedAt)
            .Take(100)
            .ToListAsync(ct);

        return logs.Select(MapActivity).ToList();
    }

    private static ActivityItemDto MapActivity(AuditLog log)
    {
        var detail = log.NewValues ?? log.OldValues;
        var summary = string.IsNullOrWhiteSpace(detail)
            ? $"{log.Action} {log.EntityType}"
            : $"{log.Action} {log.EntityType}: {detail}";

        return new ActivityItemDto(
            log.Id,
            log.Action,
            log.Module,
            log.EntityType,
            log.EntityId,
            log.UserEmail,
            log.CreatedAt,
            summary
        );
    }

    private static ProjectDetailDto MapDetail(Project p) => new(
        p.Id,
        p.Code,
        p.Name,
        p.ClientId,
        p.Client.Name,
        EnumDisplay.FormatProjectStatus(p.Status),
        p.Progress,
        p.StartDate?.ToString("dd MMM yyyy"),
        p.EndDate?.ToString("dd MMM yyyy"),
        p.ContractValue,
        p.Budget,
        p.Description,
        p.SiteLocation,
        p.Contract?.Id,
        p.TenderId,
        p.StartDate?.ToString("yyyy-MM-dd"),
        p.EndDate?.ToString("yyyy-MM-dd")
    );
}
