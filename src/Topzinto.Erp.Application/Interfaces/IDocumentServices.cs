using Topzinto.Erp.Application.DTOs.Documents;
using Topzinto.Erp.Application.DTOs.Admin;

namespace Topzinto.Erp.Application.Interfaces;

public interface IDocumentService
{
    Task<DocumentSummaryDto> GetSummaryAsync(CancellationToken ct = default);
    Task<IReadOnlyList<DocumentDto>> GetAllAsync(string? search = null, string? parentType = null, Guid? parentId = null, bool? expiringOnly = null, CancellationToken ct = default);
    Task<DocumentDto> CreateAsync(CreateDocumentRequest request, Guid? userId, CancellationToken ct = default);
    Task<DocumentDto?> AttachFileAsync(Guid id, Stream fileStream, string fileName, string contentType, Guid? userId, CancellationToken ct = default);
    Task<(Stream Stream, string ContentType, string FileName)?> GetFileAsync(Guid id, CancellationToken ct = default);
}

public interface IExportService
{
    Task<byte[]> ExportProjectsCsvAsync(CancellationToken ct = default);
    Task<byte[]> ExportBoqCsvAsync(CancellationToken ct = default);
    Task<byte[]> ExportClaimsCsvAsync(CancellationToken ct = default);
    Task<byte[]> ExportSuppliersCsvAsync(CancellationToken ct = default);
    Task<byte[]> ExportProcurementCsvAsync(CancellationToken ct = default);
    Task<byte[]> ExportInvoicesCsvAsync(CancellationToken ct = default);
    Task<byte[]> ExportFleetCsvAsync(CancellationToken ct = default);
    Task<byte[]> ExportDocumentsCsvAsync(CancellationToken ct = default);
    Task<byte[]> ExportEmployeesCsvAsync(CancellationToken ct = default);
    Task<byte[]> ExportTimesheetsCsvAsync(CancellationToken ct = default);
    Task<byte[]> ExportProjectsExcelAsync(CancellationToken ct = default);
    Task<byte[]> ExportSuppliersExcelAsync(CancellationToken ct = default);
    Task<byte[]> ExportProcurementExcelAsync(CancellationToken ct = default);
    Task<byte[]> ExportEmployeesExcelAsync(CancellationToken ct = default);
}

public interface IBackupService
{
    Task<string> CreateBackupAsync(CancellationToken ct = default);
    BackupHubDto GetStatus();
    (Stream Stream, string FileName, string ContentType)? OpenBackup(string fileName);
}
