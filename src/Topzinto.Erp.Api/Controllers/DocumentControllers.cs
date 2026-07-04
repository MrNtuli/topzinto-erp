using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Topzinto.Erp.Application.DTOs.Documents;
using Topzinto.Erp.Application.Interfaces;
using Topzinto.Erp.Api.Authorization;

namespace Topzinto.Erp.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = ErpModules.DocumentsPolicy)]
public class DocumentsController : ControllerBase
{
    private readonly IDocumentService _service;

    public DocumentsController(IDocumentService service) => _service = service;

    [HttpGet("summary")]
    public async Task<IActionResult> GetSummary(CancellationToken ct) =>
        Ok(await _service.GetSummaryAsync(ct));

    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] string? search,
        [FromQuery] string? parentType,
        [FromQuery] Guid? parentId,
        [FromQuery] bool? expiringOnly,
        CancellationToken ct) =>
        Ok(await _service.GetAllAsync(search, parentType, parentId, expiringOnly, ct));

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateDocumentRequest request, CancellationToken ct)
    {
        var result = await _service.CreateAsync(request, GetUserId(), ct);
        return CreatedAtAction(nameof(GetAll), result);
    }

    [HttpPost("{id:guid}/upload")]
    [RequestSizeLimit(52_428_800)]
    public async Task<IActionResult> Upload(Guid id, IFormFile file, CancellationToken ct)
    {
        if (file.Length == 0) return BadRequest(new { message = "No file provided." });
        await using var stream = file.OpenReadStream();
        var result = await _service.AttachFileAsync(id, stream, file.FileName, file.ContentType, GetUserId(), ct);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpGet("{id:guid}/download")]
    public async Task<IActionResult> Download(Guid id, CancellationToken ct)
    {
        var file = await _service.GetFileAsync(id, ct);
        if (file is null) return NotFound();
        return File(file.Value.Stream, file.Value.ContentType, file.Value.FileName);
    }

    private Guid? GetUserId() =>
        Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : null;
}

[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = ErpModules.ReportsPolicy)]
public class ExportController : ControllerBase
{
    private readonly IExportService _service;

    public ExportController(IExportService service) => _service = service;

    [HttpGet("projects")]
    public async Task<IActionResult> Projects(CancellationToken ct) =>
        File(await _service.ExportProjectsCsvAsync(ct), "text/csv", $"projects_{DateTime.UtcNow:yyyyMMdd}.csv");

    [HttpGet("boq")]
    public async Task<IActionResult> Boq(CancellationToken ct) =>
        File(await _service.ExportBoqCsvAsync(ct), "text/csv", $"boq_{DateTime.UtcNow:yyyyMMdd}.csv");

    [HttpGet("claims")]
    public async Task<IActionResult> Claims(CancellationToken ct) =>
        File(await _service.ExportClaimsCsvAsync(ct), "text/csv", $"claims_{DateTime.UtcNow:yyyyMMdd}.csv");

    [HttpGet("suppliers")]
    public async Task<IActionResult> Suppliers(CancellationToken ct) =>
        File(await _service.ExportSuppliersCsvAsync(ct), "text/csv", $"suppliers_{DateTime.UtcNow:yyyyMMdd}.csv");

    [HttpGet("procurement")]
    public async Task<IActionResult> Procurement(CancellationToken ct) =>
        File(await _service.ExportProcurementCsvAsync(ct), "text/csv", $"procurement_{DateTime.UtcNow:yyyyMMdd}.csv");

    [HttpGet("invoices")]
    public async Task<IActionResult> Invoices(CancellationToken ct) =>
        File(await _service.ExportInvoicesCsvAsync(ct), "text/csv", $"invoices_{DateTime.UtcNow:yyyyMMdd}.csv");

    [HttpGet("fleet")]
    public async Task<IActionResult> Fleet(CancellationToken ct) =>
        File(await _service.ExportFleetCsvAsync(ct), "text/csv", $"fleet_{DateTime.UtcNow:yyyyMMdd}.csv");

    [HttpGet("documents")]
    public async Task<IActionResult> Documents(CancellationToken ct) =>
        File(await _service.ExportDocumentsCsvAsync(ct), "text/csv", $"documents_{DateTime.UtcNow:yyyyMMdd}.csv");

    [HttpGet("employees")]
    public async Task<IActionResult> Employees(CancellationToken ct) =>
        File(await _service.ExportEmployeesCsvAsync(ct), "text/csv", $"employees_{DateTime.UtcNow:yyyyMMdd}.csv");

    [HttpGet("timesheets")]
    public async Task<IActionResult> Timesheets(CancellationToken ct) =>
        File(await _service.ExportTimesheetsCsvAsync(ct), "text/csv", $"timesheets_{DateTime.UtcNow:yyyyMMdd}.csv");

    [HttpGet("projects/excel")]
    public async Task<IActionResult> ProjectsExcel(CancellationToken ct) =>
        File(
            await _service.ExportProjectsExcelAsync(ct),
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            $"projects_{DateTime.UtcNow:yyyyMMdd}.xlsx");

    [HttpGet("suppliers/excel")]
    public async Task<IActionResult> SuppliersExcel(CancellationToken ct) =>
        File(
            await _service.ExportSuppliersExcelAsync(ct),
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            $"suppliers_{DateTime.UtcNow:yyyyMMdd}.xlsx");

    [HttpGet("procurement/excel")]
    public async Task<IActionResult> ProcurementExcel(CancellationToken ct) =>
        File(
            await _service.ExportProcurementExcelAsync(ct),
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            $"procurement_{DateTime.UtcNow:yyyyMMdd}.xlsx");

    [HttpGet("employees/excel")]
    public async Task<IActionResult> EmployeesExcel(CancellationToken ct) =>
        File(
            await _service.ExportEmployeesExcelAsync(ct),
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            $"employees_{DateTime.UtcNow:yyyyMMdd}.xlsx");
}
