using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Topzinto.Erp.Application.DTOs.Equipment;
using Topzinto.Erp.Application.DTOs.Fleet;
using Topzinto.Erp.Application.Interfaces;
using Topzinto.Erp.Api.Authorization;

namespace Topzinto.Erp.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = ErpModules.FleetPolicy)]
public class FleetController : ControllerBase
{
    private readonly IFleetService _service;

    public FleetController(IFleetService service) => _service = service;

    [HttpGet("summary")]
    public async Task<IActionResult> GetSummary(CancellationToken ct) =>
        Ok(await _service.GetSummaryAsync(ct));

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] string? search, [FromQuery] string? status, CancellationToken ct) =>
        Ok(await _service.GetAllAsync(search, status, ct));

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var result = await _service.GetByIdAsync(id, ct);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateVehicleRequest request, CancellationToken ct)
    {
        var result = await _service.CreateAsync(request, GetUserId(), ct);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateVehicleRequest request, CancellationToken ct)
    {
        var result = await _service.UpdateAsync(id, request, GetUserId(), ct);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpPost("{id:guid}/fuel-logs")]
    public async Task<IActionResult> CreateFuelLog(Guid id, [FromBody] CreateFuelLogRequest request, CancellationToken ct)
    {
        var result = await _service.CreateFuelLogAsync(id, request, GetUserId(), ct);
        return result is null ? NotFound() : Ok(result);
    }

    private Guid? GetUserId() =>
        Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : null;
}

[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = ErpModules.EquipmentPolicy)]
public class EquipmentController : ControllerBase
{
    private readonly IEquipmentService _service;

    public EquipmentController(IEquipmentService service) => _service = service;

    [HttpGet("summary")]
    public async Task<IActionResult> GetSummary(CancellationToken ct) =>
        Ok(await _service.GetSummaryAsync(ct));

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] string? search, [FromQuery] string? status, CancellationToken ct) =>
        Ok(await _service.GetAllAsync(search, status, ct));

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var result = await _service.GetByIdAsync(id, ct);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateEquipmentRequest request, CancellationToken ct)
    {
        var result = await _service.CreateAsync(request, GetUserId(), ct);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateEquipmentRequest request, CancellationToken ct)
    {
        var result = await _service.UpdateAsync(id, request, GetUserId(), ct);
        return result is null ? NotFound() : Ok(result);
    }

    private Guid? GetUserId() =>
        Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : null;
}
