using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Topzinto.Erp.Application.DTOs.Chat;
using Topzinto.Erp.Application.Interfaces;
using Topzinto.Erp.Domain.Entities;
using Topzinto.Erp.Domain.Enums;
using Topzinto.Erp.Infrastructure.Persistence;

namespace Topzinto.Erp.Infrastructure.Services;

public class ChatService : IChatService
{
    private readonly AppDbContext _db;
    private readonly IFileStorageService _files;
    private readonly IEmailService _email;
    private readonly IConfiguration _config;

    public ChatService(AppDbContext db, IFileStorageService files, IEmailService email, IConfiguration config)
    {
        _db = db;
        _files = files;
        _email = email;
        _config = config;
    }

    public async Task<IReadOnlyList<ChatChannelDto>> GetChannelsAsync(Guid userId, CancellationToken ct = default)
    {
        var channels = await _db.ChatChannels
            .Include(c => c.Project)
            .Include(c => c.Messages)
            .Include(c => c.Members)
            .Where(c => c.Type != ChatChannelType.Direct || c.Members.Any(m => m.UserId == userId))
            .OrderBy(c => c.Type)
            .ThenBy(c => c.Name)
            .ToListAsync(ct);

        var readStates = await _db.ChatChannelReads
            .Where(r => r.UserId == userId)
            .ToDictionaryAsync(r => r.ChannelId, r => r.LastReadAt, ct);

        var otherUserIds = channels
            .Where(c => c.Type == ChatChannelType.Direct)
            .Select(c => c.Members.FirstOrDefault(m => m.UserId != userId)?.UserId)
            .Where(id => id.HasValue)
            .Select(id => id!.Value)
            .Distinct()
            .ToList();

        var displayNames = await _db.Users
            .Where(u => otherUserIds.Contains(u.Id))
            .ToDictionaryAsync(u => u.Id, u => $"{u.FirstName} {u.LastName}".Trim(), ct);

        return channels
            .Select(c => MapChannel(c, userId, readStates, displayNames))
            .ToList();
    }

    public async Task<ChatChannelDto?> GetOrCreateDirectChannelAsync(Guid userId, Guid otherUserId, CancellationToken ct = default)
    {
        if (userId == otherUserId) return null;

        var other = await _db.Users.FindAsync([otherUserId], ct);
        if (other is null || !other.IsActive) return null;

        var slug = BuildDirectSlug(userId, otherUserId);
        var existing = await _db.ChatChannels
            .Include(c => c.Project)
            .Include(c => c.Messages)
            .Include(c => c.Members)
            .FirstOrDefaultAsync(c => c.Slug == slug && c.Type == ChatChannelType.Direct, ct);

        if (existing is not null)
        {
            var readStates = await _db.ChatChannelReads
                .Where(r => r.UserId == userId)
                .ToDictionaryAsync(r => r.ChannelId, r => r.LastReadAt, ct);
            var names = new Dictionary<Guid, string> { [otherUserId] = $"{other.FirstName} {other.LastName}".Trim() };
            return MapChannel(existing, userId, readStates, names);
        }

        var channel = new ChatChannel
        {
            Name = $"{other.FirstName} {other.LastName}".Trim(),
            Slug = slug,
            Type = ChatChannelType.Direct,
            Description = "Direct message",
            CreatedBy = userId,
            Members =
            [
                new ChatChannelMember { UserId = userId },
                new ChatChannelMember { UserId = otherUserId },
            ],
        };

        _db.ChatChannels.Add(channel);
        await _db.SaveChangesAsync(ct);

        var reads = new Dictionary<Guid, DateTime>();
        var displayNames = new Dictionary<Guid, string> { [otherUserId] = channel.Name };
        return MapChannel(channel, userId, reads, displayNames);
    }

    public async Task<IReadOnlyList<ChatMessageDto>> GetMessagesAsync(Guid channelId, Guid userId, int take = 100, CancellationToken ct = default)
    {
        if (!await UserCanAccessChannelAsync(userId, channelId, ct))
            return [];

        var messages = await _db.ChatMessages
            .Where(m => m.ChannelId == channelId)
            .OrderByDescending(m => m.CreatedAt)
            .Take(take)
            .ToListAsync(ct);

        return messages
            .OrderBy(m => m.CreatedAt)
            .Select(MapMessage)
            .ToList();
    }

    public async Task<ChatMessageDto?> SendMessageAsync(Guid channelId, Guid userId, string senderName, string content, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(content)) return null;
        if (!await UserCanAccessChannelAsync(userId, channelId, ct)) return null;

        var message = new ChatMessage
        {
            ChannelId = channelId,
            SenderUserId = userId,
            SenderName = senderName.Trim(),
            Content = content.Trim(),
            CreatedBy = userId,
        };

        _db.ChatMessages.Add(message);
        await _db.SaveChangesAsync(ct);
        await NotifyMentionsAsync(message, ct);
        await MarkChannelReadAsync(userId, channelId, ct);
        return MapMessage(message);
    }

    public async Task<ChatMessageDto?> SendMessageWithAttachmentAsync(
        Guid channelId, Guid userId, string senderName, string? content,
        Stream fileStream, string fileName, string contentType, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(fileName)) return null;
        if (!await UserCanAccessChannelAsync(userId, channelId, ct)) return null;

        var saved = await _files.SaveAsync(fileStream, fileName, contentType, ct);
        var message = new ChatMessage
        {
            ChannelId = channelId,
            SenderUserId = userId,
            SenderName = senderName.Trim(),
            Content = content?.Trim() ?? string.Empty,
            AttachmentFileName = saved.FileName,
            AttachmentStoragePath = saved.StoragePath,
            AttachmentContentType = saved.ContentType,
            AttachmentSizeBytes = saved.SizeBytes,
            CreatedBy = userId,
        };

        _db.ChatMessages.Add(message);
        await _db.SaveChangesAsync(ct);
        await NotifyMentionsAsync(message, ct);
        await MarkChannelReadAsync(userId, channelId, ct);
        return MapMessage(message);
    }

    public async Task<(Stream Stream, string ContentType, string FileName)?> GetAttachmentAsync(Guid messageId, CancellationToken ct = default)
    {
        var message = await _db.ChatMessages.FindAsync([messageId], ct);
        if (message?.AttachmentStoragePath is null) return null;

        var file = await _files.OpenReadAsync(message.AttachmentStoragePath, ct);
        if (file is null) return null;

        return (file.Value.Stream, message.AttachmentContentType ?? file.Value.ContentType, message.AttachmentFileName ?? file.Value.FileName);
    }

    public async Task MarkChannelReadAsync(Guid userId, Guid channelId, CancellationToken ct = default)
    {
        if (!await UserCanAccessChannelAsync(userId, channelId, ct)) return;

        var now = DateTime.UtcNow;
        var read = await _db.ChatChannelReads
            .FirstOrDefaultAsync(r => r.UserId == userId && r.ChannelId == channelId, ct);

        if (read is null)
        {
            _db.ChatChannelReads.Add(new ChatChannelRead
            {
                UserId = userId,
                ChannelId = channelId,
                LastReadAt = now,
            });
        }
        else
        {
            read.LastReadAt = now;
        }

        await _db.SaveChangesAsync(ct);
    }

    public async Task<ChatUnreadSummaryDto> GetUnreadSummaryAsync(Guid userId, CancellationToken ct = default)
    {
        var channels = await GetChannelsAsync(userId, ct);
        return new ChatUnreadSummaryDto(channels.Sum(c => c.UnreadCount));
    }

    public async Task<IReadOnlyList<ChatMentionUserDto>> GetMentionableUsersAsync(CancellationToken ct = default)
    {
        return await _db.Users
            .Where(u => u.IsActive)
            .OrderBy(u => u.FirstName)
            .ThenBy(u => u.LastName)
            .Select(u => new ChatMentionUserDto(
                u.Id,
                u.FirstName + " " + u.LastName,
                u.FirstName,
                u.LastName,
                u.Email ?? string.Empty))
            .ToListAsync(ct);
    }

    private async Task<bool> UserCanAccessChannelAsync(Guid userId, Guid channelId, CancellationToken ct)
    {
        var channel = await _db.ChatChannels.AsNoTracking().FirstOrDefaultAsync(c => c.Id == channelId, ct);
        if (channel is null) return false;
        if (channel.Type != ChatChannelType.Direct) return true;
        return await _db.ChatChannelMembers.AnyAsync(m => m.ChannelId == channelId && m.UserId == userId, ct);
    }

    private async Task NotifyMentionsAsync(ChatMessage message, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(message.Content)) return;

        var channel = await _db.ChatChannels.FindAsync([message.ChannelId], ct);
        if (channel is null) return;

        var location = channel.Type == ChatChannelType.Direct ? channel.Name : $"#{channel.Slug}";

        var users = await _db.Users
            .Where(u => u.IsActive)
            .Select(u => new { u.Id, u.FirstName, u.LastName, u.Email })
            .ToListAsync(ct);

        var userTuples = users.Select(u => (u.Id, u.FirstName, u.LastName)).ToList();
        var emailsById = users.ToDictionary(u => u.Id, u => u.Email);
        var baseUrl = (_config["App:BaseUrl"] ?? "http://localhost:5173").TrimEnd('/');

        foreach (var (mentionedUserId, _) in ChatMentionHelper.FindMentionedUsers(message.Content, userTuples, message.SenderUserId))
        {
            var key = $"chat-mention:{message.Id}:{mentionedUserId}";
            if (await _db.Notifications.AnyAsync(n => n.ReferenceKey == key, ct)) continue;

            var preview = message.Content.Length > 120 ? message.Content[..117] + "..." : message.Content;
            _db.Notifications.Add(new Notification
            {
                UserId = mentionedUserId,
                Title = "You were mentioned in chat",
                Message = $"{message.SenderName} mentioned you in {location}: \"{preview}\"",
                Category = "Chat",
                Severity = "Info",
                ReferenceKey = key,
                LinkPath = "/messages",
            });

            if (emailsById.TryGetValue(mentionedUserId, out var email) && !string.IsNullOrWhiteSpace(email))
            {
                try
                {
                    await _email.SendAsync(
                        email,
                        "You were mentioned in TopZinto Team Chat",
                        $"""
                        <p><strong>{message.SenderName}</strong> mentioned you in <strong>{location}</strong>:</p>
                        <p style="font-style:italic;">"{System.Net.WebUtility.HtmlEncode(preview)}"</p>
                        <p><a href="{baseUrl}/messages">Open Team Chat</a></p>
                        """,
                        ct);
                }
                catch
                {
                    // In-app notification still created; email failure should not block chat.
                }
            }
        }

        await _db.SaveChangesAsync(ct);
    }

    private static ChatChannelDto MapChannel(
        ChatChannel c,
        Guid userId,
        IReadOnlyDictionary<Guid, DateTime> readStates,
        IReadOnlyDictionary<Guid, string> directDisplayNames)
    {
        var last = c.Messages.OrderByDescending(m => m.CreatedAt).FirstOrDefault();
        readStates.TryGetValue(c.Id, out var lastReadAt);
        var unread = c.Messages.Count(m => m.SenderUserId != userId && m.CreatedAt > lastReadAt);

        var name = c.Name;
        if (c.Type == ChatChannelType.Direct)
        {
            var otherId = c.Members.FirstOrDefault(m => m.UserId != userId)?.UserId;
            if (otherId.HasValue && directDisplayNames.TryGetValue(otherId.Value, out var displayName))
                name = displayName;
        }

        return new ChatChannelDto(
            c.Id,
            name,
            c.Slug,
            FormatChannelType(c.Type),
            c.Description,
            c.Project?.Name,
            c.Messages.Count,
            last is null ? null : last.CreatedAt.ToString("yyyy-MM-dd HH:mm"),
            unread
        );
    }

    private static string BuildDirectSlug(Guid userId, Guid otherUserId)
    {
        var (a, b) = userId.CompareTo(otherUserId) < 0 ? (userId, otherUserId) : (otherUserId, userId);
        return $"dm-{a:N}-{b:N}";
    }

    private static ChatMessageDto MapMessage(ChatMessage m) => new(
        m.Id,
        m.ChannelId,
        m.SenderUserId,
        m.SenderName,
        m.Content,
        m.CreatedAt.ToString("yyyy-MM-dd HH:mm"),
        m.AttachmentFileName,
        m.AttachmentContentType,
        m.AttachmentSizeBytes
    );

    private static string FormatChannelType(ChatChannelType t) => t switch
    {
        ChatChannelType.Department => "Department",
        ChatChannelType.Project => "Project",
        ChatChannelType.Direct => "Direct",
        _ => "General",
    };
}
