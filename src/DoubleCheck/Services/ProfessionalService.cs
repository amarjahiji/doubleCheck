using DoubleCheck.Abstractions;
using DoubleCheck.Data;
using DoubleCheck.Dtos;
using DoubleCheck.Entities;
using DoubleCheck.Enums;
using DoubleCheck.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace DoubleCheck.Services;

/// <summary>Professional applications + professional self-management.</summary>
public class ProfessionalService : IProfessionalService
{
    private readonly AppDbContext _db;
    public ProfessionalService(AppDbContext db) => _db = db;

    public async Task<ProfessionalApplicationResponse> ApplyAsync(Guid userId, ApplyProfessionalRequest request, CancellationToken ct = default)
    {
        if (await _db.ProfessionalProfiles.AnyAsync(p => p.UserId == userId, ct))
            throw new ConflictException("You are already a professional.");

        if (await _db.ProfessionalApplications.AnyAsync(a => a.UserId == userId && a.Status == ApplicationStatus.Pending, ct))
            throw new ConflictException("You already have a pending application.");

        var categoryIds = request.CategoryIds.Distinct().ToList();
        if (categoryIds.Count == 0)
            throw new ValidationException("At least one category is required.");

        var validCount = await _db.Categories.CountAsync(c => categoryIds.Contains(c.Id), ct);
        if (validCount != categoryIds.Count)
            throw new ValidationException("One or more categories do not exist.");

        var application = new ProfessionalApplication
        {
            UserId = userId,
            RequestedRate = request.RequestedRate,
            Bio = request.Bio,
            Status = ApplicationStatus.Pending,
            Categories = categoryIds.Select(id => new ProfessionalApplicationCategory { CategoryId = id }).ToList()
        };
        _db.ProfessionalApplications.Add(application);
        await _db.SaveChangesAsync(ct);

        return ToApplicationResponse(application);
    }

    public async Task<ProfessionalApplicationResponse> GetMyApplicationAsync(Guid userId, CancellationToken ct = default)
    {
        var application = await _db.ProfessionalApplications
            .Include(a => a.Categories)
            .Where(a => a.UserId == userId)
            .OrderByDescending(a => a.CreatedAt)
            .FirstOrDefaultAsync(ct)
            ?? throw new NotFoundException("No application found.");

        return ToApplicationResponse(application);
    }

    public async Task<ProfessionalProfileResponse> UpdateMyProfileAsync(Guid userId, UpdateProfessionalRequest request, CancellationToken ct = default)
    {
        var profile = await _db.ProfessionalProfiles
            .Include(p => p.Categories)
            .FirstOrDefaultAsync(p => p.UserId == userId, ct)
            ?? throw new NotFoundException("Professional profile not found.");

        var categoryIds = request.CategoryIds.Distinct().ToList();
        var validCount = await _db.Categories.CountAsync(c => categoryIds.Contains(c.Id), ct);
        if (validCount != categoryIds.Count)
            throw new ValidationException("One or more categories do not exist.");

        profile.RatePerAnswer = request.RatePerAnswer;
        profile.Bio = request.Bio;
        profile.Categories.Clear();
        foreach (var id in categoryIds)
            profile.Categories.Add(new ProfessionalCategory { ProfessionalProfileId = profile.Id, CategoryId = id });

        await _db.SaveChangesAsync(ct);
        return await GetProfileAsync(userId, ct);
    }

    public async Task<ProfessionalProfileResponse> SetAvailabilityAsync(Guid userId, bool isAvailable, CancellationToken ct = default)
    {
        var profile = await _db.ProfessionalProfiles.FirstOrDefaultAsync(p => p.UserId == userId, ct)
            ?? throw new NotFoundException("Professional profile not found.");
        profile.IsAvailable = isAvailable;
        await _db.SaveChangesAsync(ct);
        return await GetProfileAsync(userId, ct);
    }

    public async Task<ProfessionalProfileResponse> GetProfileAsync(Guid userId, CancellationToken ct = default)
    {
        var profile = await _db.ProfessionalProfiles
            .Include(p => p.Categories)
            .FirstOrDefaultAsync(p => p.UserId == userId, ct)
            ?? throw new NotFoundException("Professional profile not found.");

        var displayName = await _db.Users.Where(u => u.Id == userId).Select(u => u.DisplayName).FirstOrDefaultAsync(ct) ?? string.Empty;

        return new ProfessionalProfileResponse(
            profile.UserId, displayName, profile.RatePerAnswer, profile.IsAvailable, profile.Bio,
            profile.Categories.Select(c => c.CategoryId).ToArray());
    }

    private static ProfessionalApplicationResponse ToApplicationResponse(ProfessionalApplication a) =>
        new(a.Id, a.RequestedRate, a.Bio, a.Status.ToString(),
            a.Categories.Select(c => c.CategoryId).ToArray(), a.CreatedAt, a.DecidedAt);
}
