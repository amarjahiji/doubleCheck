using DoubleCheck.Abstractions;
using DoubleCheck.Dtos;
using DoubleCheck.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DoubleCheck.Controllers;

/// <summary>Admin endpoints for role management, application decisions, and read-only oversight.</summary>
[ApiController]
[Route("api/admin")]
[Authorize(Roles = Roles.Admin)]
public class AdminController : ControllerBase
{
    private readonly IAdminService _admin;

    /// <summary>Creates an admin controller with the admin application service.</summary>
    public AdminController(IAdminService admin) => _admin = admin;

    /// <summary>List users with roles and balances for admin oversight.</summary>
    [HttpGet("users")]
    public async Task<ActionResult<IReadOnlyList<AdminUserResponse>>> GetUsers(CancellationToken ct)
        => Ok(await _admin.GetUsersAsync(ct));

    /// <summary>Assign a role to a user.</summary>
    [HttpPost("users/{userId:guid}/roles")]
    public async Task<IActionResult> AssignRole(Guid userId, AssignRoleRequest request, CancellationToken ct)
    {
        await _admin.AssignRoleAsync(userId, request.Role, ct);
        return NoContent();
    }

    /// <summary>Revoke a role from a user.</summary>
    [HttpDelete("users/{userId:guid}/roles/{role}")]
    public async Task<IActionResult> RevokeRole(Guid userId, string role, CancellationToken ct)
    {
        await _admin.RevokeRoleAsync(userId, role, ct);
        return NoContent();
    }

    /// <summary>List professional applications, optionally filtered by status.</summary>
    [HttpGet("professional-applications")]
    public async Task<ActionResult<IReadOnlyList<AdminProfessionalApplicationResponse>>> GetProfessionalApplications(
        [FromQuery] string? status,
        CancellationToken ct)
        => Ok(await _admin.GetProfessionalApplicationsAsync(status, ct));

    /// <summary>Approve a professional application and create the professional profile.</summary>
    [HttpPost("professional-applications/{id:guid}/approve")]
    public async Task<ActionResult<ProfessionalProfileResponse>> Approve(Guid id, CancellationToken ct)
        => Ok(await _admin.ApproveApplicationAsync(id, ct));

    /// <summary>Reject a professional application.</summary>
    [HttpPost("professional-applications/{id:guid}/reject")]
    public async Task<ActionResult<ProfessionalApplicationResponse>> Reject(Guid id, CancellationToken ct)
        => Ok(await _admin.RejectApplicationAsync(id, ct));

    /// <summary>Get system usage and verification-session summary statistics.</summary>
    [HttpGet("stats")]
    public async Task<ActionResult<AdminStatsResponse>> GetStats(CancellationToken ct)
        => Ok(await _admin.GetStatsAsync(ct));
}
