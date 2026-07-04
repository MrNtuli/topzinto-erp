namespace Topzinto.Erp.Application.DTOs.SiteReports;

public record SiteReportDto(
    Guid Id,
    Guid ProjectId,
    string ProjectName,
    string ReportDate,
    string? Weather,
    string Status,
    string? SubmittedByName,
    string WorkCompletedPreview
);

public record SiteReportPhotoDto(
    Guid Id,
    string FileName,
    string ContentType,
    long SizeBytes,
    string? Caption
);

public record SiteReportDetailDto(
    Guid Id,
    Guid ProjectId,
    string ProjectName,
    string ReportDate,
    string? Weather,
    string? Temperature,
    string? WindSpeed,
    int? PersonnelCount,
    string WorkCompleted,
    string? WorkPlanned,
    string? DelaysIssues,
    string? Notes,
    string Status,
    string? SubmittedByName,
    string? SubmittedAt,
    IReadOnlyList<SiteReportPhotoDto> Photos
);

public record CreateSiteReportRequest(
    Guid ProjectId,
    DateTime ReportDate,
    string? Weather,
    string? Temperature,
    string? WindSpeed,
    int? PersonnelCount,
    string WorkCompleted,
    string? WorkPlanned,
    string? DelaysIssues,
    string? Notes,
    bool Submit
);

public record UpdateSiteReportRequest(
    string? Weather,
    string? Temperature,
    string? WindSpeed,
    int? PersonnelCount,
    string WorkCompleted,
    string? WorkPlanned,
    string? DelaysIssues,
    string? Notes,
    bool Submit
);
