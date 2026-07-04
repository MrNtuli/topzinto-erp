using Topzinto.Erp.Application.DTOs.Clients;

namespace Topzinto.Erp.Application.Interfaces;

public interface IClientService
{
    Task<IReadOnlyList<ClientDto>> GetAllAsync(string? search = null, CancellationToken ct = default);
    Task<ClientDetailDto?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<ClientDetailDto> CreateAsync(CreateClientRequest request, Guid? userId, CancellationToken ct = default);
    Task<ClientDetailDto?> UpdateAsync(Guid id, UpdateClientRequest request, Guid? userId, CancellationToken ct = default);
    Task<bool> DeleteAsync(Guid id, Guid? userId, CancellationToken ct = default);
}
