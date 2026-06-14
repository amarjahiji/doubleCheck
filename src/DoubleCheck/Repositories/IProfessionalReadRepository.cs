namespace DoubleCheck.Repositories;

/// <summary>Defines read-only professional profile queries needed by verification workflows.</summary>
public interface IProfessionalReadRepository
{
    /// <summary>Lists available professionals who cover the requested category.</summary>
    Task<IReadOnlyList<ProfessionalExpertMatch>> GetAvailableExpertsByCategoryAsync(
        Guid categoryId,
        CancellationToken ct = default);

    /// <summary>Gets an available professional for a selected category, including their current rate.</summary>
    Task<ProfessionalSelection?> GetAvailableProfessionalForCategoryAsync(
        Guid professionalUserId,
        Guid categoryId,
        CancellationToken ct = default);
}

/// <summary>Represents a category attached to a matched professional.</summary>
public sealed record ProfessionalExpertCategory(Guid Id, string Name);

/// <summary>Represents an available professional returned from expert matching.</summary>
public sealed record ProfessionalExpertMatch(
    Guid UserId,
    string DisplayName,
    decimal Rate,
    IReadOnlyList<ProfessionalExpertCategory> Categories);

/// <summary>Represents the selected professional and rate used when creating a session.</summary>
public sealed record ProfessionalSelection(Guid UserId, decimal Rate);
