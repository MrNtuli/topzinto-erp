using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Topzinto.Erp.Application.DTOs.Compliance;
using Topzinto.Erp.Application.Interfaces;
using Topzinto.Erp.Api.Authorization;

namespace Topzinto.Erp.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = ErpModules.CompliancePolicy)]
public class ComplianceController : ControllerBase
{
    private readonly IComplianceService _service;

    public ComplianceController(IComplianceService service) => _service = service;

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] Guid? projectId, [FromQuery] string? status, CancellationToken ct) =>
        Ok(await _service.GetAllAsync(projectId, status, ct));

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var result = await _service.GetByIdAsync(id, ct);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateComplianceRecordRequest request, CancellationToken ct)
    {
        var result = await _service.CreateAsync(request, GetUserId(), ct);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateComplianceRecordRequest request, CancellationToken ct)
    {
        var result = await _service.UpdateAsync(id, request, GetUserId(), ct);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var ok = await _service.DeleteAsync(id, GetUserId(), ct);
        return ok ? NoContent() : NotFound();
    }

    private Guid? GetUserId() =>
        Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : null;
}
