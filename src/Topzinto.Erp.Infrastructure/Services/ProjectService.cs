using Microsoft.EntityFrameworkCore;
using Topzinto.Erp.Application.DTOs.Projects;
using Topzinto.Erp.Application.Interfaces;
using Topzinto.Erp.Domain.Entities;
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
