using Topzinto.Erp.Application.DTOs.Contracts;

namespace Topzinto.Erp.Application.Interfaces;

public interface IContractService
{
    Task<IReadOnlyList<ContractDto>> GetAllAsync(string? search = null, string? status = null, CancellationToken ct = default);
    Task<ContractDetailDto?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<ContractDetailDto> CreateAsync(CreateContractRequest request, Guid? userId, CancellationToken ct = default);
    Task<ContractDetailDto?> UpdateAsync(Guid id, UpdateContractRequest request, Guid? userId, CancellationToken ct = default);
    Task<bool> DeleteAsync(Guid id, Guid? userId, CancellationToken ct = default);
}
