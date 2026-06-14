using DoubleCheck.Abstractions;
using DoubleCheck.Data;
using DoubleCheck.Dtos;
using DoubleCheck.Entities;
using DoubleCheck.Enums;
using DoubleCheck.Exceptions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace DoubleCheck.Services;

/// <summary>Admin-only operations for roles, applications, and read-only oversight.</summary>
public class AdminService : IAdminService
{
    private readonly AppDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;

    /// <summary>Creates an admin service backed by EF Core and ASP.NET Identity.</summary>
    public AdminService(AppDbContext db, UserManager<ApplicationUser> userManager)
    {
        _db = db;
        _userManager = userManager;
    }

    /// <inheritdoc />
    public async Task AssignRoleAsync(Guid userId, string role, CancellationToken ct = default)
    {
        EnsureValidRole(role);
        var user = await _userManager.FindByIdAsync(userId.ToString())
            ?? throw new NotFoundException("User not found.");

        if (await _userManager.IsInRoleAsync(user, role))
            return;

        var result = await _userManager.AddToRoleAsync(user, role);
        if (!result.Succeeded)
            throw new DomainException(string.Join("; ", result.Errors.Select(e => e.Description)));
    }

    /// <inheritdoc />
    public async Task RevokeRoleAsync(Guid userId, string role, CancellationToken ct = default)
    {
        EnsureValidRole(role);
        var user = await _userManager.FindByIdAsync(userId.ToString())
            ?? throw new NotFoundException("User not found.");

        var result = await _userManager.RemoveFromRoleAsync(user, role);
        if (!result.Succeeded)
            throw new DomainException(string.Join("; ", result.Errors.Select(e => e.Description)));
    }

    /// <inheritdoc />
    public async Task<ProfessionalProfileResponse> ApproveApplicationAsync(Guid applicationId, CancellationToken ct = default)
    {
        var application = await _db.ProfessionalApplications
            .Include(a => a.Categories)
            .FirstOrDefaultAsync(a => a.Id == applicationId, ct)
            ?? throw new NotFoundException("Application not found.");

        if (application.Status != ApplicationStatus.Pending)
            throw new ConflictException("Application is not pending.");

        var user = await _userManager.FindByIdAsync(application.UserId.ToString())
            ?? throw new NotFoundException("Applicant user not found.");

        var profile = new ProfessionalProfile
        {
            UserId = application.UserId,
            RatePerAnswer = application.RequestedRate,
            Bio = application.Bio,
            IsAvailable = true,
            Categories = application.Categories.Select(c => new ProfessionalCategory { CategoryId = c.CategoryId }).ToList()
        };
        _db.ProfessionalProfiles.Add(profile);

        application.Status = ApplicationStatus.Approved;
        application.DecidedAt = DateTime.UtcNow;

        var result = await _userManager.AddToRoleAsync(user, Roles.Professional);
        if (!result.Succeeded)
            throw new DomainException(string.Join("; ", result.Errors.Select(e => e.Description)));

        await _db.SaveChangesAsync(ct);

        return new ProfessionalProfileResponse(
            profile.UserId, user.DisplayName, profile.RatePerAnswer, profile.IsAvailable, profile.Bio,
            profile.Categories.Select(c => c.CategoryId).ToArray());
    }

    /// <inheritdoc />
    public async Task<ProfessionalApplicationResponse> RejectApplicationAsync(Guid applicationId, CancellationToken ct = default)
    {
        var application = await _db.ProfessionalApplications
            .Include(a => a.Categories)
            .FirstOrDefaultAsync(a => a.Id == applicationId, ct)
            ?? throw new NotFoundException("Application not found.");

        if (application.Status != ApplicationStatus.Pending)
            throw new ConflictException("Application is not pending.");

        application.Status = ApplicationStatus.Rejected;
        application.DecidedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);

        return new ProfessionalApplicationResponse(
            application.Id, application.RequestedRate, application.Bio, application.Status.ToString(),
            application.Categories.Select(c => c.CategoryId).ToArray(), application.CreatedAt, application.DecidedAt);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<AdminUserResponse>> GetUsersAsync(CancellationToken ct = default)
    {
        var users = await _db.Users
            .OrderBy(u => u.Email)
            .ToListAsync(ct);

        var userIds = users.Select(u => u.Id).ToArray();
        var rolesByUserId = await _db.UserRoles
            .Where(ur => userIds.Contains(ur.UserId))
            .Join(
                _db.Roles,
                userRole => userRole.RoleId,
                role => role.Id,
                (userRole, role) => new { userRole.UserId, RoleName = role.Name ?? string.Empty })
            .GroupBy(x => x.UserId)
            .ToDictionaryAsync(
                group => group.Key,
                group => (IReadOnlyList<string>)group
                    .Select(x => x.RoleName)
                    .Where(role => role.Length > 0)
                    .OrderBy(role => role)
                    .ToArray(),
                ct);

        var result = new List<AdminUserResponse>(users.Count);
        foreach (var user in users)
        {
            rolesByUserId.TryGetValue(user.Id, out var roles);
            result.Add(new AdminUserResponse(
                user.Id,
                user.Email ?? string.Empty,
                user.DisplayName,
                user.Balance,
                roles ?? Array.Empty<string>()));
        }

        return result;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<AdminProfessionalApplicationResponse>> GetProfessionalApplicationsAsync(string? status = null, CancellationToken ct = default)
    {
        IQueryable<ProfessionalApplication> query = _db.ProfessionalApplications
            .Include(a => a.Categories)
            .OrderByDescending(a => a.CreatedAt);

        if (!string.IsNullOrWhiteSpace(status))
        {
            if (!Enum.TryParse<ApplicationStatus>(status, ignoreCase: true, out var parsed) ||
                !Enum.IsDefined(parsed))
                throw new ValidationException("Unknown application status.");

            query = query.Where(a => a.Status == parsed);
        }

        return await query
            .Select(a => new AdminProfessionalApplicationResponse(
                a.Id,
                a.UserId,
                a.RequestedRate,
                a.Bio,
                a.Status.ToString(),
                a.Categories.Select(c => c.CategoryId).ToArray(),
                a.CreatedAt,
                a.DecidedAt))
            .ToListAsync(ct);
    }

    /// <inheritdoc />
    public async Task<AdminStatsResponse> GetStatsAsync(CancellationToken ct = default)
    {
        var users = await _db.Users.CountAsync(ct);
        var conversations = await _db.Conversations.CountAsync(ct);
        var messages = await _db.Messages.CountAsync(ct);
        var openSessions = await _db.VerificationSessions.CountAsync(s => s.Status == SessionStatus.Open, ct);
        var closedSessions = await _db.VerificationSessions.CountAsync(s => s.Status == SessionStatus.Closed, ct);
        var resolvedSessions = await _db.VerificationSessions
            .CountAsync(s => s.Status == SessionStatus.Closed && s.Outcome == SessionOutcome.Resolved, ct);
        var resolutionRate = closedSessions == 0 ? 0 : (double)resolvedSessions / closedSessions;

        return new AdminStatsResponse(users, conversations, messages, openSessions, closedSessions, resolutionRate);
    }

    private static void EnsureValidRole(string role)
    {
        if (!Roles.All.Contains(role))
            throw new ValidationException($"Unknown role '{role}'. Valid roles: {string.Join(", ", Roles.All)}.");
    }
}
