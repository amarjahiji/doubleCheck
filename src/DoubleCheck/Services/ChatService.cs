using System.Security.Cryptography;
using System.Text;
using DoubleCheck.Abstractions;
using DoubleCheck.Dtos;
using DoubleCheck.Entities;
using DoubleCheck.Enums;
using DoubleCheck.Exceptions;
using DoubleCheck.Repositories;
using Microsoft.Extensions.Caching.Memory;

namespace DoubleCheck.Services;

/// <summary>Coordinates user-owned conversations, message persistence, and cached AI replies.</summary>
public class ChatService : IChatService
{
    private static readonly TimeSpan AiCacheTtl = TimeSpan.FromHours(1);

    private readonly IConversationRepository _conversations;
    private readonly IMessageRepository _messages;
    private readonly IAiService _ai;
    private readonly ICurrentUser _currentUser;
    private readonly IMemoryCache _cache;

    /// <summary>Creates a chat service with repository, AI, caller, and cache dependencies.</summary>
    public ChatService(
        IConversationRepository conversations,
        IMessageRepository messages,
        IAiService ai,
        ICurrentUser currentUser,
        IMemoryCache cache)
    {
        _conversations = conversations;
        _messages = messages;
        _ai = ai;
        _currentUser = currentUser;
        _cache = cache;
    }

    /// <inheritdoc />
    public async Task<ConversationResponse> CreateConversationAsync(CreateConversationRequest request, CancellationToken ct = default)
    {
        EnsureAuthenticated();

        if (string.IsNullOrWhiteSpace(request.Title))
            throw new ValidationException("Title is required.");

        var categoryName = await _conversations.GetCategoryNameAsync(request.CategoryId, ct);
        if (categoryName is null)
            throw new ValidationException("Category does not exist.");

        var conversation = new Conversation
        {
            UserId = _currentUser.UserId,
            Title = request.Title.Trim(),
            CategoryId = request.CategoryId
        };

        await _conversations.AddAsync(conversation, ct);
        await _conversations.SaveChangesAsync(ct);

        return ToConversationResponse(conversation, categoryName);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<ConversationResponse>> GetMyConversationsAsync(CancellationToken ct = default)
    {
        EnsureAuthenticated();

        var conversations = await _conversations.GetForUserWithCategoryAsync(_currentUser.UserId, ct);
        return conversations
            .Select(x => ToConversationResponse(x.Conversation, x.CategoryName))
            .ToList();
    }

    /// <inheritdoc />
    public async Task<ConversationResponse> GetConversationAsync(Guid id, CancellationToken ct = default)
    {
        var conversation = await GetOwnedConversationWithCategoryAsync(id, ct);
        return ToConversationResponse(conversation.Conversation, conversation.CategoryName);
    }

    /// <inheritdoc />
    public async Task<SendMessageResponse> SendMessageAsync(Guid conversationId, SendMessageRequest request, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(request.Content))
            throw new ValidationException("Message content is required.");

        var conversation = await GetOwnedConversationWithCategoryAsync(conversationId, ct);

        var userMessage = new Message
        {
            ConversationId = conversation.Conversation.Id,
            Sender = MessageSender.User,
            Content = request.Content.Trim()
        };

        var aiAnswer = await GetCachedAiAnswerAsync(userMessage.Content, conversation.CategoryName, ct);
        var aiMessage = new Message
        {
            ConversationId = conversation.Conversation.Id,
            Sender = MessageSender.Ai,
            Content = aiAnswer
        };

        await _messages.AddAsync(userMessage, ct);
        await _messages.AddAsync(aiMessage, ct);
        await _messages.SaveChangesAsync(ct);

        return new SendMessageResponse(ToMessageResponse(userMessage), ToMessageResponse(aiMessage));
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<MessageResponse>> GetMessagesAsync(Guid conversationId, CancellationToken ct = default)
    {
        await GetOwnedConversationAsync(conversationId, ct);
        var messages = await _messages.GetForConversationAsync(conversationId, ct);
        return messages.Select(ToMessageResponse).ToList();
    }

    private async Task<Conversation> GetOwnedConversationAsync(Guid id, CancellationToken ct)
    {
        EnsureAuthenticated();

        var conversation = await _conversations.GetByIdAsync(id, ct)
            ?? throw new NotFoundException("Conversation not found.");

        if (conversation.UserId != _currentUser.UserId)
            throw new ForbiddenException("You do not own this conversation.");

        return conversation;
    }

    private async Task<ConversationWithCategory> GetOwnedConversationWithCategoryAsync(Guid id, CancellationToken ct)
    {
        EnsureAuthenticated();

        var conversation = await _conversations.GetWithCategoryAsync(id, ct)
            ?? throw new NotFoundException("Conversation not found.");

        if (conversation.Conversation.UserId != _currentUser.UserId)
            throw new ForbiddenException("You do not own this conversation.");

        return conversation;
    }

    private async Task<string> GetCachedAiAnswerAsync(string question, string categoryName, CancellationToken ct)
    {
        var key = $"ai:{categoryName}:{Sha256(question)}";
        if (_cache.TryGetValue(key, out string? cached) && cached is not null)
            return cached;

        var answer = await _ai.GenerateAnswerAsync(question, categoryName, ct);
        _cache.Set(key, answer, AiCacheTtl);
        return answer;
    }

    private void EnsureAuthenticated()
    {
        if (!_currentUser.IsAuthenticated || _currentUser.UserId == Guid.Empty)
            throw new UnauthorizedException("Authentication is required.");
    }

    private static string Sha256(string value)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(value.Trim()));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }

    private static ConversationResponse ToConversationResponse(Conversation conversation, string categoryName) =>
        new(conversation.Id, conversation.Title, conversation.CategoryId, categoryName, conversation.CreatedAt);

    private static MessageResponse ToMessageResponse(Message message) =>
        new(message.Id, message.ConversationId, message.Sender.ToString(), message.Content, message.CreatedAt);
}
