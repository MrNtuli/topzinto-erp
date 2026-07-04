using Topzinto.Erp.Application.DTOs.Stores;

namespace Topzinto.Erp.Application.Interfaces;

public interface IStoresService
{
    Task<StoresSummaryDto> GetSummaryAsync(CancellationToken ct = default);
    Task<IReadOnlyList<InventoryItemDto>> GetAllItemsAsync(string? search = null, bool? lowStockOnly = null, CancellationToken ct = default);
    Task<InventoryItemDetailDto?> GetItemByIdAsync(Guid id, CancellationToken ct = default);
    Task<InventoryItemDetailDto> CreateItemAsync(CreateInventoryItemRequest request, Guid? userId, CancellationToken ct = default);
    Task<InventoryItemDetailDto?> UpdateItemAsync(Guid id, UpdateInventoryItemRequest request, Guid? userId, CancellationToken ct = default);
    Task<IReadOnlyList<InventoryTransactionDto>> GetTransactionsAsync(string? search = null, CancellationToken ct = default);
    Task<InventoryTransactionDto> CreateTransactionAsync(CreateInventoryTransactionRequest request, Guid? userId, CancellationToken ct = default);
}
