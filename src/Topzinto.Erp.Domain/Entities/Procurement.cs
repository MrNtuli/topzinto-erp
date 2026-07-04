using Topzinto.Erp.Domain.Common;
using Topzinto.Erp.Domain.Enums;

namespace Topzinto.Erp.Domain.Entities;

public class Supplier : BaseEntity
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public SupplierCategory Category { get; set; } = SupplierCategory.Materials;
    public SupplierStatus Status { get; set; } = SupplierStatus.Active;
    public string? ContactPerson { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? Province { get; set; }
    public string? VatNumber { get; set; }
    public string? Notes { get; set; }

    public ICollection<PurchaseOrder> PurchaseOrders { get; set; } = [];
}

public class PurchaseOrder : BaseEntity
{
    public string PoNumber { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public Guid SupplierId { get; set; }
    public Supplier Supplier { get; set; } = null!;
    public Guid? ProjectId { get; set; }
    public Project? Project { get; set; }
    public PoStatus Status { get; set; } = PoStatus.Draft;
    public DateTime OrderDate { get; set; } = DateTime.UtcNow.Date;
    public DateTime? RequiredDate { get; set; }
    public decimal TotalAmount { get; set; }
    public string? RequestedByName { get; set; }
    public string? ApprovedByName { get; set; }
    public string? Notes { get; set; }

    public ICollection<PurchaseOrderLine> Lines { get; set; } = [];
}

public class PurchaseOrderLine : BaseEntity
{
    public Guid PurchaseOrderId { get; set; }
    public PurchaseOrder PurchaseOrder { get; set; } = null!;
    public string Description { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public string Unit { get; set; } = "ea";
    public decimal UnitPrice { get; set; }
    public decimal LineTotal { get; set; }
}

public class InventoryItem : BaseEntity
{
    public string ItemCode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = "General";
    public string Unit { get; set; } = "ea";
    public decimal QuantityOnHand { get; set; }
    public decimal ReorderLevel { get; set; }
    public string? Location { get; set; }
    public decimal UnitCost { get; set; }
    public string? Notes { get; set; }

    public ICollection<InventoryTransaction> Transactions { get; set; } = [];
}

public class InventoryTransaction : BaseEntity
{
    public Guid InventoryItemId { get; set; }
    public InventoryItem InventoryItem { get; set; } = null!;
    public InventoryTransactionType TransactionType { get; set; }
    public decimal Quantity { get; set; }
    public DateTime TransactionDate { get; set; } = DateTime.UtcNow.Date;
    public string? Reference { get; set; }
    public Guid? ProjectId { get; set; }
    public Project? Project { get; set; }
    public string? Notes { get; set; }
    public string? RecordedByName { get; set; }
}
