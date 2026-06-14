using DoubleCheck.Dtos;

namespace DoubleCheck.Abstractions;

/// <summary>Application service for conversations and AI-assisted messages.</summary>
public interface IChatService
{
    /// <summary>Creates a conversation owned by the current user.</summary>
    Task<ConversationResponse> CreateConversationAsync(CreateConversationRequest request, CancellationToken ct = default);

    /// <summary>Lists conversations owned by the current user.</summary>
    Task<IReadOnlyList<ConversationResponse>> GetMyConversationsAsync(CancellationToken ct = default);

    /// <summary>Gets one current-user-owned conversation.</summary>
    Task<ConversationResponse> GetConversationAsync(Guid id, CancellationToken ct = default);

    /// <summary>Stores a user message and the matching AI response.</summary>
    Task<SendMessageResponse> SendMessageAsync(Guid conversationId, SendMessageRequest request, CancellationToken ct = default);

    /// <summary>Lists messages for one current-user-owned conversation.</summary>
    Task<IReadOnlyList<MessageResponse>> GetMessagesAsync(Guid conversationId, CancellationToken ct = default);
}
