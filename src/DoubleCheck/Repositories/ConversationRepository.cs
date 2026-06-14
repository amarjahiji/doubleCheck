using DoubleCheck.Data;
using DoubleCheck.Entities;
using Microsoft.EntityFrameworkCore;

namespace DoubleCheck.Repositories;

public class ConversationRepository : IConversationRepository
{
    private readonly AppDbContext _db;
    public ConversationRepository(AppDbContext db) => _db = db;

    public Task<Conversation?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        _db.Conversations.FirstOrDefaultAsync(c => c.Id == id, ct);

    public async Task<IReadOnlyList<Conversation>> GetForUserAsync(Guid userId, CancellationToken ct = default) =>
        await _db.Conversations
            .Where(c => c.UserId == userId)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync(ct);

    public Task<string?> GetCategoryNameAsync(Guid categoryId, CancellationToken ct = default) =>
        _db.Categories
            .Where(c => c.Id == categoryId)
            .Select(c => c.Name)
            .FirstOrDefaultAsync(ct);

    public async Task AddAsync(Conversation conversation, CancellationToken ct = default) =>
        await _db.Conversations.AddAsync(conversation, ct);

    public Task SaveChangesAsync(CancellationToken ct = default) => _db.SaveChangesAsync(ct);
}
