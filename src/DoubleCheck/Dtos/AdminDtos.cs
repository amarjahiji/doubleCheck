using System.ComponentModel.DataAnnotations;

namespace DoubleCheck.Dtos;

/// <summary>Request to assign an ASP.NET Identity role to a user.</summary>
public class AssignRoleRequest
{
    /// <summary>Role name to assign.</summary>
    [Required]
    public string Role { get; set; } = string.Empty;
}

/// <summary>Admin-facing user row with roles and wallet balance.</summary>
public record AdminUserResponse(
    Guid Id,
    string Email,
    string DisplayName,
    decimal Balance,
    IReadOnlyList<string> Roles);

/// <summary>Admin-facing professional application row.</summary>
public record AdminProfessionalApplicationResponse(
    Guid Id,
    Guid UserId,
    decimal RequestedRate,
    string? Bio,
    string Status,
    Guid[] CategoryIds,
    DateTime CreatedAt,
    DateTime? DecidedAt);

/// <summary>Aggregate platform statistics for admin oversight.</summary>
public record AdminStatsResponse(
    int Users,
    int Conversations,
    int Messages,
    int OpenSessions,
    int ClosedSessions,
    double ResolutionRate);
