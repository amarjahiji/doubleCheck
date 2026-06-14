using DoubleCheck.Entities;

namespace DoubleCheck.Repositories;

/// <summary>Read/write access for conversation messages.</summary>
public interface IMessageRepository
{
    /// <summary>Adds a message to the current unit of work.</summary>
    Task AddAsync(Message message, CancellationToken ct = default);

    /// <summary>Gets messages for a conversation ordered by creation time.</summary>
    Task<IReadOnlyList<Message>> GetForConversationAsync(Guid conversationId, CancellationToken ct = default);

    /// <summary>Persists pending message changes.</summary>
    Task SaveChangesAsync(CancellationToken ct = default);
}
