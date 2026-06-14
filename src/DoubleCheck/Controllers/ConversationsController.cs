using DoubleCheck.Abstractions;
using DoubleCheck.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DoubleCheck.Controllers;

/// <summary>Endpoints for user-owned conversations and AI-assisted messaging.</summary>
[ApiController]
[Route("api/conversations")]
[Authorize]
public class ConversationsController : ControllerBase
{
    private readonly IChatService _chat;

    /// <summary>Creates a conversations controller with the chat application service.</summary>
    public ConversationsController(IChatService chat) => _chat = chat;

    /// <summary>Create a conversation owned by the authenticated user.</summary>
    [HttpPost]
    public async Task<ActionResult<ConversationResponse>> Create(CreateConversationRequest request, CancellationToken ct)
    {
        var result = await _chat.CreateConversationAsync(request, ct);
        return CreatedAtAction(nameof(Get), new { id = result.Id }, result);
    }

    /// <summary>List conversations owned by the authenticated user.</summary>
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ConversationResponse>>> GetMine(CancellationToken ct)
        => Ok(await _chat.GetMyConversationsAsync(ct));

    /// <summary>Get one conversation owned by the authenticated user.</summary>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ConversationResponse>> Get(Guid id, CancellationToken ct)
        => Ok(await _chat.GetConversationAsync(id, ct));

    /// <summary>Add a user message and generated AI response to a conversation.</summary>
    [HttpPost("{id:guid}/messages")]
    public async Task<ActionResult<SendMessageResponse>> SendMessage(Guid id, SendMessageRequest request, CancellationToken ct)
    {
        var result = await _chat.SendMessageAsync(id, request, ct);
        return StatusCode(StatusCodes.Status201Created, result);
    }

    /// <summary>List messages for a conversation owned by the authenticated user.</summary>
    [HttpGet("{id:guid}/messages")]
    public async Task<ActionResult<IReadOnlyList<MessageResponse>>> GetMessages(Guid id, CancellationToken ct)
        => Ok(await _chat.GetMessagesAsync(id, ct));
}
