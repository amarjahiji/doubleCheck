using System.ComponentModel.DataAnnotations;

namespace DoubleCheck.Dtos;

public class AssignRoleRequest
{
    [Required]
    public string Role { get; set; } = string.Empty;
}

public record AdminUserResponse(
    Guid Id,
    string Email,
    string DisplayName,
    decimal Balance,
    IReadOnlyList<string> Roles);

public record AdminProfessionalApplicationResponse(
    Guid Id,
    Guid UserId,
    decimal RequestedRate,
    string? Bio,
    string Status,
    Guid[] CategoryIds,
    DateTime CreatedAt,
    DateTime? DecidedAt);

public record AdminStatsResponse(
    int Users,
    int Conversations,
    int Messages,
    int OpenSessions,
    int ClosedSessions,
    double ResolutionRate);
