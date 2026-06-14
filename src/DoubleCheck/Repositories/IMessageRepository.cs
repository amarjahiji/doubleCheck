using DoubleCheck.Entities;

namespace DoubleCheck.Repositories;

public interface IMessageRepository
{
    Task AddAsync(Message message, CancellationToken ct = default);
    Task<IReadOnlyList<Message>> GetForConversationAsync(Guid conversationId, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
}
