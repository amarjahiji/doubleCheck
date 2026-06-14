using System.ComponentModel.DataAnnotations;
using DoubleCheck.Enums;

namespace DoubleCheck.Dtos;

/// <summary>Response DTO for a category shown on an expert match.</summary>
public sealed record ExpertCategoryResponse(Guid Id, string Name);

/// <summary>Response DTO for an available professional returned by expert matching.</summary>
public sealed record ExpertMatchResponse(
    Guid UserId,
    string DisplayName,
    decimal Rate,
    IReadOnlyList<ExpertCategoryResponse> Categories);

/// <summary>Request DTO for creating a verification session from a selected professional and AI answer.</summary>
public sealed record CreateVerificationSessionRequest
{
    /// <summary>The user id of the professional selected by the requester.</summary>
    [Required]
    public Guid ProfessionalUserId { get; init; }

    /// <summary>The category id that the selected professional must cover.</summary>
    [Required]
    public Guid CategoryId { get; init; }

    /// <summary>An optional source message id from the chat module.</summary>
    public Guid? SourceMessageId { get; init; }

    /// <summary>The question text snapshotted into the verification session.</summary>
    [Required]
    [MinLength(1)]
    public string QuestionText { get; init; } = string.Empty;

    /// <summary>The AI answer text snapshotted into the verification session.</summary>
    [Required]
    [MinLength(1)]
    public string AiAnswerText { get; init; } = string.Empty;
}

/// <summary>Request DTO for resolving a verification session with an expert solution.</summary>
public sealed record ResolveVerificationSessionRequest
{
    /// <summary>The professional's final solution for the verification session.</summary>
    [Required]
    [MinLength(1)]
    public string Solution { get; init; } = string.Empty;
}

/// <summary>Response DTO describing a verification session.</summary>
public sealed record VerificationSessionResponse(
    Guid Id,
    Guid RequesterUserId,
    Guid ProfessionalUserId,
    Guid CategoryId,
    Guid? SourceMessageId,
    string QuestionSnapshot,
    string AiAnswerSnapshot,
    decimal AgreedRate,
    SessionStatus Status,
    SessionOutcome Outcome,
    string? ExpertSolution,
    DateTime? ClosedAt,
    DateTime CreatedAt);
