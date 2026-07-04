using Microsoft.EntityFrameworkCore;
using Topzinto.Erp.Application.DTOs.Procurement;
using Topzinto.Erp.Application.Interfaces;
using Topzinto.Erp.Domain.Entities;
using Topzinto.Erp.Domain.Enums;
using Topzinto.Erp.Infrastructure.Common;
using Topzinto.Erp.Infrastructure.Persistence;

namespace Topzinto.Erp.Infrastructure.Services;

public class ProcurementService : IProcurementService
{
    private readonly AppDbContext _db;
    private readonly IAuditService _audit;

    public ProcurementService(AppDbContext db, IAuditService audit)
    {
        _db = db;
        _audit = audit;
    }

    public async Task<ProcurementSummaryDto> GetSummaryAsync(CancellationToken ct = default)
    {
        var orders = await _db.PurchaseOrders.ToListAsync(ct);
        return new ProcurementSummaryDto(
            orders.Count,
            orders.Count(o => o.Status == PoStatus.PendingApproval),
            orders.Count(o => o.Status == PoStatus.Approved || o.Status == PoStatus.Ordered),
            orders.Count(o => o.Status == PoStatus.Delivered),
            orders.Sum(o => o.TotalAmount)
        );
    }

    public async Task<IReadOnlyList<PurchaseOrderDto>> GetAllAsync(string? search = null, string? status = null, CancellationToken ct = default)
    {
        var query = _db.PurchaseOrders
            .Include(po => po.Supplier)
            .Include(po => po.Project)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(po =>
                po.PoNumber.Contains(search) || po.Title.Contains(search) ||
                po.Supplier.Name.Contains(search) || (po.Project != null && po.Project.Name.Contains(search)));

        if (!string.IsNullOrWhiteSpace(status))
        {
            var s = ProcurementDisplay.ParsePoStatus(status);
            query = query.Where(po => po.Status == s);
        }

        var list = await query.OrderByDescending(po => po.OrderDate).ToListAsync(ct);
        return list.Select(MapList).ToList();
    }

    public async Task<PurchaseOrderDetailDto?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var po = await _db.PurchaseOrders
            .Include(x => x.Supplier)
            .Include(x => x.Project)
            .Include(x => x.Lines)
            .FirstOrDefaultAsync(x => x.Id == id, ct);
        return po is null ? null : MapDetail(po);
    }

    public async Task<PurchaseOrderDetailDto> CreateAsync(CreatePurchaseOrderRequest request, Guid? userId, CancellationToken ct = default)
    {
        var po = new PurchaseOrder
        {
            PoNumber = request.PoNumber,
            Title = request.Title,
            SupplierId = request.SupplierId,
            ProjectId = request.ProjectId,
            Status = ProcurementDisplay.ParsePoStatus(request.Status),
            OrderDate = request.OrderDate,
            RequiredDate = request.RequiredDate,
            RequestedByName = request.RequestedByName,
            Notes = request.Notes,
            CreatedBy = userId,
        };

        foreach (var line in request.Lines)
            po.Lines.Add(CreateLine(line));

        po.TotalAmount = po.Lines.Sum(l => l.LineTotal);
        _db.PurchaseOrders.Add(po);
        await _db.SaveChangesAsync(ct);
        await _audit.LogAsync(userId, "", "Create", "Procurement", "PurchaseOrder", po.Id.ToString(), newValues: po.PoNumber, ct: ct);
        return (await GetByIdAsync(po.Id, ct))!;
    }

    public async Task<PurchaseOrderDetailDto?> UpdateAsync(Guid id, UpdatePurchaseOrderRequest request, Guid? userId, CancellationToken ct = default)
    {
        var po = await _db.PurchaseOrders.Include(x => x.Lines).FirstOrDefaultAsync(x => x.Id == id, ct);
        if (po is null) return null;

        po.Title = request.Title;
        po.SupplierId = request.SupplierId;
        po.ProjectId = request.ProjectId;
        po.Status = ProcurementDisplay.ParsePoStatus(request.Status);
        po.OrderDate = request.OrderDate;
        po.RequiredDate = request.RequiredDate;
        po.RequestedByName = request.RequestedByName;
        po.ApprovedByName = request.ApprovedByName;
        po.Notes = request.Notes;
        po.UpdatedAt = DateTime.UtcNow;
        po.UpdatedBy = userId;

        _db.PurchaseOrderLines.RemoveRange(po.Lines);
        po.Lines.Clear();
        foreach (var line in request.Lines)
            po.Lines.Add(CreateLine(line));

        po.TotalAmount = po.Lines.Sum(l => l.LineTotal);
        await _db.SaveChangesAsync(ct);
        await _audit.LogAsync(userId, "", "Update", "Procurement", "PurchaseOrder", po.Id.ToString(), newValues: po.PoNumber, ct: ct);
        return await GetByIdAsync(id, ct);
    }

    private static PurchaseOrderLine CreateLine(CreatePurchaseOrderLineRequest line)
    {
        var qty = line.Quantity;
        var price = line.UnitPrice;
        return new PurchaseOrderLine
        {
            Description = line.Description,
            Quantity = qty,
            Unit = line.Unit,
            UnitPrice = price,
            LineTotal = qty * price,
        };
    }

    private static PurchaseOrderDto MapList(PurchaseOrder po) => new(
        po.Id, po.PoNumber, po.Title, po.Supplier.Name, po.Project?.Name,
        ProcurementDisplay.FormatPoStatus(po.Status), po.TotalAmount,
        ProcurementDisplay.FormatDate(po.OrderDate),
        ProcurementDisplay.FormatDate(po.RequiredDate)
    );

    private static PurchaseOrderDetailDto MapDetail(PurchaseOrder po) => new(
        po.Id, po.PoNumber, po.Title, po.SupplierId, po.Supplier.Name,
        po.ProjectId, po.Project?.Name,
        ProcurementDisplay.FormatPoStatus(po.Status), po.TotalAmount,
        ProcurementDisplay.FormatDate(po.OrderDate),
        ProcurementDisplay.FormatDate(po.RequiredDate),
        po.RequestedByName, po.ApprovedByName, po.Notes,
        po.Lines.OrderBy(l => l.CreatedAt).Select(l => new PurchaseOrderLineDto(
            l.Id, l.Description, l.Quantity, l.Unit, l.UnitPrice, l.LineTotal
        )).ToList()
    );
}
