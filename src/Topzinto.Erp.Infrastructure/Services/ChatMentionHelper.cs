namespace Topzinto.Erp.Infrastructure.Services;

internal static class ChatMentionHelper
{
    public static IEnumerable<(Guid UserId, string DisplayName)> FindMentionedUsers(
        string content,
        IReadOnlyList<(Guid Id, string FirstName, string LastName)> users,
        Guid senderUserId)
    {
        if (string.IsNullOrWhiteSpace(content)) yield break;

        foreach (var user in users.OrderByDescending(u => $"{u.FirstName} {u.LastName}".Length))
        {
            if (user.Id == senderUserId) continue;

            var displayName = $"{user.FirstName} {user.LastName}".Trim();
            if (displayName.Length == 0) continue;

            var mention = $"@{displayName}";
            if (content.Contains(mention, StringComparison.OrdinalIgnoreCase))
                yield return (user.Id, displayName);
        }
    }
}
