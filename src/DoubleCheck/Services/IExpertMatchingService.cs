using DoubleCheck.Dtos;

namespace DoubleCheck.Services;

/// <summary>Defines expert matching operations for verification requests.</summary>
public interface IExpertMatchingService
{
    /// <summary>Gets available experts for the requested category.</summary>
    Task<IReadOnlyList<ExpertMatchResponse>> GetMatchingExpertsAsync(
        Guid categoryId,
        CancellationToken ct = default);
}
