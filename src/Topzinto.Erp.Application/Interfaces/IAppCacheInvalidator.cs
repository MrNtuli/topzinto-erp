namespace Topzinto.Erp.Application.Interfaces;

public interface IAppCacheInvalidator
{
    Task InvalidateDashboardAndReportsAsync(CancellationToken ct = default);
}
