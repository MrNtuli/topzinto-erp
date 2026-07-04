using Topzinto.Erp.Application.DTOs.Search;

namespace Topzinto.Erp.Application.Interfaces;

public interface ISearchService
{
    Task<IReadOnlyList<SearchResultDto>> SearchAsync(string query, int limit = 20, CancellationToken ct = default);
}

public interface IFileStorageService
{
    Task<(string StoragePath, string FileName, string ContentType, long SizeBytes)> SaveAsync(
        Stream content, string fileName, string contentType, CancellationToken ct = default);
    Task<(Stream Stream, string ContentType, string FileName)?> OpenReadAsync(string storagePath, CancellationToken ct = default);
    void Delete(string storagePath);
}
