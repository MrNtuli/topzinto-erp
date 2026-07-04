using Microsoft.EntityFrameworkCore;
using Topzinto.Erp.Application.DTOs.SiteReports;
using Topzinto.Erp.Application.Interfaces;
using Topzinto.Erp.Domain.Entities;
using Topzinto.Erp.Domain.Enums;
using Topzinto.Erp.Infrastructure.Persistence;

namespace Topzinto.Erp.Infrastructure.Services;

public class SiteReportService : ISiteReportService
{
    private const int MaxPhotosPerReport = 5;
    private static readonly HashSet<string> AllowedImageTypes =
    [
        "image/jpeg", "image/jpg", "image/png", "image/webp", "image/gif"
    ];

    private readonly AppDbContext _db;
    private readonly IAuditService _audit;
    private readonly IFileStorageService _files;

    public SiteReportService(AppDbContext db, IAuditService audit, IFileStorageService files)
    {
        _db = db;
        _audit = audit;
        _files = files;
    }

    public async Task<IReadOnlyList<SiteReportDto>> GetAllAsync(Guid? projectId = null, CancellationToken ct = default)
    {
        var query = _db.SiteReports.Include(r => r.Project).AsQueryable();
        if (projectId.HasValue) query = query.Where(r => r.ProjectId == projectId);

        var reports = await query.OrderByDescending(r => r.ReportDate).ToListAsync(ct);
        return reports.Select(r => new SiteReportDto(
            r.Id,
            r.ProjectId,
            r.Project.Name,
            r.ReportDate.ToString("dd MMM yyyy"),
            r.Weather,
            FormatStatus(r.Status),
            r.SubmittedByName,
            r.WorkCompleted.Length > 80 ? r.WorkCompleted[..80] + "..." : r.WorkCompleted
        )).ToList();
    }

    public async Task<SiteReportDetailDto?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var r = await _db.SiteReports
            .Include(x => x.Project)
            .Include(x => x.Photos)
            .FirstOrDefaultAsync(x => x.Id == id, ct);
        return r is null ? null : MapDetail(r);
    }

    public async Task<SiteReportDetailDto> CreateAsync(CreateSiteReportRequest request, Guid? userId, string? userName, CancellationToken ct = default)
    {
        var report = new SiteReport
        {
            ProjectId = request.ProjectId,
            ReportDate = request.ReportDate.Date,
            Weather = request.Weather,
            Temperature = request.Temperature,
            WindSpeed = request.WindSpeed,
            PersonnelCount = request.PersonnelCount,
            WorkCompleted = request.WorkCompleted,
            WorkPlanned = request.WorkPlanned,
            DelaysIssues = request.DelaysIssues,
            Notes = request.Notes,
            CreatedBy = userId,
        };

        if (request.Submit)
            ApplySubmit(report, userName);

        _db.SiteReports.Add(report);
        await _db.SaveChangesAsync(ct);
        await _db.Entry(report).Reference(r => r.Project).LoadAsync(ct);
        await _audit.LogAsync(userId, userName ?? "", "Create", "SiteReports", "SiteReport", report.Id.ToString(), ct: ct);
        return MapDetail(report);
    }

    public async Task<SiteReportDetailDto?> UpdateAsync(Guid id, UpdateSiteReportRequest request, Guid? userId, string? userName, CancellationToken ct = default)
    {
        var report = await _db.SiteReports
            .Include(r => r.Project)
            .Include(r => r.Photos)
            .FirstOrDefaultAsync(r => r.Id == id, ct);
        if (report is null) return null;

        report.Weather = request.Weather;
        report.Temperature = request.Temperature;
        report.WindSpeed = request.WindSpeed;
        report.PersonnelCount = request.PersonnelCount;
        report.WorkCompleted = request.WorkCompleted;
        report.WorkPlanned = request.WorkPlanned;
        report.DelaysIssues = request.DelaysIssues;
        report.Notes = request.Notes;
        report.UpdatedAt = DateTime.UtcNow;
        report.UpdatedBy = userId;

        if (request.Submit && report.Status == SiteReportStatus.Draft)
            ApplySubmit(report, userName);

        await _db.SaveChangesAsync(ct);
        return MapDetail(report);
    }

    public async Task<byte[]?> ExportPdfAsync(Guid id, CancellationToken ct = default)
    {
        var detail = await GetByIdAsync(id, ct);
        return detail is null ? null : SiteReportPdfGenerator.Generate(detail);
    }

    public async Task<SiteReportPhotoDto?> AddPhotoAsync(
        Guid reportId,
        Stream fileStream,
        string fileName,
        string contentType,
        string? caption,
        Guid? userId,
        CancellationToken ct = default)
    {
        if (!AllowedImageTypes.Contains(contentType.ToLowerInvariant()))
            return null;

        var report = await _db.SiteReports.Include(r => r.Photos).FirstOrDefaultAsync(r => r.Id == reportId, ct);
        if (report is null) return null;
        if (report.Photos.Count >= MaxPhotosPerReport) return null;

        var saved = await _files.SaveAsync(fileStream, fileName, contentType, ct);
        var photo = new SiteReportPhoto
        {
            SiteReportId = reportId,
            FileName = saved.FileName,
            StoragePath = saved.StoragePath,
            ContentType = saved.ContentType,
            SizeBytes = saved.SizeBytes,
            Caption = caption?.Trim(),
            CreatedBy = userId,
        };

        _db.SiteReportPhotos.Add(photo);
        await _db.SaveChangesAsync(ct);
        await _audit.LogAsync(userId, "", "AddPhoto", "SiteReports", "SiteReport", reportId.ToString(), newValues: fileName, ct: ct);

        return MapPhoto(photo);
    }

    public async Task<(Stream Stream, string ContentType, string FileName)?> GetPhotoAsync(
        Guid reportId,
        Guid photoId,
        CancellationToken ct = default)
    {
        var photo = await _db.SiteReportPhotos
            .FirstOrDefaultAsync(p => p.Id == photoId && p.SiteReportId == reportId, ct);
        if (photo is null) return null;

        var file = await _files.OpenReadAsync(photo.StoragePath, ct);
        if (file is null) return null;

        return (file.Value.Stream, photo.ContentType, photo.FileName);
    }

    private static void ApplySubmit(SiteReport report, string? userName)
    {
        report.Status = SiteReportStatus.Submitted;
        report.SubmittedByName = userName;
        report.SubmittedAt = DateTime.UtcNow;
    }

    private static string FormatStatus(SiteReportStatus s) => s switch
    {
        SiteReportStatus.Draft => "Draft",
        SiteReportStatus.Submitted => "Submitted",
        SiteReportStatus.Approved => "Approved",
        _ => s.ToString(),
    };

    private static SiteReportPhotoDto MapPhoto(SiteReportPhoto p) =>
        new(p.Id, p.FileName, p.ContentType, p.SizeBytes, p.Caption);

    private static SiteReportDetailDto MapDetail(SiteReport r) => new(
        r.Id, r.ProjectId, r.Project.Name,
        r.ReportDate.ToString("dd MMM yyyy"),
        r.Weather, r.Temperature, r.WindSpeed, r.PersonnelCount,
        r.WorkCompleted, r.WorkPlanned, r.DelaysIssues, r.Notes,
        FormatStatus(r.Status), r.SubmittedByName,
        r.SubmittedAt?.ToString("dd MMM yyyy HH:mm"),
        r.Photos.OrderBy(p => p.CreatedAt).Select(MapPhoto).ToList()
    );
}
