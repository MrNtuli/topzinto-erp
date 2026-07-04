using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using Topzinto.Erp.Application.Interfaces;

namespace Topzinto.Erp.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HealthController : ControllerBase
{
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> Get(
        [FromServices] IConfiguration config,
        [FromServices] IDistributedCache cache,
        [FromServices] IEmailService email,
        CancellationToken ct)
    {
        var redisEnabled = config.GetValue("Redis:Enabled", false);
        var cacheStatus = redisEnabled ? "redis" : "memory";

        if (redisEnabled)
        {
            try
            {
                await cache.SetStringAsync("teerp:health:ping", "ok",
                    new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(10) }, ct);
                cacheStatus = "redis";
            }
            catch
            {
                cacheStatus = "redis-unavailable";
            }
        }

        return Ok(new
        {
            status = "healthy",
            service = "TopZinto ERP API",
            version = "2.28",
            cache = cacheStatus,
            email = email.IsEnabled ? "smtp" : "disabled",
        });
    }
}
