using Topzinto.Erp.Domain.Common;
using Topzinto.Erp.Domain.Enums;

namespace Topzinto.Erp.Domain.Entities;

public class DocumentRecord : BaseEntity
{
    public string Title { get; set; } = string.Empty;
    public string Category { get; set; } = "General";
    public DocumentParentType ParentType { get; set; } = DocumentParentType.Project;
    public Guid? ParentId { get; set; }
    public string? ParentName { get; set; }
    public string? FileName { get; set; }
    public string? StoragePath { get; set; }
    public string? ContentType { get; set; }
    public long? FileSizeBytes { get; set; }
    public int Version { get; set; } = 1;
    public DateTime? IssueDate { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public DocumentStatus Status { get; set; } = DocumentStatus.Approved;
    public string? Notes { get; set; }
}
