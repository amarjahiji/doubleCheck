using DoubleCheck.Data;
using Microsoft.EntityFrameworkCore;

namespace DoubleCheck.Repositories;

public class ProfessionalReadRepository : IProfessionalReadRepository
{
    private readonly AppDbContext _db;

    public ProfessionalReadRepository(AppDbContext db) => _db = db;

    public async Task<IReadOnlyList<ProfessionalExpertMatch>> GetAvailableExpertsByCategoryAsync(
        Guid categoryId,
        CancellationToken ct = default)
    {
        var experts = await (
            from profile in _db.ProfessionalProfiles.AsNoTracking()
            join user in _db.Users.AsNoTracking() on profile.UserId equals user.Id
            where profile.IsAvailable && profile.Categories.Any(category => category.CategoryId == categoryId)
            orderby profile.RatePerAnswer, user.DisplayName
            select new
            {
                ProfileId = profile.Id,
                profile.UserId,
                user.DisplayName,
                Rate = profile.RatePerAnswer
            })
            .ToListAsync(ct);

        if (experts.Count == 0)
            return Array.Empty<ProfessionalExpertMatch>();

        var profileIds = experts.Select(expert => expert.ProfileId).ToList();
        var categories = await (
            from professionalCategory in _db.ProfessionalCategories.AsNoTracking()
            join category in _db.Categories.AsNoTracking()
                on professionalCategory.CategoryId equals category.Id
            where profileIds.Contains(professionalCategory.ProfessionalProfileId)
            orderby category.Name
            select new
            {
                professionalCategory.ProfessionalProfileId,
                Category = new ProfessionalExpertCategory(category.Id, category.Name)
            })
            .ToListAsync(ct);

        var categoriesByProfile = categories
            .GroupBy(item => item.ProfessionalProfileId)
            .ToDictionary(group => group.Key, group => group.Select(item => item.Category).ToList());

        return experts
            .Select(expert => new ProfessionalExpertMatch(
                expert.UserId,
                expert.DisplayName,
                expert.Rate,
                categoriesByProfile.TryGetValue(expert.ProfileId, out var expertCategories)
                    ? (IReadOnlyList<ProfessionalExpertCategory>)expertCategories
                    : Array.Empty<ProfessionalExpertCategory>()))
            .ToList();
    }

    public async Task<ProfessionalSelection?> GetAvailableProfessionalForCategoryAsync(
        Guid professionalUserId,
        Guid categoryId,
        CancellationToken ct = default)
    {
        return await _db.ProfessionalProfiles
            .AsNoTracking()
            .Where(profile =>
                profile.UserId == professionalUserId &&
                profile.IsAvailable &&
                profile.Categories.Any(category => category.CategoryId == categoryId))
            .Select(profile => new ProfessionalSelection(profile.UserId, profile.RatePerAnswer))
            .FirstOrDefaultAsync(ct);
    }
}
