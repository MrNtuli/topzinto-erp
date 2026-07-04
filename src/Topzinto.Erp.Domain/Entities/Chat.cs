using Topzinto.Erp.Domain.Common;
using Topzinto.Erp.Domain.Enums;

namespace Topzinto.Erp.Domain.Entities;

public class ChatChannel : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public ChatChannelType Type { get; set; } = ChatChannelType.General;
    public EmployeeDepartment? Department { get; set; }
    public Guid? ProjectId { get; set; }
    public Project? Project { get; set; }
    public string? Description { get; set; }

    public ICollection<ChatMessage> Messages { get; set; } = [];
    public ICollection<ChatChannelMember> Members { get; set; } = [];
}

public class ChatMessage : BaseEntity
{
    public Guid ChannelId { get; set; }
    public ChatChannel Channel { get; set; } = null!;
    public Guid SenderUserId { get; set; }
    public string SenderName { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string? AttachmentFileName { get; set; }
    public string? AttachmentStoragePath { get; set; }
    public string? AttachmentContentType { get; set; }
    public long? AttachmentSizeBytes { get; set; }
}

public class ChatChannelRead
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }
    public Guid ChannelId { get; set; }
    public ChatChannel Channel { get; set; } = null!;
    public DateTime LastReadAt { get; set; } = DateTime.UtcNow;
}

public class ChatChannelMember
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ChannelId { get; set; }
    public ChatChannel Channel { get; set; } = null!;
    public Guid UserId { get; set; }
    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
}
