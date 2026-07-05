using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Topzinto.Erp.Application.DTOs.Financial;
using Topzinto.Erp.Application.Interfaces;
using Topzinto.Erp.Api.Authorization;

namespace Topzinto.Erp.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = ErpModules.BoqPolicy)]
public class BoqController : ControllerBase
{
    private readonly IBoqService _boq;
    private readonly IFinancialService _financial;

    public BoqController(IBoqService boq, IFinancialService financial)
    {
        _boq = boq;
        _financial = financial;
    }

    [HttpGet("summary")]
    public async Task<IActionResult> GetSummary(CancellationToken ct) =>
        Ok(await _boq.GetSummaryAsync(ct));

    [HttpGet("financial-summary")]
    public async Task<IActionResult> GetFinancialSummary(CancellationToken ct) =>
        Ok(await _financial.GetSummaryAsync(ct));

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] Guid? projectId, [FromQuery] string? search, CancellationToken ct) =>
        Ok(await _boq.GetAllAsync(projectId, search, ct));

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var result = await _boq.GetByIdAsync(id, ct);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateBoqItemRequest request, CancellationToken ct)
    {
        var result = await _boq.CreateAsync(request, GetUserId(), ct);
        return CreatedAtAction(nameof(GetAll), new { projectId = result.ProjectId }, result);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateBoqItemRequest request, CancellationToken ct)
    {
        var result = await _boq.UpdateAsync(id, request, GetUserId(), ct);
        return result is null ? NotFound() : Ok(result);
    }

    private Guid? GetUserId() =>
        Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : null;
}

[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = ErpModules.BoqPolicy)]
public class ClaimsController : ControllerBase
{
    private readonly IClaimsService _service;

    public ClaimsController(IClaimsService service) => _service = service;

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
    public async Task<IActionResult> Create([FromBody] CreateClaimRequest request, CancellationToken ct)
    {
        var result = await _service.CreateAsync(request, GetUserId(), ct);
        return CreatedAtAction(nameof(GetAll), new { projectId = result.ProjectName }, result);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateClaimRequest request, CancellationToken ct)
    {
        var result = await _service.UpdateAsync(id, request, GetUserId(), ct);
        return result is null ? NotFound() : Ok(result);
    }

    private Guid? GetUserId() =>
        Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : null;
}

[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = ErpModules.BoqPolicy)]
public class InvoicesController : ControllerBase
{
    private readonly IInvoiceService _service;

    public InvoicesController(IInvoiceService service) => _service = service;

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] Guid? projectId, [FromQuery] string? status, CancellationToken ct) =>
        Ok(await _service.GetAllAsync(projectId, status, ct));
}

[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = ErpModules.DashboardPolicy)]
public class DashboardController : ControllerBase
{
    private readonly IDashboardService _service;

    public DashboardController(IDashboardService service) => _service = service;

    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] bool refresh = false, CancellationToken ct = default) =>
        Ok(await _service.GetAsync(refresh, ct));
}

[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = ErpModules.ReportsPolicy)]
public class ReportsController : ControllerBase
{
    private readonly IReportsService _service;

    public ReportsController(IReportsService service) => _service = service;

    [HttpGet("hub")]
    public async Task<IActionResult> GetHub(CancellationToken ct) =>
        Ok(await _service.GetHubAsync(ct));
}
