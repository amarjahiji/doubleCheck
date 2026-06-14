using DoubleCheck.Abstractions;
using DoubleCheck.Dtos;
using DoubleCheck.Entities;
using DoubleCheck.Exceptions;
using DoubleCheck.Repositories;
using DoubleCheck.Services;
using Microsoft.Extensions.Caching.Memory;
using NSubstitute;
using Xunit;

namespace DoubleCheck.Tests;

public class ChatServiceTests
{
    [Fact]
    public async Task SendMessageAsync_ForOwnedConversation_StoresUserAndAiMessages()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();
        var conversation = new Conversation { Id = Guid.NewGuid(), UserId = userId, CategoryId = categoryId, Title = "Help" };
        var conversations = Substitute.For<IConversationRepository>();
        conversations.GetByIdAsync(conversation.Id, Arg.Any<CancellationToken>()).Returns(conversation);
        conversations.GetCategoryNameAsync(categoryId, Arg.Any<CancellationToken>()).Returns("Tech");

        var messages = Substitute.For<IMessageRepository>();
        var ai = Substitute.For<IAiService>();
        ai.GenerateAnswerAsync("How do I fix this?", "Tech", Arg.Any<CancellationToken>()).Returns("AI answer");

        var sut = new ChatService(conversations, messages, ai, CurrentUser(userId), new MemoryCache(new MemoryCacheOptions()));

        // Act
        var result = await sut.SendMessageAsync(conversation.Id, new SendMessageRequest { Content = "How do I fix this?" });

        // Assert
        Assert.Equal("How do I fix this?", result.UserMessage.Content);
        Assert.Equal("AI answer", result.AiMessage.Content);
        await messages.Received(2).AddAsync(Arg.Any<Message>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SendMessageAsync_ForSomeoneElsesConversation_ThrowsForbiddenAndDoesNotCallAi()
    {
        // Arrange
        var callerId = Guid.NewGuid();
        var conversation = new Conversation { Id = Guid.NewGuid(), UserId = Guid.NewGuid(), CategoryId = Guid.NewGuid() };
        var conversations = Substitute.For<IConversationRepository>();
        conversations.GetByIdAsync(conversation.Id, Arg.Any<CancellationToken>()).Returns(conversation);
        var messages = Substitute.For<IMessageRepository>();
        var ai = Substitute.For<IAiService>();

        var sut = new ChatService(conversations, messages, ai, CurrentUser(callerId), new MemoryCache(new MemoryCacheOptions()));

        // Act + Assert
        await Assert.ThrowsAsync<ForbiddenException>(() =>
            sut.SendMessageAsync(conversation.Id, new SendMessageRequest { Content = "Question" }));
        await ai.DidNotReceive().GenerateAnswerAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
        await messages.DidNotReceive().AddAsync(Arg.Any<Message>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SendMessageAsync_WithSameQuestionAndCategory_UsesCachedAiAnswer()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();
        var conversation = new Conversation { Id = Guid.NewGuid(), UserId = userId, CategoryId = categoryId };
        var conversations = Substitute.For<IConversationRepository>();
        conversations.GetByIdAsync(conversation.Id, Arg.Any<CancellationToken>()).Returns(conversation);
        conversations.GetCategoryNameAsync(categoryId, Arg.Any<CancellationToken>()).Returns("Health");
        var ai = Substitute.For<IAiService>();
        ai.GenerateAnswerAsync("Same question", "Health", Arg.Any<CancellationToken>()).Returns("Cached answer");

        var sut = new ChatService(
            conversations,
            Substitute.For<IMessageRepository>(),
            ai,
            CurrentUser(userId),
            new MemoryCache(new MemoryCacheOptions()));

        // Act
        await sut.SendMessageAsync(conversation.Id, new SendMessageRequest { Content = "Same question" });
        await sut.SendMessageAsync(conversation.Id, new SendMessageRequest { Content = "Same question" });

        // Assert
        await ai.Received(1).GenerateAnswerAsync("Same question", "Health", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SendMessageAsync_WithDifferentCategory_MissesAiCache()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var techId = Guid.NewGuid();
        var foodId = Guid.NewGuid();
        var techConversation = new Conversation { Id = Guid.NewGuid(), UserId = userId, CategoryId = techId };
        var foodConversation = new Conversation { Id = Guid.NewGuid(), UserId = userId, CategoryId = foodId };
        var conversations = Substitute.For<IConversationRepository>();
        conversations.GetByIdAsync(techConversation.Id, Arg.Any<CancellationToken>()).Returns(techConversation);
        conversations.GetByIdAsync(foodConversation.Id, Arg.Any<CancellationToken>()).Returns(foodConversation);
        conversations.GetCategoryNameAsync(techId, Arg.Any<CancellationToken>()).Returns("Tech");
        conversations.GetCategoryNameAsync(foodId, Arg.Any<CancellationToken>()).Returns("Food");
        var ai = Substitute.For<IAiService>();
        ai.GenerateAnswerAsync("Same question", Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns("Answer");

        var sut = new ChatService(
            conversations,
            Substitute.For<IMessageRepository>(),
            ai,
            CurrentUser(userId),
            new MemoryCache(new MemoryCacheOptions()));

        // Act
        await sut.SendMessageAsync(techConversation.Id, new SendMessageRequest { Content = "Same question" });
        await sut.SendMessageAsync(foodConversation.Id, new SendMessageRequest { Content = "Same question" });

        // Assert
        await ai.Received(2).GenerateAnswerAsync("Same question", Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    private static ICurrentUser CurrentUser(Guid userId)
    {
        var current = Substitute.For<ICurrentUser>();
        current.IsAuthenticated.Returns(true);
        current.UserId.Returns(userId);
        return current;
    }
}
