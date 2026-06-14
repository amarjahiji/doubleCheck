using System.ComponentModel.DataAnnotations;

namespace DoubleCheck.Dtos;

/// <summary>Request to create a user-owned conversation.</summary>
public class CreateConversationRequest
{
    /// <summary>Conversation title shown in the user's conversation list.</summary>
    [Required, MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    /// <summary>Category used to contextualize AI answers.</summary>
    [Required]
    public Guid CategoryId { get; set; }
}

/// <summary>Request to add a user message to a conversation.</summary>
public class SendMessageRequest
{
    /// <summary>User-authored prompt or follow-up.</summary>
    [Required, MaxLength(8000)]
    public string Content { get; set; } = string.Empty;
}

/// <summary>Conversation response returned to the owner.</summary>
public record ConversationResponse(
    Guid Id,
    string Title,
    Guid CategoryId,
    string CategoryName,
    DateTime CreatedAt);

/// <summary>Message response returned to the conversation owner.</summary>
public record MessageResponse(
    Guid Id,
    Guid ConversationId,
    string Sender,
    string Content,
    DateTime CreatedAt);

/// <summary>Response containing the stored user message and generated AI message.</summary>
public record SendMessageResponse(MessageResponse UserMessage, MessageResponse AiMessage);
