using DoubleCheck.Dtos;
using DoubleCheck.Exceptions;
using DoubleCheck.Repositories;
using Microsoft.Extensions.Caching.Memory;

namespace DoubleCheck.Services;

public class ExpertMatchingService : IExpertMatchingService
{
    private static readonly TimeSpan CacheTtl = TimeSpan.FromSeconds(45);

    private readonly IProfessionalReadRepository _professionals;
    private readonly IMemoryCache _cache;

    public ExpertMatchingService(IProfessionalReadRepository professionals, IMemoryCache cache)
    {
        _professionals = professionals;
        _cache = cache;
    }

    public async Task<IReadOnlyList<ExpertMatchResponse>> GetMatchingExpertsAsync(
        Guid categoryId,
        CancellationToken ct = default)
    {
        if (categoryId == Guid.Empty)
            throw new ValidationException("CategoryId is required.");

        var cacheKey = $"experts:{categoryId}";
        var cached = await _cache.GetOrCreateAsync<IReadOnlyList<ExpertMatchResponse>>(cacheKey, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = CacheTtl;

            var experts = await _professionals.GetAvailableExpertsByCategoryAsync(categoryId, ct);
            return experts
                .Select(expert => new ExpertMatchResponse(
                    expert.UserId,
                    expert.DisplayName,
                    expert.Rate,
                    expert.Categories
                        .Select(category => new ExpertCategoryResponse(category.Id, category.Name))
                        .ToList()))
                .ToList();
        });

        return cached ?? Array.Empty<ExpertMatchResponse>();
    }
}
