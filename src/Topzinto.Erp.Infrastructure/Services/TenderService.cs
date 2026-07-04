using Microsoft.EntityFrameworkCore;
using Topzinto.Erp.Application.DTOs.Tenders;
using Topzinto.Erp.Application.Interfaces;
using Topzinto.Erp.Domain.Entities;
using Topzinto.Erp.Infrastructure.Common;
using Topzinto.Erp.Infrastructure.Persistence;

namespace Topzinto.Erp.Infrastructure.Services;

public class TenderService : ITenderService
{
    private readonly AppDbContext _db;
    private readonly IAuditService _audit;

    public TenderService(AppDbContext db, IAuditService audit)
    {
        _db = db;
        _audit = audit;
    }

    public async Task<IReadOnlyList<TenderDto>> GetAllAsync(string? search = null, string? status = null, CancellationToken ct = default)
    {
        var query = _db.Tenders.Include(t => t.Client).AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(t => t.Title.Contains(search) || t.ReferenceNumber.Contains(search));

        if (!string.IsNullOrWhiteSpace(status))
        {
            var s = EnumDisplay.ParseTenderStatus(status);
            query = query.Where(t => t.Status == s);
        }

        return await query
            .OrderBy(t => t.ClosingDate)
            .Select(t => new TenderDto(
                t.Id,
                t.ReferenceNumber,
                t.Title,
                t.Client.Name,
                t.ClientId,
                EnumDisplay.FormatTenderStatus(t.Status),
                t.ClosingDate.ToString("dd MMM yyyy"),
                t.EstimatedValue
            ))
            .ToListAsync(ct);
    }

    public async Task<TenderDetailDto?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var t = await _db.Tenders.Include(x => x.Client).FirstOrDefaultAsync(x => x.Id == id, ct);
        return t is null ? null : MapDetail(t);
    }

    public async Task<TenderDetailDto> CreateAsync(CreateTenderRequest request, Guid? userId, CancellationToken ct = default)
    {
        var tender = new Tender
        {
            ReferenceNumber = request.ReferenceNumber,
            Title = request.Title,
            ClientId = request.ClientId,
            ClosingDate = request.ClosingDate,
            Status = EnumDisplay.ParseTenderStatus(request.Status),
            EstimatedValue = request.EstimatedValue,
            Notes = request.Notes,
            CreatedBy = userId,
        };

        _db.Tenders.Add(tender);
        await _db.SaveChangesAsync(ct);
        await _db.Entry(tender).Reference(t => t.Client).LoadAsync(ct);
        await _audit.LogAsync(userId, "", "Create", "Tenders", "Tender", tender.Id.ToString(), newValues: tender.Title, ct: ct);
        return MapDetail(tender);
    }

    public async Task<TenderDetailDto?> UpdateAsync(Guid id, UpdateTenderRequest request, Guid? userId, CancellationToken ct = default)
    {
        var tender = await _db.Tenders.Include(t => t.Client).FirstOrDefaultAsync(t => t.Id == id, ct);
        if (tender is null) return null;

        tender.ReferenceNumber = request.ReferenceNumber;
        tender.Title = request.Title;
        tender.ClientId = request.ClientId;
        tender.ClosingDate = request.ClosingDate;
        tender.Status = EnumDisplay.ParseTenderStatus(request.Status);
        tender.EstimatedValue = request.EstimatedValue;
        tender.Notes = request.Notes;
        tender.UpdatedAt = DateTime.UtcNow;
        tender.UpdatedBy = userId;

        await _db.SaveChangesAsync(ct);
        return MapDetail(tender);
    }

    public async Task<bool> DeleteAsync(Guid id, Guid? userId, CancellationToken ct = default)
    {
        var tender = await _db.Tenders.FindAsync([id], ct);
        if (tender is null) return false;
        tender.IsDeleted = true;
        tender.UpdatedAt = DateTime.UtcNow;
        tender.UpdatedBy = userId;
        await _db.SaveChangesAsync(ct);
        return true;
    }

    private static TenderDetailDto MapDetail(Tender t) => new(
        t.Id,
        t.ReferenceNumber,
        t.Title,
        t.ClientId,
        t.Client.Name,
        t.ClosingDate.ToString("dd MMM yyyy"),
        EnumDisplay.FormatTenderStatus(t.Status),
        t.EstimatedValue,
        t.Notes
    );
}
