using DoubleCheck.Dtos;

namespace DoubleCheck.Abstractions;

public interface IAdminService
{
    Task AssignRoleAsync(Guid userId, string role, CancellationToken ct = default);
    Task RevokeRoleAsync(Guid userId, string role, CancellationToken ct = default);
    Task<ProfessionalProfileResponse> ApproveApplicationAsync(Guid applicationId, CancellationToken ct = default);
    Task<ProfessionalApplicationResponse> RejectApplicationAsync(Guid applicationId, CancellationToken ct = default);
}
