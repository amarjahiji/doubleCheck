using DoubleCheck.Data;
using DoubleCheck.Entities;
using Microsoft.EntityFrameworkCore;

namespace DoubleCheck.Repositories;

public class MessageRepository : IMessageRepository
{
    private readonly AppDbContext _db;
    public MessageRepository(AppDbContext db) => _db = db;

    public async Task AddAsync(Message message, CancellationToken ct = default) =>
        await _db.Messages.AddAsync(message, ct);

    public async Task<IReadOnlyList<Message>> GetForConversationAsync(Guid conversationId, CancellationToken ct = default) =>
        await _db.Messages
            .Where(m => m.ConversationId == conversationId)
            .OrderBy(m => m.CreatedAt)
            .ToListAsync(ct);

    public Task SaveChangesAsync(CancellationToken ct = default) => _db.SaveChangesAsync(ct);
}
