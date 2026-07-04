using Microsoft.EntityFrameworkCore;
using Topzinto.Erp.Application.DTOs.Dashboard;
using Topzinto.Erp.Application.Interfaces;
using Topzinto.Erp.Domain.Enums;
using Topzinto.Erp.Infrastructure.Common;
using Topzinto.Erp.Infrastructure.Persistence;

namespace Topzinto.Erp.Infrastructure.Services;

public class DashboardService : IDashboardService
{
    private readonly AppDbContext _db;

    public DashboardService(AppDbContext db) => _db = db;

    public async Task<DashboardDto> GetAsync(bool refreshCache = false, CancellationToken ct = default)
    {
        var projects = await _db.Projects.ToListAsync(ct);
        var tenders = await _db.Tenders.ToListAsync(ct);
        var claims = await _db.Claims.ToListAsync(ct);
        var purchaseOrders = await _db.PurchaseOrders.ToListAsync(ct);
        var contracts = await _db.Contracts.Where(c => c.Status == ContractStatus.Active).ToListAsync(ct);
        var vehicles = await _db.Vehicles.ToListAsync(ct);
        var equipment = await _db.Equipment.ToListAsync(ct);
        var today = DateTime.UtcNow.Date;

        var overdueTasks = await _db.ProjectTasks.CountAsync(t =>
            t.Status != ProjectTaskStatus.Completed &&
            t.DueDate.HasValue && t.DueDate.Value.Date < today, ct);

        var activeUsers = await _db.Users.CountAsync(u => u.IsActive, ct);

        var reports = await _db.SiteReports
            .Include(r => r.Project)
            .OrderByDescending(r => r.ReportDate)
            .Take(5)
            .ToListAsync(ct);

        var soon = DateTime.UtcNow.Date.AddDays(30);
        var docsExpiring = await _db.Documents.CountAsync(d =>
            d.ExpiryDate != null && d.ExpiryDate.Value.Date <= soon && d.ExpiryDate.Value.Date >= DateTime.UtcNow.Date, ct);

        return new DashboardDto(
            projects.Count(p => p.Status == ProjectStatus.Active),
            tenders.Count(t => t.Status is TenderStatus.Identified or TenderStatus.Preparing or TenderStatus.Submitted),
            contracts.Sum(c => c.Value),
            claims.Where(c => c.Status is ClaimStatus.Submitted or ClaimStatus.Approved).Sum(c => c.Amount),
            vehicles.Count(v => v.Status == VehicleStatus.InUse),
            vehicles.Count,
            equipment.Count(e => e.Status == EquipmentStatus.InUse),
            equipment.Count,
            activeUsers,
            overdueTasks,
            docsExpiring,
            new ProjectProgressDto(
                projects.Count(p => p.Status == ProjectStatus.Completed),
                projects.Count(p => p.Status == ProjectStatus.Active),
                projects.Count(p => p.Status == ProjectStatus.OnHold),
                projects.Count(p => p.Status == ProjectStatus.Planned)
            ),
            reports.Select(r => new RecentSiteReportDto(
                r.Id,
                r.Project.Name,
                r.ReportDate.ToString("dd MMM yyyy"),
                r.Status.ToString()
            )).ToList(),
            BuildFinancialTrend(claims, purchaseOrders)
        );
    }

    private static IReadOnlyList<FinancialTrendPointDto> BuildFinancialTrend(
        IReadOnlyList<Domain.Entities.Claim> claims,
        IReadOnlyList<Domain.Entities.PurchaseOrder> purchaseOrders)
    {
        var today = DateTime.UtcNow.Date;
        var startMonth = new DateTime(today.Year, today.Month, 1).AddMonths(-5);
        var points = new List<FinancialTrendPointDto>();

        for (var i = 0; i < 6; i++)
        {
            var monthStart = startMonth.AddMonths(i);
            var monthEnd = monthStart.AddMonths(1);
            var claimsAmount = claims
                .Where(c => c.ClaimDate.Date >= monthStart && c.ClaimDate.Date < monthEnd)
                .Sum(c => c.Amount);
            var procurementAmount = purchaseOrders
                .Where(p => p.OrderDate.Date >= monthStart && p.OrderDate.Date < monthEnd)
                .Sum(p => p.TotalAmount);
            points.Add(new FinancialTrendPointDto(
                monthStart.ToString("MMM yy"),
                claimsAmount,
                procurementAmount));
        }

        return points;
    }
}

public class ReportsService : IReportsService
{
    private readonly AppDbContext _db;

    public ReportsService(AppDbContext db) => _db = db;

    public async Task<ReportsHubDto> GetHubAsync(CancellationToken ct = default)
    {
        var projectCount = await _db.Projects.CountAsync(ct);
        var contracts = await _db.Contracts.ToListAsync(ct);
        var purchaseOrders = await _db.PurchaseOrders.ToListAsync(ct);
        var fleetCount = await _db.Vehicles.CountAsync(ct);
        var siteReports = await _db.SiteReports.CountAsync(ct);
        var boqItems = await _db.BoqItems.ToListAsync(ct);
        var pendingClaimsList = await _db.Claims
            .Where(c => c.Status == ClaimStatus.Submitted || c.Status == ClaimStatus.Approved)
            .ToListAsync(ct);

        var contractValue = contracts.Sum(c => c.Value);
        var poValue = purchaseOrders.Sum(p => p.TotalAmount);
        var boqValue = boqItems.Sum(b => b.Amount);
        var pendingClaims = pendingClaimsList.Sum(c => c.Amount);

        return new ReportsHubDto([
            new ReportCardDto("projects", "Project Portfolio", "Active projects and progress summary", $"{projectCount} projects", "/projects"),
            new ReportCardDto("financial", "Financial Summary", "Contracts, BOQ and claims overview", FormatMoney(contractValue), "/boq"),
            new ReportCardDto("procurement", "Procurement Spend", "Purchase orders and supplier spend", FormatMoney(poValue), "/procurement"),
            new ReportCardDto("fleet", "Fleet Utilisation", "Vehicles and equipment status", $"{fleetCount} vehicles", "/fleet"),
            new ReportCardDto("site", "Site Reports", "Daily site reporting register", $"{siteReports} reports", "/site-reports"),
            new ReportCardDto("boq", "BOQ & Costing", "Bill of quantities and cost tracking", FormatMoney(boqValue), "/boq"),
            new ReportCardDto("claims", "Claims Register", "Pending and approved claims", FormatMoney(pendingClaims), "/boq"),
        ]);
    }

    private static string FormatMoney(decimal amount) =>
        amount >= 1_000_000 ? $"R{amount / 1_000_000:0.#}M" : $"R{amount:N0}";
}
