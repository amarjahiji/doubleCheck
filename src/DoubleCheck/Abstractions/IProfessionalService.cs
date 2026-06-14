using DoubleCheck.Dtos;

namespace DoubleCheck.Abstractions;

public interface IProfessionalService
{
    Task<ProfessionalApplicationResponse> ApplyAsync(Guid userId, ApplyProfessionalRequest request, CancellationToken ct = default);
    Task<ProfessionalApplicationResponse> GetMyApplicationAsync(Guid userId, CancellationToken ct = default);
    Task<ProfessionalProfileResponse> UpdateMyProfileAsync(Guid userId, UpdateProfessionalRequest request, CancellationToken ct = default);
    Task<ProfessionalProfileResponse> SetAvailabilityAsync(Guid userId, bool isAvailable, CancellationToken ct = default);
    Task<ProfessionalProfileResponse> GetProfileAsync(Guid userId, CancellationToken ct = default);
}
