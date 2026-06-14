using DoubleCheck.Abstractions;
using DoubleCheck.Dtos;
using DoubleCheck.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DoubleCheck.Controllers;

[ApiController]
[Route("api/admin")]
[Authorize(Roles = Roles.Admin)]
public class AdminController : ControllerBase
{
    private readonly IAdminService _admin;
    public AdminController(IAdminService admin) => _admin = admin;

    /// <summary>Assign a role to a user (admin only — the core authz endpoint).</summary>
    [HttpGet("users")]
    public async Task<ActionResult<IReadOnlyList<AdminUserResponse>>> GetUsers(CancellationToken ct)
        => Ok(await _admin.GetUsersAsync(ct));

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

    /// <summary>Approve a professional application: assigns the Professional role and creates the profile.</summary>
    [HttpGet("professional-applications")]
    public async Task<ActionResult<IReadOnlyList<AdminProfessionalApplicationResponse>>> GetProfessionalApplications(
        [FromQuery] string? status,
        CancellationToken ct)
        => Ok(await _admin.GetProfessionalApplicationsAsync(status, ct));

    [HttpPost("professional-applications/{id:guid}/approve")]
    public async Task<ActionResult<ProfessionalProfileResponse>> Approve(Guid id, CancellationToken ct)
        => Ok(await _admin.ApproveApplicationAsync(id, ct));

    /// <summary>Reject a professional application.</summary>
    [HttpPost("professional-applications/{id:guid}/reject")]
    public async Task<ActionResult<ProfessionalApplicationResponse>> Reject(Guid id, CancellationToken ct)
        => Ok(await _admin.RejectApplicationAsync(id, ct));

    [HttpGet("stats")]
    public async Task<ActionResult<AdminStatsResponse>> GetStats(CancellationToken ct)
        => Ok(await _admin.GetStatsAsync(ct));
}
