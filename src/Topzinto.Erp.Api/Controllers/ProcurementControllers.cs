using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Topzinto.Erp.Application.DTOs.Procurement;
using Topzinto.Erp.Application.DTOs.Stores;
using Topzinto.Erp.Application.DTOs.Suppliers;
using Topzinto.Erp.Application.Interfaces;
using Topzinto.Erp.Api.Authorization;

namespace Topzinto.Erp.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = ErpModules.SuppliersPolicy)]
public class SuppliersController : ControllerBase
{
    private readonly ISupplierService _service;

    public SuppliersController(ISupplierService service) => _service = service;

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
    public async Task<IActionResult> Create([FromBody] CreateSupplierRequest request, CancellationToken ct)
    {
        var result = await _service.CreateAsync(request, GetUserId(), ct);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateSupplierRequest request, CancellationToken ct)
    {
        var result = await _service.UpdateAsync(id, request, GetUserId(), ct);
        return result is null ? NotFound() : Ok(result);
    }

    private Guid? GetUserId() =>
        Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : null;
}

[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = ErpModules.ProcurementPolicy)]
public class ProcurementController : ControllerBase
{
    private readonly IProcurementService _service;

    public ProcurementController(IProcurementService service) => _service = service;

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
    public async Task<IActionResult> Create([FromBody] CreatePurchaseOrderRequest request, CancellationToken ct)
    {
        var result = await _service.CreateAsync(request, GetUserId(), ct);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdatePurchaseOrderRequest request, CancellationToken ct)
    {
        var result = await _service.UpdateAsync(id, request, GetUserId(), ct);
        return result is null ? NotFound() : Ok(result);
    }

    private Guid? GetUserId() =>
        Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : null;
}

[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = ErpModules.StoresPolicy)]
public class StoresController : ControllerBase
{
    private readonly IStoresService _service;

    public StoresController(IStoresService service) => _service = service;

    [HttpGet("summary")]
    public async Task<IActionResult> GetSummary(CancellationToken ct) =>
        Ok(await _service.GetSummaryAsync(ct));

    [HttpGet("items")]
    public async Task<IActionResult> GetItems([FromQuery] string? search, [FromQuery] bool? lowStockOnly, CancellationToken ct) =>
        Ok(await _service.GetAllItemsAsync(search, lowStockOnly, ct));

    [HttpGet("items/{id:guid}")]
    public async Task<IActionResult> GetItemById(Guid id, CancellationToken ct)
    {
        var result = await _service.GetItemByIdAsync(id, ct);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpPost("items")]
    public async Task<IActionResult> CreateItem([FromBody] CreateInventoryItemRequest request, CancellationToken ct)
    {
        var result = await _service.CreateItemAsync(request, GetUserId(), ct);
        return CreatedAtAction(nameof(GetItemById), new { id = result.Id }, result);
    }

    [HttpPut("items/{id:guid}")]
    public async Task<IActionResult> UpdateItem(Guid id, [FromBody] UpdateInventoryItemRequest request, CancellationToken ct)
    {
        var result = await _service.UpdateItemAsync(id, request, GetUserId(), ct);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpGet("transactions")]
    public async Task<IActionResult> GetTransactions([FromQuery] string? search, CancellationToken ct) =>
        Ok(await _service.GetTransactionsAsync(search, ct));

    [HttpPost("transactions")]
    public async Task<IActionResult> CreateTransaction([FromBody] CreateInventoryTransactionRequest request, CancellationToken ct)
    {
        var result = await _service.CreateTransactionAsync(request, GetUserId(), ct);
        return Ok(result);
    }

    private Guid? GetUserId() =>
        Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : null;
}
