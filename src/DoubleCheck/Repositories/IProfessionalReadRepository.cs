namespace DoubleCheck.Repositories;

public interface IProfessionalReadRepository
{
    Task<IReadOnlyList<ProfessionalExpertMatch>> GetAvailableExpertsByCategoryAsync(
        Guid categoryId,
        CancellationToken ct = default);

    Task<ProfessionalSelection?> GetAvailableProfessionalForCategoryAsync(
        Guid professionalUserId,
        Guid categoryId,
        CancellationToken ct = default);
}

public sealed record ProfessionalExpertCategory(Guid Id, string Name);

public sealed record ProfessionalExpertMatch(
    Guid UserId,
    string DisplayName,
    decimal Rate,
    IReadOnlyList<ProfessionalExpertCategory> Categories);

public sealed record ProfessionalSelection(Guid UserId, decimal Rate);
