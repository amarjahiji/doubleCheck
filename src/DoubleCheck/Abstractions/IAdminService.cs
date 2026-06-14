using DoubleCheck.Dtos;

namespace DoubleCheck.Abstractions;

/// <summary>Application service for admin role management, approvals, and oversight reads.</summary>
public interface IAdminService
{
    /// <summary>Assigns a role to a user.</summary>
    Task AssignRoleAsync(Guid userId, string role, CancellationToken ct = default);

    /// <summary>Revokes a role from a user.</summary>
    Task RevokeRoleAsync(Guid userId, string role, CancellationToken ct = default);

    /// <summary>Approves a pending professional application.</summary>
    Task<ProfessionalProfileResponse> ApproveApplicationAsync(Guid applicationId, CancellationToken ct = default);

    /// <summary>Rejects a pending professional application.</summary>
    Task<ProfessionalApplicationResponse> RejectApplicationAsync(Guid applicationId, CancellationToken ct = default);

    /// <summary>Lists users with their roles and balances.</summary>
    Task<IReadOnlyList<AdminUserResponse>> GetUsersAsync(CancellationToken ct = default);

    /// <summary>Lists professional applications, optionally filtered by status.</summary>
    Task<IReadOnlyList<AdminProfessionalApplicationResponse>> GetProfessionalApplicationsAsync(string? status = null, CancellationToken ct = default);

    /// <summary>Gets aggregate usage and verification statistics.</summary>
    Task<AdminStatsResponse> GetStatsAsync(CancellationToken ct = default);
}
