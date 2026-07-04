using Microsoft.EntityFrameworkCore;
using Topzinto.Erp.Application.DTOs.Employees;
using Topzinto.Erp.Application.Interfaces;
using Topzinto.Erp.Domain.Entities;
using Topzinto.Erp.Infrastructure.Common;
using Topzinto.Erp.Infrastructure.Persistence;

namespace Topzinto.Erp.Infrastructure.Services;

public class EmployeeService : IEmployeeService
{
    private readonly AppDbContext _db;
    private readonly IAuditService _audit;

    public EmployeeService(AppDbContext db, IAuditService audit)
    {
        _db = db;
        _audit = audit;
    }

    public async Task<IReadOnlyList<EmployeeDto>> GetAllAsync(string? search = null, string? status = null, string? department = null, CancellationToken ct = default)
    {
        var query = _db.Employees.Include(e => e.AssignedProject).AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim();
            query = query.Where(e =>
                e.EmployeeNumber.Contains(s) ||
                e.FirstName.Contains(s) ||
                e.LastName.Contains(s) ||
                e.JobTitle.Contains(s) ||
                (e.Phone != null && e.Phone.Contains(s)));
        }

        if (!string.IsNullOrWhiteSpace(status))
        {
            var st = HrDisplay.ParseStatus(status);
            query = query.Where(e => e.Status == st);
        }

        if (!string.IsNullOrWhiteSpace(department))
        {
            var dept = HrDisplay.ParseDepartment(department);
            query = query.Where(e => e.Department == dept);
        }

        var list = await query.OrderBy(e => e.LastName).ThenBy(e => e.FirstName).ToListAsync(ct);
        return list.Select(MapList).ToList();
    }

    public async Task<EmployeeDetailDto?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var employee = await _db.Employees
            .Include(e => e.AssignedProject)
            .FirstOrDefaultAsync(e => e.Id == id, ct);
        return employee is null ? null : MapDetail(employee);
    }

    public async Task<EmployeeDetailDto> CreateAsync(CreateEmployeeRequest request, Guid? userId, CancellationToken ct = default)
    {
        var employee = MapToEntity(new Employee(), request);
        employee.CreatedBy = userId;
        _db.Employees.Add(employee);
        await _db.SaveChangesAsync(ct);
        await _audit.LogAsync(userId, "", "Create", "HR", "Employee", employee.Id.ToString(), newValues: $"{employee.FirstName} {employee.LastName}", ct: ct);
        return (await GetByIdAsync(employee.Id, ct))!;
    }

    public async Task<EmployeeDetailDto?> UpdateAsync(Guid id, UpdateEmployeeRequest request, Guid? userId, CancellationToken ct = default)
    {
        var employee = await _db.Employees.FindAsync([id], ct);
        if (employee is null) return null;
        MapToEntity(employee, request);
        employee.UpdatedAt = DateTime.UtcNow;
        employee.UpdatedBy = userId;
        await _db.SaveChangesAsync(ct);
        await _audit.LogAsync(userId, "", "Update", "HR", "Employee", employee.Id.ToString(), newValues: $"{employee.FirstName} {employee.LastName}", ct: ct);
        return await GetByIdAsync(id, ct);
    }

    private static EmployeeDto MapList(Employee e) => new(
        e.Id,
        e.EmployeeNumber,
        $"{e.FirstName} {e.LastName}",
        e.JobTitle,
        HrDisplay.FormatDepartment(e.Department),
        HrDisplay.FormatTrade(e.Trade),
        HrDisplay.FormatStatus(e.Status),
        e.Phone,
        e.Email,
        e.AssignedProject?.Name,
        HrDisplay.FormatDate(e.HireDate)
    );

    private static EmployeeDetailDto MapDetail(Employee e) => new(
        e.Id,
        e.EmployeeNumber,
        e.FirstName,
        e.LastName,
        e.IdNumber,
        e.JobTitle,
        HrDisplay.FormatDepartment(e.Department),
        HrDisplay.FormatTrade(e.Trade),
        HrDisplay.FormatStatus(e.Status),
        e.Phone,
        e.Email,
        HrDisplay.FormatDate(e.HireDate),
        HrDisplay.FormatDate(e.TerminationDate),
        e.AssignedProjectId,
        e.AssignedProject?.Name,
        e.HourlyRate,
        e.Notes
    );

    private static Employee MapToEntity(Employee e, CreateEmployeeRequest r)
    {
        e.EmployeeNumber = r.EmployeeNumber.Trim();
        e.FirstName = r.FirstName.Trim();
        e.LastName = r.LastName.Trim();
        e.IdNumber = r.IdNumber?.Trim();
        e.JobTitle = r.JobTitle.Trim();
        e.Department = HrDisplay.ParseDepartment(r.Department);
        e.Trade = HrDisplay.ParseTrade(r.Trade);
        e.Status = HrDisplay.ParseStatus(r.Status);
        e.Phone = r.Phone?.Trim();
        e.Email = r.Email?.Trim();
        e.HireDate = HrDisplay.ParseDate(r.HireDate) ?? DateTime.UtcNow.Date;
        e.TerminationDate = HrDisplay.ParseDate(r.TerminationDate);
        e.AssignedProjectId = r.AssignedProjectId;
        e.HourlyRate = r.HourlyRate;
        e.Notes = r.Notes?.Trim();
        return e;
    }

    private static Employee MapToEntity(Employee e, UpdateEmployeeRequest r) =>
        MapToEntity(e, new CreateEmployeeRequest(
            r.EmployeeNumber, r.FirstName, r.LastName, r.IdNumber, r.JobTitle,
            r.Department, r.Trade, r.Status, r.Phone, r.Email, r.HireDate,
            r.TerminationDate, r.AssignedProjectId, r.HourlyRate, r.Notes));
}
