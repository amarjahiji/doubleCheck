using DoubleCheck.Entities;

namespace DoubleCheck.Repositories;

/// <summary>Read/write access for conversations and their category labels.</summary>
public interface IConversationRepository
{
    /// <summary>Gets a conversation by identifier.</summary>
    Task<Conversation?> GetByIdAsync(Guid id, CancellationToken ct = default);

    /// <summary>Gets a user's conversations with category names in one query.</summary>
    Task<IReadOnlyList<ConversationWithCategory>> GetForUserWithCategoryAsync(Guid userId, CancellationToken ct = default);

    /// <summary>Gets one conversation with its category name.</summary>
    Task<ConversationWithCategory?> GetWithCategoryAsync(Guid id, CancellationToken ct = default);

    /// <summary>Gets conversations owned by a user.</summary>
    Task<IReadOnlyList<Conversation>> GetForUserAsync(Guid userId, CancellationToken ct = default);

    /// <summary>Gets a category name by identifier.</summary>
    Task<string?> GetCategoryNameAsync(Guid categoryId, CancellationToken ct = default);

    /// <summary>Adds a new conversation to the current unit of work.</summary>
    Task AddAsync(Conversation conversation, CancellationToken ct = default);

    /// <summary>Persists pending conversation changes.</summary>
    Task SaveChangesAsync(CancellationToken ct = default);
}

/// <summary>Conversation plus the display name of its category.</summary>
public record ConversationWithCategory(Conversation Conversation, string CategoryName);
