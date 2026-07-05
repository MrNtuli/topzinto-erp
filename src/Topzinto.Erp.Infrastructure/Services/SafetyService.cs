using Microsoft.EntityFrameworkCore;
using Topzinto.Erp.Application.DTOs.Safety;
using Topzinto.Erp.Application.Interfaces;
using Topzinto.Erp.Domain.Entities;
using Topzinto.Erp.Domain.Enums;
using Topzinto.Erp.Infrastructure.Persistence;

namespace Topzinto.Erp.Infrastructure.Services;

public class SafetyService : ISafetyService
{
    private readonly AppDbContext _db;
    private readonly IAuditService _audit;

    public SafetyService(AppDbContext db, IAuditService audit)
    {
        _db = db;
        _audit = audit;
    }

    public async Task<IReadOnlyList<SafetyIncidentDto>> GetAllAsync(
        Guid? projectId = null, string? status = null, CancellationToken ct = default)
    {
        var query = _db.SafetyIncidents.Include(i => i.Project).AsQueryable();
        if (projectId.HasValue)
            query = query.Where(i => i.ProjectId == projectId.Value);
        if (!string.IsNullOrWhiteSpace(status))
            query = query.Where(i => i.Status == ParseStatus(status));

        var incidents = await query.OrderByDescending(i => i.IncidentDate).ToListAsync(ct);
        return incidents.Select(Map).ToList();
    }

    public async Task<SafetyIncidentDto?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var incident = await _db.SafetyIncidents.Include(i => i.Project).FirstOrDefaultAsync(i => i.Id == id, ct);
        return incident is null ? null : Map(incident);
    }

    public async Task<SafetyIncidentDto> CreateAsync(CreateSafetyIncidentRequest request, Guid? userId, CancellationToken ct = default)
    {
        var incident = new SafetyIncident
        {
            ProjectId = request.ProjectId,
            IncidentDate = request.IncidentDate.Date,
            Title = request.Title.Trim(),
            Description = request.Description.Trim(),
            Severity = ParseSeverity(request.Severity),
            Status = ParseStatus(request.Status),
            Location = request.Location?.Trim(),
            ReportedByName = request.ReportedByName?.Trim(),
            CorrectiveAction = request.CorrectiveAction?.Trim(),
            CreatedBy = userId,
        };

        _db.SafetyIncidents.Add(incident);
        await _db.SaveChangesAsync(ct);
        await _audit.LogAsync(userId, "", "Create", "Safety", "SafetyIncident", incident.Id.ToString(), newValues: incident.Title, ct: ct);
        return (await GetByIdAsync(incident.Id, ct))!;
    }

    public async Task<SafetyIncidentDto?> UpdateAsync(Guid id, UpdateSafetyIncidentRequest request, Guid? userId, CancellationToken ct = default)
    {
        var incident = await _db.SafetyIncidents.FindAsync([id], ct);
        if (incident is null) return null;

        incident.ProjectId = request.ProjectId;
        incident.IncidentDate = request.IncidentDate.Date;
        incident.Title = request.Title.Trim();
        incident.Description = request.Description.Trim();
        incident.Severity = ParseSeverity(request.Severity);
        incident.Status = ParseStatus(request.Status);
        incident.Location = request.Location?.Trim();
        incident.ReportedByName = request.ReportedByName?.Trim();
        incident.CorrectiveAction = request.CorrectiveAction?.Trim();
        incident.UpdatedAt = DateTime.UtcNow;
        incident.UpdatedBy = userId;

        await _db.SaveChangesAsync(ct);
        await _audit.LogAsync(userId, "", "Update", "Safety", "SafetyIncident", incident.Id.ToString(), newValues: incident.Title, ct: ct);
        return await GetByIdAsync(id, ct);
    }

    public async Task<bool> DeleteAsync(Guid id, Guid? userId, CancellationToken ct = default)
    {
        var incident = await _db.SafetyIncidents.FindAsync([id], ct);
        if (incident is null) return false;
        incident.IsDeleted = true;
        incident.UpdatedAt = DateTime.UtcNow;
        incident.UpdatedBy = userId;
        await _db.SaveChangesAsync(ct);
        await _audit.LogAsync(userId, "", "Delete", "Safety", "SafetyIncident", incident.Id.ToString(), newValues: incident.Title, ct: ct);
        return true;
    }

    private static SafetyIncidentDto Map(SafetyIncident i) => new(
        i.Id,
        i.ProjectId,
        i.Project.Name,
        i.IncidentDate.ToString("dd MMM yyyy"),
        i.Title,
        i.Description,
        FormatSeverity(i.Severity),
        FormatStatus(i.Status),
        i.Location,
        i.ReportedByName,
        i.CorrectiveAction
    );

    private static SafetyIncidentSeverity ParseSeverity(string? value) => value?.ToLowerInvariant() switch
    {
        "low" => SafetyIncidentSeverity.Low,
        "high" => SafetyIncidentSeverity.High,
        "critical" => SafetyIncidentSeverity.Critical,
        _ => SafetyIncidentSeverity.Medium,
    };

    private static SafetyIncidentStatus ParseStatus(string? value) => value?.ToLowerInvariant() switch
    {
        "investigating" => SafetyIncidentStatus.Investigating,
        "resolved" => SafetyIncidentStatus.Resolved,
        "closed" => SafetyIncidentStatus.Closed,
        _ => SafetyIncidentStatus.Reported,
    };

    private static string FormatSeverity(SafetyIncidentSeverity s) => s.ToString();
    private static string FormatStatus(SafetyIncidentStatus s) => s.ToString();
}
