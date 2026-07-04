namespace Topzinto.Erp.Application.DTOs.Chat;

public record ChatChannelDto(
    Guid Id,
    string Name,
    string Slug,
    string Type,
    string? Description,
    string? ProjectName,
    int MessageCount,
    string? LastMessageAt,
    int UnreadCount
);

public record ChatUnreadSummaryDto(int TotalUnread);

public record ChatMessageDto(
    Guid Id,
    Guid ChannelId,
    Guid SenderUserId,
    string SenderName,
    string Content,
    string SentAt,
    string? AttachmentFileName,
    string? AttachmentContentType,
    long? AttachmentSizeBytes
);

public record SendChatMessageRequest(string Content);

public record ChatMentionUserDto(
    Guid Id,
    string DisplayName,
    string FirstName,
    string LastName,
    string Email
);
