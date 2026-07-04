using Topzinto.Erp.Application.DTOs.Employees;

namespace Topzinto.Erp.Application.Interfaces;

public interface IEmployeeService
{
    Task<IReadOnlyList<EmployeeDto>> GetAllAsync(string? search = null, string? status = null, string? department = null, CancellationToken ct = default);
    Task<EmployeeDetailDto?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<EmployeeDetailDto> CreateAsync(CreateEmployeeRequest request, Guid? userId, CancellationToken ct = default);
    Task<EmployeeDetailDto?> UpdateAsync(Guid id, UpdateEmployeeRequest request, Guid? userId, CancellationToken ct = default);
}
