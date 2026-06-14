using System.ComponentModel.DataAnnotations;

namespace DoubleCheck.Dtos;

public class ApplyProfessionalRequest
{
    [Range(0, 1_000_000)]
    public decimal RequestedRate { get; set; }

    [MaxLength(2000)]
    public string? Bio { get; set; }

    [Required, MinLength(1)]
    public List<Guid> CategoryIds { get; set; } = new();
}

public class UpdateProfessionalRequest
{
    [Range(0, 1_000_000)]
    public decimal RatePerAnswer { get; set; }

    [MaxLength(2000)]
    public string? Bio { get; set; }

    [Required, MinLength(1)]
    public List<Guid> CategoryIds { get; set; } = new();
}

public class SetAvailabilityRequest
{
    public bool IsAvailable { get; set; }
}

public record ProfessionalApplicationResponse(
    Guid Id,
    decimal RequestedRate,
    string? Bio,
    string Status,
    Guid[] CategoryIds,
    DateTime CreatedAt,
    DateTime? DecidedAt);

public record ProfessionalProfileResponse(
    Guid UserId,
    string DisplayName,
    decimal RatePerAnswer,
    bool IsAvailable,
    string? Bio,
    Guid[] CategoryIds);
