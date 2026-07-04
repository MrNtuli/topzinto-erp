using Microsoft.EntityFrameworkCore;
using Topzinto.Erp.Application.DTOs.Timesheets;
using Topzinto.Erp.Application.Interfaces;
using Topzinto.Erp.Domain.Entities;
using Topzinto.Erp.Infrastructure.Common;
using Topzinto.Erp.Infrastructure.Persistence;

namespace Topzinto.Erp.Infrastructure.Services;

public class TimesheetService : ITimesheetService
{
    private readonly AppDbContext _db;
    private readonly IAuditService _audit;

    public TimesheetService(AppDbContext db, IAuditService audit)
    {
        _db = db;
        _audit = audit;
    }

    public async Task<IReadOnlyList<TimesheetEntryDto>> GetAllAsync(
        Guid? projectId = null, Guid? employeeId = null, string? status = null, CancellationToken ct = default)
    {
        var query = _db.TimesheetEntries
            .Include(t => t.Employee)
            .Include(t => t.Project)
            .AsQueryable();

        if (projectId.HasValue)
            query = query.Where(t => t.ProjectId == projectId.Value);
        if (employeeId.HasValue)
            query = query.Where(t => t.EmployeeId == employeeId.Value);
        if (!string.IsNullOrWhiteSpace(status))
        {
            var s = HrDisplay.ParseTimesheetStatus(status);
            query = query.Where(t => t.Status == s);
        }

        var list = await query.OrderByDescending(t => t.WorkDate).ToListAsync(ct);
        return list.Select(Map).ToList();
    }

    public async Task<IReadOnlyList<ProjectLabourSummaryDto>> GetLabourSummaryAsync(
        Guid? projectId = null, CancellationToken ct = default)
    {
        var query = _db.TimesheetEntries
            .Include(t => t.Employee)
            .Include(t => t.Project)
            .AsQueryable();

        if (projectId.HasValue)
            query = query.Where(t => t.ProjectId == projectId.Value);

        var entries = await query.ToListAsync(ct);

        return entries
            .GroupBy(t => new { t.ProjectId, t.Project.Name })
            .Select(g => new ProjectLabourSummaryDto(
                g.Key.ProjectId,
                g.Key.Name,
                g.Sum(t => t.Hours),
                g.Sum(t => t.Hours * (t.Employee.HourlyRate ?? 0)),
                g.Count()
            ))
            .OrderByDescending(s => s.TotalHours)
            .ToList();
    }

    public async Task<TimesheetEntryDto?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var entry = await _db.TimesheetEntries
            .Include(t => t.Employee)
            .Include(t => t.Project)
            .FirstOrDefaultAsync(t => t.Id == id, ct);
        return entry is null ? null : Map(entry);
    }

    public async Task<TimesheetEntryDto> CreateAsync(CreateTimesheetRequest request, Guid? userId, CancellationToken ct = default)
    {
        var entry = MapToEntity(new TimesheetEntry(), request);
        entry.CreatedBy = userId;
        _db.TimesheetEntries.Add(entry);
        await _db.SaveChangesAsync(ct);
        await _audit.LogAsync(userId, "", "Create", "HR", "TimesheetEntry", entry.Id.ToString(), newValues: $"{entry.Hours}h", ct: ct);
        return (await GetByIdAsync(entry.Id, ct))!;
    }

    public async Task<TimesheetEntryDto?> UpdateAsync(Guid id, UpdateTimesheetRequest request, Guid? userId, CancellationToken ct = default)
    {
        var entry = await _db.TimesheetEntries.FindAsync([id], ct);
        if (entry is null) return null;
        MapToEntity(entry, request);
        entry.UpdatedAt = DateTime.UtcNow;
        entry.UpdatedBy = userId;
        await _db.SaveChangesAsync(ct);
        await _audit.LogAsync(userId, "", "Update", "HR", "TimesheetEntry", entry.Id.ToString(), newValues: $"{entry.Hours}h", ct: ct);
        return await GetByIdAsync(id, ct);
    }

    private static TimesheetEntryDto Map(TimesheetEntry t) => new(
        t.Id,
        t.EmployeeId,
        $"{t.Employee.FirstName} {t.Employee.LastName}",
        t.ProjectId,
        t.Project.Name,
        HrDisplay.FormatDate(t.WorkDate),
        t.Hours,
        HrDisplay.FormatTimesheetStatus(t.Status),
        t.Description,
        t.Notes,
        t.Employee.HourlyRate.HasValue ? t.Hours * t.Employee.HourlyRate.Value : null
    );

    private static TimesheetEntry MapToEntity(TimesheetEntry e, CreateTimesheetRequest r)
    {
        e.EmployeeId = r.EmployeeId;
        e.ProjectId = r.ProjectId;
        e.WorkDate = HrDisplay.ParseDate(r.WorkDate) ?? DateTime.UtcNow.Date;
        e.Hours = r.Hours;
        e.Status = HrDisplay.ParseTimesheetStatus(r.Status);
        e.Description = r.Description?.Trim();
        e.Notes = r.Notes?.Trim();
        return e;
    }

    private static TimesheetEntry MapToEntity(TimesheetEntry e, UpdateTimesheetRequest r) =>
        MapToEntity(e, new CreateTimesheetRequest(
            r.EmployeeId, r.ProjectId, r.WorkDate, r.Hours, r.Status, r.Description, r.Notes));
}
