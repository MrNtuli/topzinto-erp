using Microsoft.EntityFrameworkCore;
using Topzinto.Erp.Application.DTOs.Attendance;
using Topzinto.Erp.Application.Interfaces;
using Topzinto.Erp.Domain.Entities;
using Topzinto.Erp.Domain.Enums;
using Topzinto.Erp.Infrastructure.Persistence;

namespace Topzinto.Erp.Infrastructure.Services;

public class AttendanceService : IAttendanceService
{
    private readonly AppDbContext _db;
    private readonly IAuditService _audit;

    public AttendanceService(AppDbContext db, IAuditService audit)
    {
        _db = db;
        _audit = audit;
    }

    public async Task<IReadOnlyList<AttendanceRecordDto>> GetAllAsync(
        Guid? projectId = null,
        Guid? employeeId = null,
        string? status = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        CancellationToken ct = default)
    {
        var query = _db.AttendanceRecords
            .Include(a => a.Employee)
            .Include(a => a.Project)
            .AsQueryable();

        if (projectId.HasValue)
            query = query.Where(a => a.ProjectId == projectId.Value);
        if (employeeId.HasValue)
            query = query.Where(a => a.EmployeeId == employeeId.Value);
        if (!string.IsNullOrWhiteSpace(status))
            query = query.Where(a => a.Status == ParseStatus(status));
        if (fromDate.HasValue)
            query = query.Where(a => a.WorkDate >= fromDate.Value.Date);
        if (toDate.HasValue)
            query = query.Where(a => a.WorkDate <= toDate.Value.Date);

        var records = await query.OrderByDescending(a => a.WorkDate).ThenBy(a => a.Employee.LastName).ToListAsync(ct);
        return records.Select(Map).ToList();
    }

    public async Task<AttendanceRecordDto?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var record = await _db.AttendanceRecords
            .Include(a => a.Employee)
            .Include(a => a.Project)
            .FirstOrDefaultAsync(a => a.Id == id, ct);
        return record is null ? null : Map(record);
    }

    public async Task<AttendanceRecordDto> CreateAsync(CreateAttendanceRecordRequest request, Guid? userId, CancellationToken ct = default)
    {
        var workDate = request.WorkDate.Date;
        var exists = await _db.AttendanceRecords.AnyAsync(
            a => a.EmployeeId == request.EmployeeId && a.WorkDate == workDate, ct);
        if (exists)
            throw new InvalidOperationException("Attendance already recorded for this employee on this date.");

        var record = new AttendanceRecord
        {
            EmployeeId = request.EmployeeId,
            ProjectId = request.ProjectId,
            WorkDate = workDate,
            Status = ParseStatus(request.Status),
            CheckInTime = ParseTime(request.CheckInTime),
            CheckOutTime = ParseTime(request.CheckOutTime),
            HoursWorked = request.HoursWorked,
            Notes = request.Notes?.Trim(),
            CreatedBy = userId,
        };

        _db.AttendanceRecords.Add(record);
        await _db.SaveChangesAsync(ct);
        await _audit.LogAsync(userId, "", "Create", "Attendance", "AttendanceRecord", record.Id.ToString(), ct: ct);
        return (await GetByIdAsync(record.Id, ct))!;
    }

    public async Task<AttendanceRecordDto?> UpdateAsync(Guid id, UpdateAttendanceRecordRequest request, Guid? userId, CancellationToken ct = default)
    {
        var record = await _db.AttendanceRecords.FindAsync([id], ct);
        if (record is null) return null;

        var workDate = request.WorkDate.Date;
        var duplicate = await _db.AttendanceRecords.AnyAsync(
            a => a.Id != id && a.EmployeeId == request.EmployeeId && a.WorkDate == workDate, ct);
        if (duplicate)
            throw new InvalidOperationException("Attendance already recorded for this employee on this date.");

        record.EmployeeId = request.EmployeeId;
        record.ProjectId = request.ProjectId;
        record.WorkDate = workDate;
        record.Status = ParseStatus(request.Status);
        record.CheckInTime = ParseTime(request.CheckInTime);
        record.CheckOutTime = ParseTime(request.CheckOutTime);
        record.HoursWorked = request.HoursWorked;
        record.Notes = request.Notes?.Trim();
        record.UpdatedAt = DateTime.UtcNow;
        record.UpdatedBy = userId;

        await _db.SaveChangesAsync(ct);
        await _audit.LogAsync(userId, "", "Update", "Attendance", "AttendanceRecord", record.Id.ToString(), ct: ct);
        return await GetByIdAsync(id, ct);
    }

    public async Task<bool> DeleteAsync(Guid id, Guid? userId, CancellationToken ct = default)
    {
        var record = await _db.AttendanceRecords.FindAsync([id], ct);
        if (record is null) return false;
        record.IsDeleted = true;
        record.UpdatedAt = DateTime.UtcNow;
        record.UpdatedBy = userId;
        await _db.SaveChangesAsync(ct);
        await _audit.LogAsync(userId, "", "Delete", "Attendance", "AttendanceRecord", record.Id.ToString(), ct: ct);
        return true;
    }

    private static AttendanceRecordDto Map(AttendanceRecord a) => new(
        a.Id,
        a.EmployeeId,
        $"{a.Employee.FirstName} {a.Employee.LastName}".Trim(),
        a.ProjectId,
        a.Project?.Name,
        a.WorkDate.ToString("dd MMM yyyy"),
        FormatStatus(a.Status),
        FormatTime(a.CheckInTime),
        FormatTime(a.CheckOutTime),
        a.HoursWorked,
        a.Notes
    );

    private static AttendanceStatus ParseStatus(string? value) => value?.ToLowerInvariant() switch
    {
        "absent" => AttendanceStatus.Absent,
        "late" => AttendanceStatus.Late,
        "halfday" or "half day" => AttendanceStatus.HalfDay,
        "onleave" or "on leave" => AttendanceStatus.OnLeave,
        "publicholiday" or "public holiday" => AttendanceStatus.PublicHoliday,
        _ => AttendanceStatus.Present,
    };

    private static string FormatStatus(AttendanceStatus status) => status switch
    {
        AttendanceStatus.Absent => "Absent",
        AttendanceStatus.Late => "Late",
        AttendanceStatus.HalfDay => "HalfDay",
        AttendanceStatus.OnLeave => "OnLeave",
        AttendanceStatus.PublicHoliday => "PublicHoliday",
        _ => "Present",
    };

    private static TimeSpan? ParseTime(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;
        return TimeSpan.TryParse(value, out var t) ? t : null;
    }

    private static string? FormatTime(TimeSpan? value) =>
        value.HasValue ? value.Value.ToString(@"hh\:mm") : null;
}
