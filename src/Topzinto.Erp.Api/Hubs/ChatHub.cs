using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Topzinto.Erp.Application.Interfaces;

namespace Topzinto.Erp.Api.Hubs;

[Authorize]
public class ChatHub : Hub
{
    private readonly IChatService _chat;

    public ChatHub(IChatService chat) => _chat = chat;

    public async Task JoinChannel(string channelId) =>
        await Groups.AddToGroupAsync(Context.ConnectionId, channelId);

    public async Task LeaveChannel(string channelId) =>
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, channelId);

    public async Task SendMessage(string channelId, string content)
    {
        if (!Guid.TryParse(channelId, out var channelGuid)) return;
        var userIdClaim = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userIdClaim is null || !Guid.TryParse(userIdClaim, out var userId)) return;

        var name = Context.User?.FindFirstValue(ClaimTypes.Name)
            ?? Context.User?.FindFirstValue(ClaimTypes.Email)
            ?? "User";

        var message = await _chat.SendMessageAsync(channelGuid, userId, name, content);
        if (message is not null)
            await Clients.Group(channelId).SendAsync("ReceiveMessage", message);
    }
}
