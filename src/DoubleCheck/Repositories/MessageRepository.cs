using DoubleCheck.Data;
using DoubleCheck.Entities;
using Microsoft.EntityFrameworkCore;

namespace DoubleCheck.Repositories;

/// <summary>EF Core repository for message persistence.</summary>
public class MessageRepository : IMessageRepository
{
    private readonly AppDbContext _db;

    /// <summary>Creates a message repository backed by the application database.</summary>
    public MessageRepository(AppDbContext db) => _db = db;

    /// <inheritdoc />
    public async Task AddAsync(Message message, CancellationToken ct = default) =>
        await _db.Messages.AddAsync(message, ct);

    /// <inheritdoc />
    public async Task<IReadOnlyList<Message>> GetForConversationAsync(Guid conversationId, CancellationToken ct = default) =>
        await _db.Messages
            .Where(m => m.ConversationId == conversationId)
            .OrderBy(m => m.CreatedAt)
            .ToListAsync(ct);

    /// <inheritdoc />
    public Task SaveChangesAsync(CancellationToken ct = default) => _db.SaveChangesAsync(ct);
}
