using System.ComponentModel.DataAnnotations;
using DoubleCheck.Enums;

namespace DoubleCheck.Dtos;

public sealed record ExpertCategoryResponse(Guid Id, string Name);

public sealed record ExpertMatchResponse(
    Guid UserId,
    string DisplayName,
    decimal Rate,
    IReadOnlyList<ExpertCategoryResponse> Categories);

public sealed record CreateVerificationSessionRequest
{
    [Required]
    public Guid ProfessionalUserId { get; init; }

    [Required]
    public Guid CategoryId { get; init; }

    public Guid? SourceMessageId { get; init; }

    [Required]
    [MinLength(1)]
    public string QuestionText { get; init; } = string.Empty;

    [Required]
    [MinLength(1)]
    public string AiAnswerText { get; init; } = string.Empty;
}

public sealed record ResolveVerificationSessionRequest
{
    [Required]
    [MinLength(1)]
    public string Solution { get; init; } = string.Empty;
}

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
