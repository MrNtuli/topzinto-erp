using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Topzinto.Erp.Application.DTOs.SiteReports;
using Topzinto.Erp.Application.Interfaces;
using Topzinto.Erp.Api.Authorization;

namespace Topzinto.Erp.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = ErpModules.SiteReportsPolicy)]
public class SiteReportsController : ControllerBase
{
    private readonly ISiteReportService _service;

    public SiteReportsController(ISiteReportService service) => _service = service;

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] Guid? projectId, CancellationToken ct) =>
        Ok(await _service.GetAllAsync(projectId, ct));

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var result = await _service.GetByIdAsync(id, ct);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateSiteReportRequest request, CancellationToken ct)
    {
        var result = await _service.CreateAsync(request, GetUserId(), GetUserName(), ct);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateSiteReportRequest request, CancellationToken ct)
    {
        var result = await _service.UpdateAsync(id, request, GetUserId(), GetUserName(), ct);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpGet("{id:guid}/pdf")]
    public async Task<IActionResult> DownloadPdf(Guid id, CancellationToken ct)
    {
        var pdf = await _service.ExportPdfAsync(id, ct);
        if (pdf is null) return NotFound();
        return File(pdf, "application/pdf", $"site_report_{id:N}.pdf");
    }

    [HttpPost("{id:guid}/photos")]
    [RequestSizeLimit(10_485_760)]
    public async Task<IActionResult> AddPhoto(
        Guid id,
        IFormFile file,
        [FromForm] string? caption,
        CancellationToken ct)
    {
        if (file.Length == 0) return BadRequest(new { message = "No file provided." });

        await using var stream = file.OpenReadStream();
        var result = await _service.AddPhotoAsync(id, stream, file.FileName, file.ContentType, caption, GetUserId(), ct);
        return result is null ? BadRequest(new { message = "Unable to upload photo. Max 5 images (JPEG, PNG, WebP)." }) : Ok(result);
    }

    [HttpGet("{id:guid}/photos/{photoId:guid}")]
    public async Task<IActionResult> GetPhoto(Guid id, Guid photoId, CancellationToken ct)
    {
        var file = await _service.GetPhotoAsync(id, photoId, ct);
        if (file is null) return NotFound();
        return File(file.Value.Stream, file.Value.ContentType, file.Value.FileName);
    }

    private Guid? GetUserId() =>
        Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : null;

    private string? GetUserName() => User.FindFirstValue(ClaimTypes.Name);
}
