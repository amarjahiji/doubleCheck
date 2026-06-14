using DoubleCheck.Abstractions;
using DoubleCheck.Data;
using DoubleCheck.Dtos;
using DoubleCheck.Entities;
using DoubleCheck.Enums;
using DoubleCheck.Exceptions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace DoubleCheck.Services;

/// <summary>Admin-only operations: role assignment and professional-application approval.</summary>
public class AdminService : IAdminService
{
    private readonly AppDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;

    public AdminService(AppDbContext db, UserManager<ApplicationUser> userManager)
    {
        _db = db;
        _userManager = userManager;
    }

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

    public async Task RevokeRoleAsync(Guid userId, string role, CancellationToken ct = default)
    {
        EnsureValidRole(role);
        var user = await _userManager.FindByIdAsync(userId.ToString())
            ?? throw new NotFoundException("User not found.");

        var result = await _userManager.RemoveFromRoleAsync(user, role);
        if (!result.Succeeded)
            throw new DomainException(string.Join("; ", result.Errors.Select(e => e.Description)));
    }

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

    private static void EnsureValidRole(string role)
    {
        if (!Roles.All.Contains(role))
            throw new ValidationException($"Unknown role '{role}'. Valid roles: {string.Join(", ", Roles.All)}.");
    }
}
