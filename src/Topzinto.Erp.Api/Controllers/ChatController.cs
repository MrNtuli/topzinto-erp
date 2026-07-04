using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Topzinto.Erp.Application.DTOs.Chat;
using Topzinto.Erp.Application.Interfaces;
using Topzinto.Erp.Api.Authorization;
using Topzinto.Erp.Api.Hubs;

namespace Topzinto.Erp.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = ErpModules.ChatPolicy)]
public class ChatController : ControllerBase
{
    private readonly IChatService _service;
    private readonly IHubContext<ChatHub> _hub;

    public ChatController(IChatService service, IHubContext<ChatHub> hub)
    {
        _service = service;
        _hub = hub;
    }

    [HttpGet("channels")]
    public async Task<IActionResult> GetChannels(CancellationToken ct)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();
        return Ok(await _service.GetChannelsAsync(userId.Value, ct));
    }

    [HttpGet("unread")]
    public async Task<IActionResult> GetUnreadSummary(CancellationToken ct)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();
        return Ok(await _service.GetUnreadSummaryAsync(userId.Value, ct));
    }

    [HttpGet("mentionable-users")]
    public async Task<IActionResult> GetMentionableUsers(CancellationToken ct) =>
        Ok(await _service.GetMentionableUsersAsync(ct));

    [HttpPost("direct/{otherUserId:guid}")]
    public async Task<IActionResult> GetOrCreateDirect(Guid otherUserId, CancellationToken ct)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();
        var result = await _service.GetOrCreateDirectChannelAsync(userId.Value, otherUserId, ct);
        return result is null ? BadRequest() : Ok(result);
    }

    [HttpGet("channels/{channelId:guid}/messages")]
    public async Task<IActionResult> GetMessages(Guid channelId, [FromQuery] int take = 100, CancellationToken ct = default)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();
        return Ok(await _service.GetMessagesAsync(channelId, userId.Value, take, ct));
    }

    [HttpPost("channels/{channelId:guid}/read")]
    public async Task<IActionResult> MarkRead(Guid channelId, CancellationToken ct)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();
        await _service.MarkChannelReadAsync(userId.Value, channelId, ct);
        return NoContent();
    }

    [HttpPost("channels/{channelId:guid}/messages")]
    public async Task<IActionResult> SendMessage(Guid channelId, [FromBody] SendChatMessageRequest request, CancellationToken ct)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();

        var name = User.FindFirstValue(ClaimTypes.Name) ?? User.FindFirstValue(ClaimTypes.Email) ?? "User";
        var result = await _service.SendMessageAsync(channelId, userId.Value, name, request.Content, ct);
        if (result is not null)
            await _hub.Clients.Group(channelId.ToString()).SendAsync("ReceiveMessage", result, ct);
        return result is null ? BadRequest() : Ok(result);
    }

    [HttpPost("channels/{channelId:guid}/messages/upload")]
    [RequestSizeLimit(10_485_760)]
    public async Task<IActionResult> SendMessageWithFile(
        Guid channelId,
        [FromForm] string? content,
        IFormFile file,
        CancellationToken ct)
    {
        if (file.Length == 0) return BadRequest(new { message = "No file provided." });

        var userId = GetUserId();
        if (userId is null) return Unauthorized();

        var name = User.FindFirstValue(ClaimTypes.Name) ?? User.FindFirstValue(ClaimTypes.Email) ?? "User";
        await using var stream = file.OpenReadStream();
        var result = await _service.SendMessageWithAttachmentAsync(
            channelId, userId.Value, name, content, stream, file.FileName, file.ContentType, ct);

        if (result is not null)
            await _hub.Clients.Group(channelId.ToString()).SendAsync("ReceiveMessage", result, ct);

        return result is null ? BadRequest() : Ok(result);
    }

    [HttpGet("messages/{messageId:guid}/attachment")]
    public async Task<IActionResult> DownloadAttachment(Guid messageId, CancellationToken ct)
    {
        var file = await _service.GetAttachmentAsync(messageId, ct);
        if (file is null) return NotFound();
        return File(file.Value.Stream, file.Value.ContentType, file.Value.FileName);
    }

    private Guid? GetUserId() =>
        Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : null;
}
