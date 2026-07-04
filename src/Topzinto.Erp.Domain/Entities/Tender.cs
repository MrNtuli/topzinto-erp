using Topzinto.Erp.Domain.Common;
using Topzinto.Erp.Domain.Enums;

namespace Topzinto.Erp.Domain.Entities;

public class Tender : BaseEntity
{
    public string ReferenceNumber { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public Guid ClientId { get; set; }
    public Client Client { get; set; } = null!;
    public DateTime ClosingDate { get; set; }
    public TenderStatus Status { get; set; } = TenderStatus.Identified;
    public decimal EstimatedValue { get; set; }
    public string? Notes { get; set; }
    public Guid? ContractId { get; set; }
}
