using Topzinto.Erp.Domain.Common;
using Topzinto.Erp.Domain.Enums;

namespace Topzinto.Erp.Domain.Entities;

public class BoqItem : BaseEntity
{
    public Guid ProjectId { get; set; }
    public Project Project { get; set; } = null!;
    public string ItemCode { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = "General";
    public string Unit { get; set; } = "ea";
    public decimal Quantity { get; set; }
    public decimal Rate { get; set; }
    public decimal Amount { get; set; }
    public string? Notes { get; set; }
}

public class Claim : BaseEntity
{
    public Guid ProjectId { get; set; }
    public Project Project { get; set; } = null!;
    public string ClaimNumber { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public DateTime ClaimDate { get; set; } = DateTime.UtcNow.Date;
    public DateTime? PeriodFrom { get; set; }
    public DateTime? PeriodTo { get; set; }
    public decimal Amount { get; set; }
    public ClaimStatus Status { get; set; } = ClaimStatus.Draft;
    public string? SubmittedByName { get; set; }
    public string? Notes { get; set; }
}

public class Invoice : BaseEntity
{
    public Guid ProjectId { get; set; }
    public Project Project { get; set; } = null!;
    public string InvoiceNumber { get; set; } = string.Empty;
    public DateTime InvoiceDate { get; set; } = DateTime.UtcNow.Date;
    public DateTime? DueDate { get; set; }
    public decimal Amount { get; set; }
    public InvoiceStatus Status { get; set; } = InvoiceStatus.Draft;
    public string? Notes { get; set; }
}
