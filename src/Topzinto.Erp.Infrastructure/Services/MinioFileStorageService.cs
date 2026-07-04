using Microsoft.Extensions.Configuration;
using Minio;
using Minio.DataModel.Args;
using Topzinto.Erp.Application.Interfaces;

namespace Topzinto.Erp.Infrastructure.Services;

public class MinioFileStorageService : IFileStorageService
{
    private readonly IMinioClient _client;
    private readonly string _bucket;
    private bool _bucketReady;
    private readonly SemaphoreSlim _bucketLock = new(1, 1);

    public MinioFileStorageService(IConfiguration config)
    {
        var section = config.GetSection("FileStorage:Minio");
        _bucket = section["Bucket"] ?? "topzinto-documents";

        _client = new MinioClient()
            .WithEndpoint(section["Endpoint"] ?? "localhost:9000")
            .WithCredentials(section["AccessKey"] ?? "topzinto", section["SecretKey"] ?? "topzinto_dev")
            .WithSSL(section.GetValue("UseSsl", false))
            .Build();
    }

    public async Task<(string StoragePath, string FileName, string ContentType, long SizeBytes)> SaveAsync(
        Stream content, string fileName, string contentType, CancellationToken ct = default)
    {
        await EnsureBucketAsync(ct);

        var safeName = Path.GetFileName(fileName);
        var storedName = $"{Guid.NewGuid():N}_{safeName}";
        var objectName = $"{DateTime.UtcNow:yyyy}/{DateTime.UtcNow:MM}/{storedName}";

        await using var buffer = new MemoryStream();
        await content.CopyToAsync(buffer, ct);
        buffer.Position = 0;

        await _client.PutObjectAsync(new PutObjectArgs()
            .WithBucket(_bucket)
            .WithObject(objectName)
            .WithStreamData(buffer)
            .WithObjectSize(buffer.Length)
            .WithContentType(contentType), ct);

        return (objectName, safeName, contentType, buffer.Length);
    }

    public async Task<(Stream Stream, string ContentType, string FileName)?> OpenReadAsync(
        string storagePath, CancellationToken ct = default)
    {
        await EnsureBucketAsync(ct);

        try
        {
            var ms = new MemoryStream();
            await _client.GetObjectAsync(new GetObjectArgs()
                .WithBucket(_bucket)
                .WithObject(storagePath)
                .WithCallbackStream(stream => stream.CopyTo(ms)), ct);
            ms.Position = 0;
            return (ms, "application/octet-stream", Path.GetFileName(storagePath));
        }
        catch (Minio.Exceptions.MinioException)
        {
            return null;
        }
    }

    public void Delete(string storagePath)
    {
        _client.RemoveObjectAsync(new RemoveObjectArgs()
            .WithBucket(_bucket)
            .WithObject(storagePath)).GetAwaiter().GetResult();
    }

    private async Task EnsureBucketAsync(CancellationToken ct)
    {
        if (_bucketReady) return;

        await _bucketLock.WaitAsync(ct);
        try
        {
            if (_bucketReady) return;

            var exists = await _client.BucketExistsAsync(new BucketExistsArgs().WithBucket(_bucket), ct);
            if (!exists)
                await _client.MakeBucketAsync(new MakeBucketArgs().WithBucket(_bucket), ct);

            _bucketReady = true;
        }
        finally
        {
            _bucketLock.Release();
        }
    }
}
