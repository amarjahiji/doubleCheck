using DoubleCheck.Dtos;

namespace DoubleCheck.Abstractions;

public interface IChatService
{
    Task<ConversationResponse> CreateConversationAsync(CreateConversationRequest request, CancellationToken ct = default);
    Task<IReadOnlyList<ConversationResponse>> GetMyConversationsAsync(CancellationToken ct = default);
    Task<ConversationResponse> GetConversationAsync(Guid id, CancellationToken ct = default);
    Task<SendMessageResponse> SendMessageAsync(Guid conversationId, SendMessageRequest request, CancellationToken ct = default);
    Task<IReadOnlyList<MessageResponse>> GetMessagesAsync(Guid conversationId, CancellationToken ct = default);
}
