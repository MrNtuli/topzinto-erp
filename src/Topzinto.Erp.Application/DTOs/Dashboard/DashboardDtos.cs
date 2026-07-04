namespace Topzinto.Erp.Application.DTOs.Dashboard;

public record DashboardDto(
    int ActiveProjects,
    int ActiveTenders,
    decimal TotalContractValue,
    decimal PendingClaimsValue,
    int FleetInUse,
    int FleetTotal,
    int EquipmentInUse,
    int EquipmentTotal,
    int ActiveUsers,
    int OverdueTasks,
    int DocumentsExpiringSoon,
    ProjectProgressDto ProjectProgress,
    IReadOnlyList<RecentSiteReportDto> LatestSiteReports,
    IReadOnlyList<FinancialTrendPointDto> FinancialTrend
);

public record ProjectProgressDto(int Completed, int InProgress, int OnHold, int Planned);

public record RecentSiteReportDto(Guid Id, string ProjectName, string ReportDate, string Status);

public record FinancialTrendPointDto(string Label, decimal ClaimsAmount, decimal ProcurementAmount);

public record ReportsHubDto(
    IReadOnlyList<ReportCardDto> Reports
);

public record ReportCardDto(
    string Id,
    string Title,
    string Description,
    string Value,
    string Link
);
