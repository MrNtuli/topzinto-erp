using System.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Npgsql;
using Topzinto.Erp.Application.DTOs.Admin;
using Topzinto.Erp.Application.Interfaces;

namespace Topzinto.Erp.Infrastructure.Services;

public class BackupService : IBackupService
{
    private static readonly string[] BackupExtensions = [".db", ".sql", ".dump"];

    private readonly IConfiguration _config;
    private readonly IHostEnvironment _env;

    public BackupService(IConfiguration config, IHostEnvironment env)
    {
        _config = config;
        _env = env;
    }

    public async Task<string> CreateBackupAsync(CancellationToken ct = default)
    {
        var useSqlite = _config.GetValue("UseSqlite", false);
        var fileName = useSqlite
            ? await CreateSqliteBackupAsync(ct)
            : await CreatePostgresBackupAsync(ct);

        ApplyRetention();
        return fileName;
    }

    public BackupHubDto GetStatus()
    {
        var useSqlite = _config.GetValue("UseSqlite", false);
        var backupDir = GetBackupDirectory();
        var files = Directory.Exists(backupDir)
            ? Directory.GetFiles(backupDir)
                .Where(f => BackupExtensions.Contains(Path.GetExtension(f), StringComparer.OrdinalIgnoreCase))
                .Select(f => new FileInfo(f))
                .OrderByDescending(f => f.CreationTimeUtc)
                .Select(f => new BackupFileDto(
                    f.Name,
                    f.Length,
                    f.CreationTimeUtc.ToString("yyyy-MM-dd HH:mm")))
                .ToList()
            : [];

        return new BackupHubDto(
            useSqlite ? "SQLite" : "PostgreSQL",
            _config.GetValue("Backup:Enabled", false),
            _config.GetValue("Backup:IntervalHours", 24),
            _config.GetValue("Backup:RetentionCount", 7),
            files
        );
    }

    public (Stream Stream, string FileName, string ContentType)? OpenBackup(string fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName) || fileName != Path.GetFileName(fileName))
            return null;

        var ext = Path.GetExtension(fileName);
        if (!BackupExtensions.Contains(ext, StringComparer.OrdinalIgnoreCase))
            return null;

        var path = Path.Combine(GetBackupDirectory(), fileName);
        if (!File.Exists(path)) return null;

        var contentType = ext.Equals(".db", StringComparison.OrdinalIgnoreCase)
            ? "application/octet-stream"
            : "application/sql";

        return (File.OpenRead(path), fileName, contentType);
    }

    private async Task<string> CreateSqliteBackupAsync(CancellationToken ct)
    {
        var conn = _config.GetConnectionString("DefaultConnection") ?? "Data Source=topzinto_erp.db";
        var dbFile = conn.Replace("Data Source=", "", StringComparison.OrdinalIgnoreCase).Trim();
        var sourcePath = Path.IsPathRooted(dbFile) ? dbFile : Path.Combine(_env.ContentRootPath, dbFile);

        if (!File.Exists(sourcePath))
            throw new FileNotFoundException("Database file not found.", sourcePath);

        var fileName = $"topzinto_erp_{DateTime.UtcNow:yyyyMMdd_HHmmss}.db";
        var destPath = Path.Combine(GetBackupDirectory(), fileName);
        await Task.Run(() => File.Copy(sourcePath, destPath, overwrite: false), ct);
        return fileName;
    }

    private async Task<string> CreatePostgresBackupAsync(CancellationToken ct)
    {
        var connectionString = _config.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("DefaultConnection is not configured.");

        var builder = new NpgsqlConnectionStringBuilder(connectionString);
        var fileName = $"topzinto_erp_{DateTime.UtcNow:yyyyMMdd_HHmmss}.sql";
        var destPath = Path.Combine(GetBackupDirectory(), fileName);

        var args = $"-h {builder.Host} -p {builder.Port} -U {builder.Username} -d {builder.Database} -F p -f \"{destPath}\"";
        var psi = new ProcessStartInfo
        {
            FileName = "pg_dump",
            Arguments = args,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
        };
        psi.Environment["PGPASSWORD"] = builder.Password ?? string.Empty;

        using var process = Process.Start(psi)
            ?? throw new InvalidOperationException("Could not start pg_dump. Install PostgreSQL client tools.");

        var stderr = await process.StandardError.ReadToEndAsync(ct);
        await process.WaitForExitAsync(ct);

        if (process.ExitCode != 0)
            throw new InvalidOperationException(string.IsNullOrWhiteSpace(stderr)
                ? "pg_dump failed."
                : stderr.Trim());

        if (!File.Exists(destPath))
            throw new InvalidOperationException("Backup file was not created.");

        return fileName;
    }

    private void ApplyRetention()
    {
        var retention = _config.GetValue("Backup:RetentionCount", 7);
        if (retention <= 0) return;

        var backupDir = GetBackupDirectory();
        if (!Directory.Exists(backupDir)) return;

        var toDelete = Directory.GetFiles(backupDir)
            .Where(f => BackupExtensions.Contains(Path.GetExtension(f), StringComparer.OrdinalIgnoreCase))
            .Select(f => new FileInfo(f))
            .OrderByDescending(f => f.CreationTimeUtc)
            .Skip(retention);

        foreach (var file in toDelete)
            file.Delete();
    }

    private string GetBackupDirectory()
    {
        var configured = _config["Backup:Directory"];
        var dir = string.IsNullOrWhiteSpace(configured)
            ? Path.Combine(_env.ContentRootPath, "backups")
            : Path.IsPathRooted(configured) ? configured : Path.Combine(_env.ContentRootPath, configured);

        Directory.CreateDirectory(dir);
        return dir;
    }
}

public class ScheduledBackupService : BackgroundService
{
    private readonly IServiceProvider _services;
    private readonly IConfiguration _config;
    private readonly ILogger<ScheduledBackupService> _logger;

    public ScheduledBackupService(
        IServiceProvider services,
        IConfiguration config,
        ILogger<ScheduledBackupService> logger)
    {
        _services = services;
        _config = config;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var intervalHours = Math.Max(1, _config.GetValue("Backup:IntervalHours", 24));
            await Task.Delay(TimeSpan.FromHours(intervalHours), stoppingToken);

            if (!_config.GetValue("Backup:Enabled", false))
                continue;

            try
            {
                using var scope = _services.CreateScope();
                var backup = scope.ServiceProvider.GetRequiredService<IBackupService>();
                var fileName = await backup.CreateBackupAsync(stoppingToken);
                _logger.LogInformation("Scheduled database backup created: {FileName}", fileName);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Scheduled database backup failed.");
            }
        }
    }
}
