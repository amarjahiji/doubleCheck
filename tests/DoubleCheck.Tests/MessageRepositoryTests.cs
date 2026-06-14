using DoubleCheck.Data;
using DoubleCheck.Entities;
using DoubleCheck.Enums;
using DoubleCheck.Repositories;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace DoubleCheck.Tests;

public class MessageRepositoryTests
{
    private static AppDbContext NewDb() =>
        new(new DbContextOptionsBuilder<AppDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);

    [Fact]
    public async Task GetForConversationAsync_ReturnsMessagesOrderedByCreatedAt()
    {
        // Arrange
        await using var db = NewDb();
        var conversationId = Guid.NewGuid();
        db.Messages.AddRange(
            new Message { ConversationId = conversationId, Sender = MessageSender.Ai, Content = "Second", CreatedAt = DateTime.UtcNow.AddMinutes(2) },
            new Message { ConversationId = conversationId, Sender = MessageSender.User, Content = "First", CreatedAt = DateTime.UtcNow.AddMinutes(1) });
        await db.SaveChangesAsync();
        var sut = new MessageRepository(db);

        // Act
        var messages = await sut.GetForConversationAsync(conversationId);

        // Assert
        Assert.Collection(
            messages,
            first => Assert.Equal("First", first.Content),
            second => Assert.Equal("Second", second.Content));
    }

    [Fact]
    public async Task GetForConversationAsync_ForUnknownConversation_ReturnsEmptyList()
    {
        // Arrange
        await using var db = NewDb();
        var sut = new MessageRepository(db);

        // Act
        var messages = await sut.GetForConversationAsync(Guid.NewGuid());

        // Assert
        Assert.Empty(messages);
    }
}
