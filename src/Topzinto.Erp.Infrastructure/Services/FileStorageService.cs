using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Topzinto.Erp.Application.Interfaces;

namespace Topzinto.Erp.Infrastructure.Services;

public class LocalFileStorageService : IFileStorageService
{
    private readonly string _root;

    public LocalFileStorageService(IConfiguration config, IHostEnvironment env)
    {
        var configured = config["FileStorage:LocalPath"] ?? config["Storage:LocalPath"];
        _root = string.IsNullOrWhiteSpace(configured)
            ? Path.Combine(env.ContentRootPath, "uploads")
            : Path.IsPathRooted(configured) ? configured : Path.Combine(env.ContentRootPath, configured);
        Directory.CreateDirectory(_root);
    }

    public async Task<(string StoragePath, string FileName, string ContentType, long SizeBytes)> SaveAsync(
        Stream content, string fileName, string contentType, CancellationToken ct = default)
    {
        var safeName = Path.GetFileName(fileName);
        var storedName = $"{Guid.NewGuid():N}_{safeName}";
        var relative = Path.Combine(DateTime.UtcNow.ToString("yyyy"), DateTime.UtcNow.ToString("MM"), storedName);
        var fullPath = Path.Combine(_root, relative);
        Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);

        await using var fs = File.Create(fullPath);
        await content.CopyToAsync(fs, ct);
        var size = fs.Length;

        return (relative.Replace('\\', '/'), safeName, contentType, size);
    }

    public Task<(Stream Stream, string ContentType, string FileName)?> OpenReadAsync(string storagePath, CancellationToken ct = default)
    {
        var fullPath = Path.Combine(_root, storagePath.Replace('/', Path.DirectorySeparatorChar));
        if (!File.Exists(fullPath))
            return Task.FromResult<(Stream, string, string)?>(null);

        Stream stream = File.OpenRead(fullPath);
        var name = Path.GetFileName(storagePath);
        return Task.FromResult<(Stream, string, string)?>((stream, "application/octet-stream", name));
    }

    public void Delete(string storagePath)
    {
        var fullPath = Path.Combine(_root, storagePath.Replace('/', Path.DirectorySeparatorChar));
        if (File.Exists(fullPath))
            File.Delete(fullPath);
    }
}
