using Topzinto.Erp.Application.DTOs.Tenders;

namespace Topzinto.Erp.Application.Interfaces;

public interface ITenderService
{
    Task<IReadOnlyList<TenderDto>> GetAllAsync(string? search = null, string? status = null, CancellationToken ct = default);
    Task<TenderDetailDto?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<TenderDetailDto> CreateAsync(CreateTenderRequest request, Guid? userId, CancellationToken ct = default);
    Task<TenderDetailDto?> UpdateAsync(Guid id, UpdateTenderRequest request, Guid? userId, CancellationToken ct = default);
    Task<bool> DeleteAsync(Guid id, Guid? userId, CancellationToken ct = default);
}
