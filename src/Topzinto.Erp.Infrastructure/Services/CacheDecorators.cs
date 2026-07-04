using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Topzinto.Erp.Application.DTOs.Dashboard;
using Topzinto.Erp.Application.Interfaces;
using Topzinto.Erp.Infrastructure.Caching;

namespace Topzinto.Erp.Infrastructure.Services;

public class CachedDashboardService : IDashboardService
{
    private readonly DashboardService _inner;
    private readonly IDistributedCache _cache;
    private readonly IConfiguration _config;

    public CachedDashboardService(DashboardService inner, IDistributedCache cache, IConfiguration config)
    {
        _inner = inner;
        _cache = cache;
        _config = config;
    }

    public async Task<DashboardDto> GetAsync(bool refreshCache = false, CancellationToken ct = default)
    {
        if (refreshCache)
            await _cache.RemoveAsync(AppCacheKeys.Dashboard, ct);

        return await GetOrSetAsync(AppCacheKeys.Dashboard, () => _inner.GetAsync(ct: ct), ct);
    }

    private async Task<T> GetOrSetAsync<T>(string key, Func<Task<T>> factory, CancellationToken ct)
    {
        var cached = await _cache.GetStringAsync(key, ct);
        if (cached is not null)
            return JsonSerializer.Deserialize<T>(cached)!;

        var value = await factory();
        var ttl = TimeSpan.FromMinutes(_config.GetValue("Redis:DefaultMinutes", 5));
        await _cache.SetStringAsync(
            key,
            JsonSerializer.Serialize(value),
            new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = ttl },
            ct);
        return value;
    }
}

public class CachedReportsService : IReportsService
{
    private readonly ReportsService _inner;
    private readonly IDistributedCache _cache;
    private readonly IConfiguration _config;

    public CachedReportsService(ReportsService inner, IDistributedCache cache, IConfiguration config)
    {
        _inner = inner;
        _cache = cache;
        _config = config;
    }

    public async Task<ReportsHubDto> GetHubAsync(CancellationToken ct = default)
    {
        var cached = await _cache.GetStringAsync(AppCacheKeys.ReportsHub, ct);
        if (cached is not null)
            return JsonSerializer.Deserialize<ReportsHubDto>(cached)!;

        var value = await _inner.GetHubAsync(ct);
        var ttl = TimeSpan.FromMinutes(_config.GetValue("Redis:DefaultMinutes", 5));
        await _cache.SetStringAsync(
            AppCacheKeys.ReportsHub,
            JsonSerializer.Serialize(value),
            new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = ttl },
            ct);
        return value;
    }
}
