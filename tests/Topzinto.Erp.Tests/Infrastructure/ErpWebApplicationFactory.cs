using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;

namespace Topzinto.Erp.Tests.Infrastructure;

public class ErpWebApplicationFactory : WebApplicationFactory<Program>, IDisposable
{
    private readonly string _dbPath = Path.Combine(Path.GetTempPath(), $"topzinto_test_{Guid.NewGuid():N}.db");

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.UseSetting("UseSqlite", "true");
        builder.UseSetting("ConnectionStrings:DefaultConnection", $"Data Source={_dbPath}");
        builder.UseSetting("Backup:Enabled", "false");
        builder.UseSetting("Redis:Enabled", "false");
        builder.UseSetting("Email:Enabled", "false");
        builder.UseSetting("Email:SystemAlerts", "false");

        builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["UseSqlite"] = "true",
                ["ConnectionStrings:DefaultConnection"] = $"Data Source={_dbPath}",
                ["Backup:Enabled"] = "false",
                ["Redis:Enabled"] = "false",
                ["Email:Enabled"] = "false",
                ["Email:SystemAlerts"] = "false",
            });
        });
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            try
            {
                if (File.Exists(_dbPath))
                    File.Delete(_dbPath);
            }
            catch
            {
                // Best-effort cleanup for temp test database.
            }
        }

        base.Dispose(disposing);
    }
}
