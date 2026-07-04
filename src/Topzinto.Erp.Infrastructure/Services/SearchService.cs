using Microsoft.EntityFrameworkCore;
using Topzinto.Erp.Application.DTOs.Search;
using Topzinto.Erp.Application.Interfaces;
using Topzinto.Erp.Infrastructure.Persistence;

namespace Topzinto.Erp.Infrastructure.Services;

public class SearchService : ISearchService
{
    private readonly AppDbContext _db;

    public SearchService(AppDbContext db) => _db = db;

    public async Task<IReadOnlyList<SearchResultDto>> SearchAsync(string query, int limit = 20, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(query) || query.Length < 2)
            return [];

        var q = query.Trim();
        var results = new List<SearchResultDto>();

        var projects = await _db.Projects
            .Where(p => p.Name.Contains(q) || p.Code.Contains(q))
            .Take(5)
            .ToListAsync(ct);
        results.AddRange(projects.Select(p => new SearchResultDto(
            "Project", p.Id, p.Name, p.Code, $"/projects/{p.Id}")));

        var clients = await _db.Clients
            .Where(c => c.Name.Contains(q))
            .Take(5)
            .ToListAsync(ct);
        results.AddRange(clients.Select(c => new SearchResultDto(
            "Client", c.Id, c.Name, c.City ?? c.Type.ToString(), $"/clients/{c.Id}")));

        var suppliers = await _db.Suppliers
            .Where(s => s.Name.Contains(q) || s.Code.Contains(q))
            .Take(5)
            .ToListAsync(ct);
        results.AddRange(suppliers.Select(s => new SearchResultDto(
            "Supplier", s.Id, s.Name, s.Code, $"/suppliers/{s.Id}")));

        var documents = await _db.Documents
            .Where(d => d.Title.Contains(q) || (d.FileName != null && d.FileName.Contains(q)))
            .Take(5)
            .ToListAsync(ct);
        results.AddRange(documents.Select(d => new SearchResultDto(
            "Document", d.Id, d.Title, d.Category, "/documents")));

        var pos = await _db.PurchaseOrders
            .Include(p => p.Supplier)
            .Where(p => p.PoNumber.Contains(q) || p.Title.Contains(q))
            .Take(5)
            .ToListAsync(ct);
        results.AddRange(pos.Select(p => new SearchResultDto(
            "Purchase Order", p.Id, p.PoNumber, p.Title, $"/procurement/{p.Id}")));

        var vehicles = await _db.Vehicles
            .Where(v => v.RegistrationNumber.Contains(q) || v.MakeModel.Contains(q))
            .Take(5)
            .ToListAsync(ct);
        results.AddRange(vehicles.Select(v => new SearchResultDto(
            "Vehicle", v.Id, v.RegistrationNumber, v.MakeModel, $"/fleet/{v.Id}")));

        var employees = await _db.Employees
            .Where(e => e.EmployeeNumber.Contains(q) || e.FirstName.Contains(q) || e.LastName.Contains(q))
            .Take(5)
            .ToListAsync(ct);
        results.AddRange(employees.Select(e => new SearchResultDto(
            "Employee", e.Id, $"{e.FirstName} {e.LastName}", e.EmployeeNumber, $"/employees/{e.Id}")));

        return results.Take(limit).ToList();
    }
}
