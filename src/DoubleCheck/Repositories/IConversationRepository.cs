using DoubleCheck.Entities;

namespace DoubleCheck.Repositories;

public interface IConversationRepository
{
    Task<Conversation?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<Conversation>> GetForUserAsync(Guid userId, CancellationToken ct = default);
    Task<string?> GetCategoryNameAsync(Guid categoryId, CancellationToken ct = default);
    Task AddAsync(Conversation conversation, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
}
