using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Topzinto.Erp.Application.DTOs.Attendance;
using Topzinto.Erp.Application.Interfaces;
using Topzinto.Erp.Api.Authorization;

namespace Topzinto.Erp.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = ErpModules.AttendancePolicy)]
public class AttendanceController : ControllerBase
{
    private readonly IAttendanceService _service;

    public AttendanceController(IAttendanceService service) => _service = service;

    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] Guid? projectId,
        [FromQuery] Guid? employeeId,
        [FromQuery] string? status,
        [FromQuery] DateTime? fromDate,
        [FromQuery] DateTime? toDate,
        CancellationToken ct) =>
        Ok(await _service.GetAllAsync(projectId, employeeId, status, fromDate, toDate, ct));

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var result = await _service.GetByIdAsync(id, ct);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateAttendanceRecordRequest request, CancellationToken ct)
    {
        try
        {
            var result = await _service.CreateAsync(request, GetUserId(), ct);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateAttendanceRecordRequest request, CancellationToken ct)
    {
        try
        {
            var result = await _service.UpdateAsync(id, request, GetUserId(), ct);
            return result is null ? NotFound() : Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
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
