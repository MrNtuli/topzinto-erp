using Topzinto.Erp.Application.DTOs.SiteReports;

namespace Topzinto.Erp.Application.Interfaces;

public interface ISiteReportService
{
    Task<IReadOnlyList<SiteReportDto>> GetAllAsync(Guid? projectId = null, CancellationToken ct = default);
    Task<SiteReportDetailDto?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<SiteReportDetailDto> CreateAsync(CreateSiteReportRequest request, Guid? userId, string? userName, CancellationToken ct = default);
    Task<SiteReportDetailDto?> UpdateAsync(Guid id, UpdateSiteReportRequest request, Guid? userId, string? userName, CancellationToken ct = default);
    Task<byte[]?> ExportPdfAsync(Guid id, CancellationToken ct = default);
    Task<SiteReportPhotoDto?> AddPhotoAsync(Guid reportId, Stream fileStream, string fileName, string contentType, string? caption, Guid? userId, CancellationToken ct = default);
    Task<(Stream Stream, string ContentType, string FileName)?> GetPhotoAsync(Guid reportId, Guid photoId, CancellationToken ct = default);
}
