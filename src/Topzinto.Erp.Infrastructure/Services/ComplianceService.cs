using Microsoft.EntityFrameworkCore;
using Topzinto.Erp.Application.DTOs.Compliance;
using Topzinto.Erp.Application.Interfaces;
using Topzinto.Erp.Domain.Entities;
using Topzinto.Erp.Domain.Enums;
using Topzinto.Erp.Infrastructure.Persistence;

namespace Topzinto.Erp.Infrastructure.Services;

public class ComplianceService : IComplianceService
{
    private readonly AppDbContext _db;
    private readonly IAuditService _audit;

    public ComplianceService(AppDbContext db, IAuditService audit)
    {
        _db = db;
        _audit = audit;
    }

    public async Task<IReadOnlyList<ComplianceRecordDto>> GetAllAsync(
        Guid? projectId = null, string? status = null, CancellationToken ct = default)
    {
        var query = _db.ComplianceRecords.Include(r => r.Project).AsQueryable();
        if (projectId.HasValue)
            query = query.Where(r => r.ProjectId == projectId.Value);
        if (!string.IsNullOrWhiteSpace(status))
            query = query.Where(r => r.Status == ParseStatus(status));

        var records = await query.OrderByDescending(r => r.IssueDate).ToListAsync(ct);
        return records.Select(Map).ToList();
    }

    public async Task<ComplianceRecordDto?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var record = await _db.ComplianceRecords.Include(r => r.Project).FirstOrDefaultAsync(r => r.Id == id, ct);
        return record is null ? null : Map(record);
    }

    public async Task<ComplianceRecordDto> CreateAsync(CreateComplianceRecordRequest request, Guid? userId, CancellationToken ct = default)
    {
        var record = new ComplianceRecord
        {
            Title = request.Title.Trim(),
            Type = ParseType(request.Type),
            EntityType = request.EntityType?.Trim(),
            EntityId = request.EntityId,
            ProjectId = request.ProjectId,
            IssueDate = request.IssueDate.Date,
            ExpiryDate = request.ExpiryDate?.Date,
            Status = ParseStatus(request.Status),
            ResponsiblePerson = request.ResponsiblePerson?.Trim(),
            Notes = request.Notes?.Trim(),
            CreatedBy = userId,
        };

        _db.ComplianceRecords.Add(record);
        await _db.SaveChangesAsync(ct);
        await _audit.LogAsync(userId, "", "Create", "Compliance", "ComplianceRecord", record.Id.ToString(), newValues: record.Title, ct: ct);
        return (await GetByIdAsync(record.Id, ct))!;
    }

    public async Task<ComplianceRecordDto?> UpdateAsync(Guid id, UpdateComplianceRecordRequest request, Guid? userId, CancellationToken ct = default)
    {
        var record = await _db.ComplianceRecords.FindAsync([id], ct);
        if (record is null) return null;

        record.Title = request.Title.Trim();
        record.Type = ParseType(request.Type);
        record.EntityType = request.EntityType?.Trim();
        record.EntityId = request.EntityId;
        record.ProjectId = request.ProjectId;
        record.IssueDate = request.IssueDate.Date;
        record.ExpiryDate = request.ExpiryDate?.Date;
        record.Status = ParseStatus(request.Status);
        record.ResponsiblePerson = request.ResponsiblePerson?.Trim();
        record.Notes = request.Notes?.Trim();
        record.UpdatedAt = DateTime.UtcNow;
        record.UpdatedBy = userId;

        await _db.SaveChangesAsync(ct);
        await _audit.LogAsync(userId, "", "Update", "Compliance", "ComplianceRecord", record.Id.ToString(), newValues: record.Title, ct: ct);
        return await GetByIdAsync(id, ct);
    }

    public async Task<bool> DeleteAsync(Guid id, Guid? userId, CancellationToken ct = default)
    {
        var record = await _db.ComplianceRecords.FindAsync([id], ct);
        if (record is null) return false;
        record.IsDeleted = true;
        record.UpdatedAt = DateTime.UtcNow;
        record.UpdatedBy = userId;
        await _db.SaveChangesAsync(ct);
        await _audit.LogAsync(userId, "", "Delete", "Compliance", "ComplianceRecord", record.Id.ToString(), newValues: record.Title, ct: ct);
        return true;
    }

    private static ComplianceRecordDto Map(ComplianceRecord r) => new(
        r.Id,
        r.Title,
        FormatType(r.Type),
        r.EntityType,
        r.EntityId,
        r.ProjectId,
        r.Project?.Name,
        r.IssueDate.ToString("dd MMM yyyy"),
        r.ExpiryDate?.ToString("dd MMM yyyy"),
        FormatStatus(r.Status),
        r.ResponsiblePerson,
        r.Notes
    );

    private static ComplianceRecordType ParseType(string? value) => value?.ToLowerInvariant() switch
    {
        "insurance" => ComplianceRecordType.Insurance,
        "license" => ComplianceRecordType.License,
        "certificate" => ComplianceRecordType.Certificate,
        "permit" => ComplianceRecordType.Permit,
        "inspection" => ComplianceRecordType.Inspection,
        _ => ComplianceRecordType.Other,
    };

    private static ComplianceRecordStatus ParseStatus(string? value) => value?.ToLowerInvariant() switch
    {
        "expiringsoon" or "expiring soon" => ComplianceRecordStatus.ExpiringSoon,
        "expired" => ComplianceRecordStatus.Expired,
        "pending" => ComplianceRecordStatus.Pending,
        "revoked" => ComplianceRecordStatus.Revoked,
        _ => ComplianceRecordStatus.Valid,
    };

    private static string FormatType(ComplianceRecordType t) => t.ToString();
    private static string FormatStatus(ComplianceRecordStatus s) => s switch
    {
        ComplianceRecordStatus.ExpiringSoon => "Expiring Soon",
        _ => s.ToString(),
    };
}
