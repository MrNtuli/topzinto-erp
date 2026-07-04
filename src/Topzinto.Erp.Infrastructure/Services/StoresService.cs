using Microsoft.EntityFrameworkCore;
using Topzinto.Erp.Application.DTOs.Stores;
using Topzinto.Erp.Application.Interfaces;
using Topzinto.Erp.Domain.Entities;
using Topzinto.Erp.Domain.Enums;
using Topzinto.Erp.Infrastructure.Common;
using Topzinto.Erp.Infrastructure.Persistence;

namespace Topzinto.Erp.Infrastructure.Services;

public class StoresService : IStoresService
{
    private readonly AppDbContext _db;
    private readonly IAuditService _audit;

    public StoresService(AppDbContext db, IAuditService audit)
    {
        _db = db;
        _audit = audit;
    }

    public async Task<StoresSummaryDto> GetSummaryAsync(CancellationToken ct = default)
    {
        var items = await _db.InventoryItems.ToListAsync(ct);
        var monthStart = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);
        var txCount = await _db.InventoryTransactions.CountAsync(t => t.TransactionDate >= monthStart, ct);
        return new StoresSummaryDto(
            items.Count,
            items.Count(i => i.QuantityOnHand <= i.ReorderLevel),
            items.Sum(i => i.QuantityOnHand * i.UnitCost),
            txCount
        );
    }

    public async Task<IReadOnlyList<InventoryItemDto>> GetAllItemsAsync(string? search = null, bool? lowStockOnly = null, CancellationToken ct = default)
    {
        var query = _db.InventoryItems.AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(i => i.Name.Contains(search) || i.ItemCode.Contains(search) || i.Category.Contains(search));

        var list = await query.OrderBy(i => i.ItemCode).ToListAsync(ct);

        if (lowStockOnly == true)
            list = list.Where(i => i.QuantityOnHand <= i.ReorderLevel).ToList();

        return list.Select(MapList).ToList();
    }

    public async Task<InventoryItemDetailDto?> GetItemByIdAsync(Guid id, CancellationToken ct = default)
    {
        var item = await _db.InventoryItems
            .Include(i => i.Transactions).ThenInclude(t => t.Project)
            .FirstOrDefaultAsync(i => i.Id == id, ct);
        return item is null ? null : MapDetail(item);
    }

    public async Task<InventoryItemDetailDto> CreateItemAsync(CreateInventoryItemRequest request, Guid? userId, CancellationToken ct = default)
    {
        var item = new InventoryItem
        {
            ItemCode = request.ItemCode,
            Name = request.Name,
            Category = request.Category,
            Unit = request.Unit,
            QuantityOnHand = request.QuantityOnHand,
            ReorderLevel = request.ReorderLevel,
            Location = request.Location,
            UnitCost = request.UnitCost,
            Notes = request.Notes,
            CreatedBy = userId,
        };
        _db.InventoryItems.Add(item);
        await _db.SaveChangesAsync(ct);
        await _audit.LogAsync(userId, "", "Create", "Stores", "InventoryItem", item.Id.ToString(), newValues: item.ItemCode, ct: ct);
        return (await GetItemByIdAsync(item.Id, ct))!;
    }

    public async Task<InventoryItemDetailDto?> UpdateItemAsync(Guid id, UpdateInventoryItemRequest request, Guid? userId, CancellationToken ct = default)
    {
        var item = await _db.InventoryItems.FindAsync([id], ct);
        if (item is null) return null;
        item.ItemCode = request.ItemCode;
        item.Name = request.Name;
        item.Category = request.Category;
        item.Unit = request.Unit;
        item.ReorderLevel = request.ReorderLevel;
        item.Location = request.Location;
        item.UnitCost = request.UnitCost;
        item.Notes = request.Notes;
        item.UpdatedAt = DateTime.UtcNow;
        item.UpdatedBy = userId;
        await _db.SaveChangesAsync(ct);
        await _audit.LogAsync(userId, "", "Update", "Stores", "InventoryItem", item.Id.ToString(), newValues: item.ItemCode, ct: ct);
        return await GetItemByIdAsync(id, ct);
    }

    public async Task<IReadOnlyList<InventoryTransactionDto>> GetTransactionsAsync(string? search = null, CancellationToken ct = default)
    {
        var query = _db.InventoryTransactions
            .Include(t => t.InventoryItem)
            .Include(t => t.Project)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(t =>
                t.Reference != null && t.Reference.Contains(search) ||
                t.InventoryItem.Name.Contains(search) ||
                t.InventoryItem.ItemCode.Contains(search));

        var list = await query.OrderByDescending(t => t.TransactionDate).ThenByDescending(t => t.CreatedAt).Take(200).ToListAsync(ct);
        return list.Select(MapTransaction).ToList();
    }

    public async Task<InventoryTransactionDto> CreateTransactionAsync(CreateInventoryTransactionRequest request, Guid? userId, CancellationToken ct = default)
    {
        var item = await _db.InventoryItems.FindAsync([request.InventoryItemId], ct)
            ?? throw new InvalidOperationException("Inventory item not found.");

        var type = ProcurementDisplay.ParseTransactionType(request.TransactionType);
        var qty = request.Quantity;

        item.QuantityOnHand = type switch
        {
            InventoryTransactionType.StockIn => item.QuantityOnHand + qty,
            InventoryTransactionType.StockOut => item.QuantityOnHand - qty,
            InventoryTransactionType.Adjustment => qty,
            _ => item.QuantityOnHand,
        };

        var tx = new InventoryTransaction
        {
            InventoryItemId = request.InventoryItemId,
            TransactionType = type,
            Quantity = qty,
            TransactionDate = request.TransactionDate,
            Reference = request.Reference,
            ProjectId = request.ProjectId,
            Notes = request.Notes,
            RecordedByName = request.RecordedByName,
            CreatedBy = userId,
        };

        _db.InventoryTransactions.Add(tx);
        await _db.SaveChangesAsync(ct);
        await _audit.LogAsync(userId, "", "Create", "Stores", "InventoryTransaction", tx.Id.ToString(), newValues: item.ItemCode, ct: ct);

        await _db.Entry(tx).Reference(t => t.InventoryItem).LoadAsync(ct);
        await _db.Entry(tx).Reference(t => t.Project).LoadAsync(ct);
        return MapTransaction(tx);
    }

    private static InventoryItemDto MapList(InventoryItem i) => new(
        i.Id, i.ItemCode, i.Name, i.Category, i.Unit,
        i.QuantityOnHand, i.ReorderLevel, i.Location, i.UnitCost,
        i.QuantityOnHand <= i.ReorderLevel
    );

    private static InventoryItemDetailDto MapDetail(InventoryItem i) => new(
        i.Id, i.ItemCode, i.Name, i.Category, i.Unit,
        i.QuantityOnHand, i.ReorderLevel, i.Location, i.UnitCost, i.Notes,
        i.Transactions.OrderByDescending(t => t.TransactionDate).Take(20)
            .Select(MapTransaction).ToList()
    );

    private static InventoryTransactionDto MapTransaction(InventoryTransaction t) => new(
        t.Id, t.InventoryItem.ItemCode, t.InventoryItem.Name,
        ProcurementDisplay.FormatTransactionType(t.TransactionType),
        t.Quantity,
        ProcurementDisplay.FormatDate(t.TransactionDate),
        t.Reference, t.Project?.Name, t.RecordedByName, t.Notes
    );
}
