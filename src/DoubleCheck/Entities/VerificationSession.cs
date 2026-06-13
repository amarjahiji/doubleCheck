using DoubleCheck.Common;
using DoubleCheck.Enums;

namespace DoubleCheck.Entities;

/// <summary>A "Double Check": a user escalates an AI answer to a chosen professional.
/// Question + AI answer are snapshotted so verification does not depend on chat data.</summary>
public class VerificationSession : BaseEntity
{
    public Guid RequesterUserId { get; set; }
    public Guid ProfessionalUserId { get; set; }
    public Guid CategoryId { get; set; }
    public Guid? SourceMessageId { get; set; }
    public string QuestionSnapshot { get; set; } = string.Empty;
    public string AiAnswerSnapshot { get; set; } = string.Empty;
    public decimal AgreedRate { get; set; }
    public SessionStatus Status { get; set; } = SessionStatus.Open;
    public SessionOutcome Outcome { get; set; } = SessionOutcome.None;
    public string? ExpertSolution { get; set; }
    public DateTime? ClosedAt { get; set; }
}
