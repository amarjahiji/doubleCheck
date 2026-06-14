using DoubleCheck.Dtos;

namespace DoubleCheck.Services;

public interface IExpertMatchingService
{
    Task<IReadOnlyList<ExpertMatchResponse>> GetMatchingExpertsAsync(
        Guid categoryId,
        CancellationToken ct = default);
}
