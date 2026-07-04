using Topzinto.Erp.Application.DTOs.Dashboard;
using Topzinto.Erp.Application.DTOs.Financial;

namespace Topzinto.Erp.Application.Interfaces;

public interface IBoqService
{
    Task<BoqSummaryDto> GetSummaryAsync(CancellationToken ct = default);
    Task<IReadOnlyList<BoqItemDto>> GetAllAsync(Guid? projectId = null, string? search = null, CancellationToken ct = default);
    Task<BoqItemDto> CreateAsync(CreateBoqItemRequest request, Guid? userId, CancellationToken ct = default);
}

public interface IClaimsService
{
    Task<IReadOnlyList<ClaimDto>> GetAllAsync(Guid? projectId = null, string? status = null, CancellationToken ct = default);
    Task<ClaimDto> CreateAsync(CreateClaimRequest request, Guid? userId, CancellationToken ct = default);
}

public interface IInvoiceService
{
    Task<IReadOnlyList<InvoiceDto>> GetAllAsync(Guid? projectId = null, string? status = null, CancellationToken ct = default);
}

public interface IFinancialService
{
    Task<FinancialSummaryDto> GetSummaryAsync(CancellationToken ct = default);
}

public interface IDashboardService
{
    Task<DashboardDto> GetAsync(bool refreshCache = false, CancellationToken ct = default);
}

public interface IReportsService
{
    Task<ReportsHubDto> GetHubAsync(CancellationToken ct = default);
}
