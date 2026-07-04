using Microsoft.EntityFrameworkCore;
using Topzinto.Erp.Application.DTOs.Suppliers;
using Topzinto.Erp.Application.Interfaces;
using Topzinto.Erp.Domain.Entities;
using Topzinto.Erp.Infrastructure.Common;
using Topzinto.Erp.Infrastructure.Persistence;

namespace Topzinto.Erp.Infrastructure.Services;

public class SupplierService : ISupplierService
{
    private readonly AppDbContext _db;
    private readonly IAuditService _audit;

    public SupplierService(AppDbContext db, IAuditService audit)
    {
        _db = db;
        _audit = audit;
    }

    public async Task<IReadOnlyList<SupplierDto>> GetAllAsync(string? search = null, string? status = null, CancellationToken ct = default)
    {
        var query = _db.Suppliers.Include(s => s.PurchaseOrders).AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(s => s.Name.Contains(search) || s.Code.Contains(search) || (s.ContactPerson != null && s.ContactPerson.Contains(search)));

        if (!string.IsNullOrWhiteSpace(status))
        {
            var s = ProcurementDisplay.ParseSupplierStatus(status);
            query = query.Where(x => x.Status == s);
        }

        var list = await query.OrderBy(s => s.Name).ToListAsync(ct);
        return list.Select(MapList).ToList();
    }

    public async Task<SupplierDetailDto?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var supplier = await _db.Suppliers
            .Include(s => s.PurchaseOrders).ThenInclude(po => po.Project)
            .FirstOrDefaultAsync(s => s.Id == id, ct);
        return supplier is null ? null : MapDetail(supplier);
    }

    public async Task<SupplierDetailDto> CreateAsync(CreateSupplierRequest request, Guid? userId, CancellationToken ct = default)
    {
        var supplier = MapToEntity(new Supplier(), request);
        supplier.CreatedBy = userId;
        _db.Suppliers.Add(supplier);
        await _db.SaveChangesAsync(ct);
        await _audit.LogAsync(userId, "", "Create", "Procurement", "Supplier", supplier.Id.ToString(), newValues: supplier.Name, ct: ct);
        return (await GetByIdAsync(supplier.Id, ct))!;
    }

    public async Task<SupplierDetailDto?> UpdateAsync(Guid id, UpdateSupplierRequest request, Guid? userId, CancellationToken ct = default)
    {
        var supplier = await _db.Suppliers.FindAsync([id], ct);
        if (supplier is null) return null;
        MapToEntity(supplier, request);
        supplier.UpdatedAt = DateTime.UtcNow;
        supplier.UpdatedBy = userId;
        await _db.SaveChangesAsync(ct);
        await _audit.LogAsync(userId, "", "Update", "Procurement", "Supplier", supplier.Id.ToString(), newValues: supplier.Name, ct: ct);
        return await GetByIdAsync(id, ct);
    }

    private static SupplierDto MapList(Supplier s) => new(
        s.Id, s.Code, s.Name,
        ProcurementDisplay.FormatSupplierCategory(s.Category),
        ProcurementDisplay.FormatSupplierStatus(s.Status),
        s.ContactPerson, s.Phone, s.Email, s.City,
        s.PurchaseOrders.Count(po => !po.IsDeleted)
    );

    private static SupplierDetailDto MapDetail(Supplier s) => new(
        s.Id, s.Code, s.Name,
        ProcurementDisplay.FormatSupplierCategory(s.Category),
        ProcurementDisplay.FormatSupplierStatus(s.Status),
        s.ContactPerson, s.Phone, s.Email, s.Address, s.City, s.Province, s.VatNumber, s.Notes,
        s.PurchaseOrders.Where(po => !po.IsDeleted).OrderByDescending(po => po.OrderDate).Take(10)
            .Select(po => new SupplierPoDto(
                po.Id, po.PoNumber, po.Title,
                ProcurementDisplay.FormatPoStatus(po.Status),
                po.Project?.Name, po.TotalAmount,
                ProcurementDisplay.FormatDate(po.OrderDate)
            )).ToList()
    );

    private static Supplier MapToEntity(Supplier s, CreateSupplierRequest r)
    {
        s.Code = r.Code;
        s.Name = r.Name;
        s.Category = ProcurementDisplay.ParseSupplierCategory(r.Category);
        s.Status = ProcurementDisplay.ParseSupplierStatus(r.Status);
        s.ContactPerson = r.ContactPerson;
        s.Phone = r.Phone;
        s.Email = r.Email;
        s.Address = r.Address;
        s.City = r.City;
        s.Province = r.Province;
        s.VatNumber = r.VatNumber;
        s.Notes = r.Notes;
        return s;
    }

    private static Supplier MapToEntity(Supplier s, UpdateSupplierRequest r) =>
        MapToEntity(s, new CreateSupplierRequest(r.Code, r.Name, r.Category, r.Status, r.ContactPerson, r.Phone, r.Email, r.Address, r.City, r.Province, r.VatNumber, r.Notes));
}
