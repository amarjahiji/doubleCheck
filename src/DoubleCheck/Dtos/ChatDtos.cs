using System.ComponentModel.DataAnnotations;

namespace DoubleCheck.Dtos;

public class CreateConversationRequest
{
    [Required, MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [Required]
    public Guid CategoryId { get; set; }
}

public class SendMessageRequest
{
    [Required, MaxLength(8000)]
    public string Content { get; set; } = string.Empty;
}

public record ConversationResponse(
    Guid Id,
    string Title,
    Guid CategoryId,
    string CategoryName,
    DateTime CreatedAt);

public record MessageResponse(
    Guid Id,
    Guid ConversationId,
    string Sender,
    string Content,
    DateTime CreatedAt);

public record SendMessageResponse(MessageResponse UserMessage, MessageResponse AiMessage);
