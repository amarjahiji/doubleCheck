using DoubleCheck.Abstractions;
using DoubleCheck.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DoubleCheck.Controllers;

[ApiController]
[Route("api/conversations")]
[Authorize]
public class ConversationsController : ControllerBase
{
    private readonly IChatService _chat;
    public ConversationsController(IChatService chat) => _chat = chat;

    [HttpPost]
    public async Task<ActionResult<ConversationResponse>> Create(CreateConversationRequest request, CancellationToken ct)
    {
        var result = await _chat.CreateConversationAsync(request, ct);
        return CreatedAtAction(nameof(Get), new { id = result.Id }, result);
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ConversationResponse>>> GetMine(CancellationToken ct)
        => Ok(await _chat.GetMyConversationsAsync(ct));

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ConversationResponse>> Get(Guid id, CancellationToken ct)
        => Ok(await _chat.GetConversationAsync(id, ct));

    [HttpPost("{id:guid}/messages")]
    public async Task<ActionResult<SendMessageResponse>> SendMessage(Guid id, SendMessageRequest request, CancellationToken ct)
    {
        var result = await _chat.SendMessageAsync(id, request, ct);
        return StatusCode(StatusCodes.Status201Created, result);
    }

    [HttpGet("{id:guid}/messages")]
    public async Task<ActionResult<IReadOnlyList<MessageResponse>>> GetMessages(Guid id, CancellationToken ct)
        => Ok(await _chat.GetMessagesAsync(id, ct));
}
