using Microsoft.EntityFrameworkCore;
using Topzinto.Erp.Application.DTOs.Equipment;
using Topzinto.Erp.Application.Interfaces;
using Topzinto.Erp.Domain.Entities;
using Topzinto.Erp.Domain.Enums;
using Topzinto.Erp.Infrastructure.Common;
using Topzinto.Erp.Infrastructure.Persistence;

namespace Topzinto.Erp.Infrastructure.Services;

public class EquipmentService : IEquipmentService
{
    private readonly AppDbContext _db;
    private readonly IAuditService _audit;

    public EquipmentService(AppDbContext db, IAuditService audit)
    {
        _db = db;
        _audit = audit;
    }

    public async Task<EquipmentSummaryDto> GetSummaryAsync(CancellationToken ct = default)
    {
        var items = await _db.Equipment.ToListAsync(ct);
        var soon = DateTime.UtcNow.Date.AddDays(30);
        return new EquipmentSummaryDto(
            items.Count,
            items.Count(e => e.Status == EquipmentStatus.Available),
            items.Count(e => e.Status == EquipmentStatus.InUse),
            items.Count(e => e.Status == EquipmentStatus.Maintenance),
            items.Count(e => e.NextInspectionDue <= soon || e.NextServiceDue <= soon)
        );
    }

    public async Task<IReadOnlyList<EquipmentDto>> GetAllAsync(string? search = null, string? status = null, CancellationToken ct = default)
    {
        var query = _db.Equipment.Include(e => e.AssignedProject).AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(e => e.Name.Contains(search) || e.AssetTag.Contains(search) || (e.OperatorName != null && e.OperatorName.Contains(search)));

        if (!string.IsNullOrWhiteSpace(status))
        {
            var s = AssetDisplay.ParseEquipmentStatus(status);
            query = query.Where(e => e.Status == s);
        }

        var list = await query.OrderBy(e => e.Name).ToListAsync(ct);
        return list.Select(MapList).ToList();
    }

    public async Task<EquipmentDetailDto?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var e = await _db.Equipment
            .Include(x => x.AssignedProject)
            .Include(x => x.Bookings).ThenInclude(b => b.Project)
            .Include(x => x.Inspections)
            .FirstOrDefaultAsync(x => x.Id == id, ct);
        return e is null ? null : MapDetail(e);
    }

    public async Task<EquipmentDetailDto> CreateAsync(CreateEquipmentRequest request, Guid? userId, CancellationToken ct = default)
    {
        var equipment = MapToEntity(new Equipment(), request);
        equipment.CreatedBy = userId;
        _db.Equipment.Add(equipment);
        await _db.SaveChangesAsync(ct);
        await _audit.LogAsync(userId, "", "Create", "Equipment", "Equipment", equipment.Id.ToString(), newValues: equipment.Name, ct: ct);
        return (await GetByIdAsync(equipment.Id, ct))!;
    }

    public async Task<EquipmentDetailDto?> UpdateAsync(Guid id, UpdateEquipmentRequest request, Guid? userId, CancellationToken ct = default)
    {
        var equipment = await _db.Equipment.FindAsync([id], ct);
        if (equipment is null) return null;
        MapToEntity(equipment, request);
        equipment.UpdatedAt = DateTime.UtcNow;
        equipment.UpdatedBy = userId;
        await _db.SaveChangesAsync(ct);
        return await GetByIdAsync(id, ct);
    }

    private static Equipment MapToEntity(Equipment e, CreateEquipmentRequest r)
    {
        e.AssetTag = r.AssetTag;
        e.Name = r.Name;
        e.Category = AssetDisplay.ParseEquipmentCategory(r.Category);
        e.Status = AssetDisplay.ParseEquipmentStatus(r.Status);
        e.MakeModel = r.MakeModel;
        e.SerialNumber = r.SerialNumber;
        e.OperatorName = r.OperatorName;
        e.NextInspectionDue = r.NextInspectionDue;
        e.NextServiceDue = r.NextServiceDue;
        e.AssignedProjectId = r.AssignedProjectId;
        e.Notes = r.Notes;
        return e;
    }

    private static Equipment MapToEntity(Equipment e, UpdateEquipmentRequest r) =>
        MapToEntity(e, new CreateEquipmentRequest(
            r.AssetTag, r.Name, r.Category, r.Status, r.MakeModel, r.SerialNumber,
            r.OperatorName, r.NextInspectionDue, r.NextServiceDue, r.AssignedProjectId, r.Notes));

    private static EquipmentDto MapList(Equipment e) => new(
        e.Id, e.AssetTag, e.Name,
        AssetDisplay.FormatEquipmentCategory(e.Category),
        AssetDisplay.FormatEquipmentStatus(e.Status),
        e.OperatorName, e.AssignedProject?.Name,
        e.NextServiceDue?.ToString("dd MMM yyyy"),
        AssetDisplay.IsExpiringSoon(e.NextInspectionDue) || AssetDisplay.IsExpiringSoon(e.NextServiceDue)
    );

    private static EquipmentDetailDto MapDetail(Equipment e) => new(
        e.Id, e.AssetTag, e.Name,
        AssetDisplay.FormatEquipmentCategory(e.Category),
        AssetDisplay.FormatEquipmentStatus(e.Status),
        e.MakeModel, e.SerialNumber, e.OperatorName,
        e.LastInspectionDate?.ToString("dd MMM yyyy"),
        e.NextInspectionDue?.ToString("dd MMM yyyy"),
        e.LastServiceDate?.ToString("dd MMM yyyy"),
        e.NextServiceDue?.ToString("dd MMM yyyy"),
        e.AssignedProjectId, e.AssignedProject?.Name, e.Notes,
        e.Bookings.OrderByDescending(b => b.StartDate).Select(b => new BookingDto(
            b.Id, b.Project.Name, b.StartDate.ToString("dd MMM yyyy"), b.EndDate.ToString("dd MMM yyyy"), b.BookedByName)).ToList(),
        e.Inspections.OrderByDescending(i => i.InspectionDate).Select(i => new InspectionDto(
            i.Id, i.InspectionDate.ToString("dd MMM yyyy"), i.Result, i.InspectorName,
            i.NextDueDate?.ToString("dd MMM yyyy"))).ToList(),
        e.NextInspectionDue?.ToString("yyyy-MM-dd"),
        e.NextServiceDue?.ToString("yyyy-MM-dd")
    );
}
