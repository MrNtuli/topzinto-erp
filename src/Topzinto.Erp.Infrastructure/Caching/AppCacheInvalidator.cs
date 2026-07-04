using Microsoft.Extensions.Caching.Distributed;
using Topzinto.Erp.Application.Interfaces;

namespace Topzinto.Erp.Infrastructure.Caching;

public class AppCacheInvalidator : IAppCacheInvalidator
{
    private readonly IDistributedCache _cache;

    public AppCacheInvalidator(IDistributedCache cache) => _cache = cache;

    public async Task InvalidateDashboardAndReportsAsync(CancellationToken ct = default)
    {
        await _cache.RemoveAsync(AppCacheKeys.Dashboard, ct);
        await _cache.RemoveAsync(AppCacheKeys.ReportsHub, ct);
    }
}
