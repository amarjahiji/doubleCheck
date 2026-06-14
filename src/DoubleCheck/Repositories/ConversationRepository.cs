using DoubleCheck.Data;
using DoubleCheck.Entities;
using Microsoft.EntityFrameworkCore;

namespace DoubleCheck.Repositories;

/// <summary>EF Core repository for conversation persistence and category projections.</summary>
public class ConversationRepository : IConversationRepository
{
    private readonly AppDbContext _db;

    /// <summary>Creates a conversation repository backed by the application database.</summary>
    public ConversationRepository(AppDbContext db) => _db = db;

    /// <inheritdoc />
    public Task<Conversation?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        _db.Conversations.FirstOrDefaultAsync(c => c.Id == id, ct);

    /// <inheritdoc />
    public async Task<IReadOnlyList<ConversationWithCategory>> GetForUserWithCategoryAsync(Guid userId, CancellationToken ct = default) =>
        await _db.Conversations
            .Where(c => c.UserId == userId)
            .Join(
                _db.Categories,
                conversation => conversation.CategoryId,
                category => category.Id,
                (conversation, category) => new ConversationWithCategory(conversation, category.Name))
            .OrderByDescending(x => x.Conversation.CreatedAt)
            .ToListAsync(ct);

    /// <inheritdoc />
    public Task<ConversationWithCategory?> GetWithCategoryAsync(Guid id, CancellationToken ct = default) =>
        _db.Conversations
            .Where(c => c.Id == id)
            .Join(
                _db.Categories,
                conversation => conversation.CategoryId,
                category => category.Id,
                (conversation, category) => new ConversationWithCategory(conversation, category.Name))
            .FirstOrDefaultAsync(ct);

    /// <inheritdoc />
    public async Task<IReadOnlyList<Conversation>> GetForUserAsync(Guid userId, CancellationToken ct = default) =>
        await _db.Conversations
            .Where(c => c.UserId == userId)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync(ct);

    /// <inheritdoc />
    public Task<string?> GetCategoryNameAsync(Guid categoryId, CancellationToken ct = default) =>
        _db.Categories
            .Where(c => c.Id == categoryId)
            .Select(c => c.Name)
            .FirstOrDefaultAsync(ct);

    /// <inheritdoc />
    public async Task AddAsync(Conversation conversation, CancellationToken ct = default) =>
        await _db.Conversations.AddAsync(conversation, ct);

    /// <inheritdoc />
    public Task SaveChangesAsync(CancellationToken ct = default) => _db.SaveChangesAsync(ct);
}
