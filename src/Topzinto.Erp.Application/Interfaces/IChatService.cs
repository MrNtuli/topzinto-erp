using Topzinto.Erp.Application.DTOs.Chat;

namespace Topzinto.Erp.Application.Interfaces;

public interface IChatService
{
    Task<IReadOnlyList<ChatChannelDto>> GetChannelsAsync(Guid userId, CancellationToken ct = default);
    Task<IReadOnlyList<ChatMessageDto>> GetMessagesAsync(Guid channelId, Guid userId, int take = 100, CancellationToken ct = default);
    Task<ChatMessageDto?> SendMessageAsync(Guid channelId, Guid userId, string senderName, string content, CancellationToken ct = default);
    Task<ChatMessageDto?> SendMessageWithAttachmentAsync(
        Guid channelId, Guid userId, string senderName, string? content,
        Stream fileStream, string fileName, string contentType, CancellationToken ct = default);
    Task<(Stream Stream, string ContentType, string FileName)?> GetAttachmentAsync(Guid messageId, CancellationToken ct = default);
    Task MarkChannelReadAsync(Guid userId, Guid channelId, CancellationToken ct = default);
    Task<ChatUnreadSummaryDto> GetUnreadSummaryAsync(Guid userId, CancellationToken ct = default);
    Task<IReadOnlyList<ChatMentionUserDto>> GetMentionableUsersAsync(CancellationToken ct = default);
    Task<ChatChannelDto?> GetOrCreateDirectChannelAsync(Guid userId, Guid otherUserId, CancellationToken ct = default);
}
