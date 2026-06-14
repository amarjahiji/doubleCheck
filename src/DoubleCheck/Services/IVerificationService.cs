using DoubleCheck.Dtos;

namespace DoubleCheck.Services;

public interface IVerificationService
{
    Task<VerificationSessionResponse> CreateSessionAsync(
        CreateVerificationSessionRequest request,
        CancellationToken ct = default);

    Task<IReadOnlyList<VerificationSessionResponse>> GetMySessionsAsync(CancellationToken ct = default);

    Task<IReadOnlyList<VerificationSessionResponse>> GetIncomingSessionsAsync(CancellationToken ct = default);

    Task<VerificationSessionResponse> GetSessionAsync(Guid id, CancellationToken ct = default);

    Task<VerificationSessionResponse> ResolveSessionAsync(
        Guid id,
        ResolveVerificationSessionRequest request,
        CancellationToken ct = default);

    Task<VerificationSessionResponse> CancelSessionAsync(Guid id, CancellationToken ct = default);
}
