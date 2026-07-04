using Microsoft.EntityFrameworkCore;
using Topzinto.Erp.Application.DTOs.Financial;
using Topzinto.Erp.Application.Interfaces;
using Topzinto.Erp.Domain.Entities;
using Topzinto.Erp.Domain.Enums;
using Topzinto.Erp.Infrastructure.Common;
using Topzinto.Erp.Infrastructure.Persistence;

namespace Topzinto.Erp.Infrastructure.Services;

public class BoqService : IBoqService
{
    private readonly AppDbContext _db;
    private readonly IAuditService _audit;

    public BoqService(AppDbContext db, IAuditService audit)
    {
        _db = db;
        _audit = audit;
    }

    public async Task<BoqSummaryDto> GetSummaryAsync(CancellationToken ct = default)
    {
        var items = await _db.BoqItems.ToListAsync(ct);
        return new BoqSummaryDto(
            items.Sum(i => i.Amount),
            items.Count,
            items.Select(i => i.ProjectId).Distinct().Count(),
            items.Count > 0 ? items.Average(i => i.Rate) : 0
        );
    }

    public async Task<IReadOnlyList<BoqItemDto>> GetAllAsync(Guid? projectId = null, string? search = null, CancellationToken ct = default)
    {
        var query = _db.BoqItems.Include(i => i.Project).AsQueryable();
        if (projectId.HasValue)
            query = query.Where(i => i.ProjectId == projectId.Value);
        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(i => i.Description.Contains(search) || i.ItemCode.Contains(search) || i.Category.Contains(search));

        var list = await query.OrderBy(i => i.ItemCode).ToListAsync(ct);
        return list.Select(i => new BoqItemDto(
            i.Id, i.ItemCode, i.Description, i.Category, i.Unit,
            i.Quantity, i.Rate, i.Amount, i.ProjectId, i.Project.Name
        )).ToList();
    }

    public async Task<BoqItemDto> CreateAsync(CreateBoqItemRequest request, Guid? userId, CancellationToken ct = default)
    {
        var amount = request.Quantity * request.Rate;
        var item = new BoqItem
        {
            ProjectId = request.ProjectId,
            ItemCode = request.ItemCode.Trim(),
            Description = request.Description.Trim(),
            Category = request.Category.Trim(),
            Unit = request.Unit.Trim(),
            Quantity = request.Quantity,
            Rate = request.Rate,
            Amount = amount,
            Notes = request.Notes?.Trim(),
            CreatedBy = userId,
        };
        _db.BoqItems.Add(item);
        await _db.SaveChangesAsync(ct);
        await _audit.LogAsync(userId, "", "Create", "Financial", "BoqItem", item.Id.ToString(), newValues: item.ItemCode, ct: ct);
        return (await GetAllAsync(item.ProjectId, null, ct)).First(i => i.Id == item.Id);
    }
}

public class ClaimsService : IClaimsService
{
    private readonly AppDbContext _db;
    private readonly IAuditService _audit;

    public ClaimsService(AppDbContext db, IAuditService audit)
    {
        _db = db;
        _audit = audit;
    }

    public async Task<IReadOnlyList<ClaimDto>> GetAllAsync(Guid? projectId = null, string? status = null, CancellationToken ct = default)
    {
        var query = _db.Claims.Include(c => c.Project).AsQueryable();
        if (projectId.HasValue)
            query = query.Where(c => c.ProjectId == projectId.Value);
        if (!string.IsNullOrWhiteSpace(status))
        {
            var s = FinancialDisplay.ParseClaimStatus(status);
            query = query.Where(c => c.Status == s);
        }

        var list = await query.OrderByDescending(c => c.ClaimDate).ToListAsync(ct);
        return list.Select(c => new ClaimDto(
            c.Id, c.ClaimNumber, c.Title, c.Project.Name,
            FinancialDisplay.FormatClaimStatus(c.Status), c.Amount,
            FinancialDisplay.FormatDate(c.ClaimDate),
            FinancialDisplay.FormatDate(c.PeriodFrom),
            FinancialDisplay.FormatDate(c.PeriodTo)
        )).ToList();
    }

    public async Task<ClaimDto> CreateAsync(CreateClaimRequest request, Guid? userId, CancellationToken ct = default)
    {
        var claim = new Claim
        {
            ProjectId = request.ProjectId,
            ClaimNumber = request.ClaimNumber.Trim(),
            Title = request.Title.Trim(),
            ClaimDate = FinancialDisplay.ParseDateTime(request.ClaimDate) ?? DateTime.UtcNow.Date,
            PeriodFrom = FinancialDisplay.ParseDateTime(request.PeriodFrom),
            PeriodTo = FinancialDisplay.ParseDateTime(request.PeriodTo),
            Amount = request.Amount,
            Status = FinancialDisplay.ParseClaimStatus(request.Status),
            SubmittedByName = request.SubmittedByName?.Trim(),
            Notes = request.Notes?.Trim(),
            CreatedBy = userId,
        };
        _db.Claims.Add(claim);
        await _db.SaveChangesAsync(ct);
        await _audit.LogAsync(userId, "", "Create", "Financial", "Claim", claim.Id.ToString(), newValues: claim.ClaimNumber, ct: ct);
        var list = await GetAllAsync(claim.ProjectId, null, ct);
        return list.First(c => c.Id == claim.Id);
    }
}

public class InvoiceService : IInvoiceService
{
    private readonly AppDbContext _db;

    public InvoiceService(AppDbContext db) => _db = db;

    public async Task<IReadOnlyList<InvoiceDto>> GetAllAsync(Guid? projectId = null, string? status = null, CancellationToken ct = default)
    {
        var query = _db.Invoices.Include(i => i.Project).ThenInclude(p => p.Client).AsQueryable();
        if (projectId.HasValue)
            query = query.Where(i => i.ProjectId == projectId.Value);
        if (!string.IsNullOrWhiteSpace(status))
        {
            var s = FinancialDisplay.ParseInvoiceStatus(status);
            query = query.Where(i => i.Status == s);
        }

        var list = await query.OrderByDescending(i => i.InvoiceDate).ToListAsync(ct);
        return list.Select(i => new InvoiceDto(
            i.Id, i.InvoiceNumber, i.Project.Name, i.Project.Client.Name,
            FinancialDisplay.FormatInvoiceStatus(i.Status), i.Amount,
            FinancialDisplay.FormatDate(i.InvoiceDate),
            FinancialDisplay.FormatDate(i.DueDate)
        )).ToList();
    }
}

public class FinancialService : IFinancialService
{
    private readonly AppDbContext _db;

    public FinancialService(AppDbContext db) => _db = db;

    public async Task<FinancialSummaryDto> GetSummaryAsync(CancellationToken ct = default)
    {
        var boqItems = await _db.BoqItems.ToListAsync(ct);
        var claims = await _db.Claims.ToListAsync(ct);
        var invoices = await _db.Invoices.ToListAsync(ct);
        var boqTotal = boqItems.Sum(i => i.Amount);

        return new FinancialSummaryDto(
            boqTotal,
            claims.Where(c => c.Status is ClaimStatus.Submitted or ClaimStatus.Approved).Sum(c => c.Amount),
            claims.Where(c => c.Status == ClaimStatus.Paid).Sum(c => c.Amount),
            invoices.Where(i => i.Status is InvoiceStatus.Sent or InvoiceStatus.Overdue).Sum(i => i.Amount),
            invoices.Where(i => i.Status == InvoiceStatus.Paid).Sum(i => i.Amount)
        );
    }
}
