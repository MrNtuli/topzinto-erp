using Topzinto.Erp.Domain.Common;

namespace Topzinto.Erp.Domain.Entities;

public class Notification : BaseEntity
{
    public Guid? UserId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string Category { get; set; } = "System";
    public string Severity { get; set; } = "Info";
    public string? ReferenceKey { get; set; }
    public string? LinkPath { get; set; }
    public bool IsRead { get; set; }
    public DateTime? ReadAt { get; set; }
}

public class CompanySetting
{
    public Guid Id { get; set; } = Guid.Parse("00000000-0000-0000-0000-000000000001");
    public string CompanyName { get; set; } = "TopZinto CC";
    public string Tagline { get; set; } = "Smart. Proficient. Value.";
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? Province { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? VatNumber { get; set; }
    public string? CidbNumber { get; set; }
    public bool SmtpEnabled { get; set; }
    public string? SmtpHost { get; set; }
    public int? SmtpPort { get; set; }
    public bool SmtpUseSsl { get; set; } = true;
    public string? SmtpUsername { get; set; }
    public string? SmtpPassword { get; set; }
    public string? EmailFromAddress { get; set; }
    public string? EmailFromName { get; set; }
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
