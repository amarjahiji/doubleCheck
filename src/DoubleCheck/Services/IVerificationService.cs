using DoubleCheck.Dtos;

namespace DoubleCheck.Services;

/// <summary>Defines business operations for verification sessions.</summary>
public interface IVerificationService
{
    /// <summary>Creates a new open verification session for the current requester.</summary>
    Task<VerificationSessionResponse> CreateSessionAsync(
        CreateVerificationSessionRequest request,
        CancellationToken ct = default);

    /// <summary>Lists sessions created by the current requester.</summary>
    Task<IReadOnlyList<VerificationSessionResponse>> GetMySessionsAsync(CancellationToken ct = default);

    /// <summary>Lists open sessions assigned to the current professional.</summary>
    Task<IReadOnlyList<VerificationSessionResponse>> GetIncomingSessionsAsync(CancellationToken ct = default);

    /// <summary>Gets a session if the current user is the requester or assigned professional.</summary>
    Task<VerificationSessionResponse> GetSessionAsync(Guid id, CancellationToken ct = default);

    /// <summary>Resolves an open session and settles wallet balances.</summary>
    Task<VerificationSessionResponse> ResolveSessionAsync(
        Guid id,
        ResolveVerificationSessionRequest request,
        CancellationToken ct = default);

    /// <summary>Cancels an open session as the requester.</summary>
    Task<VerificationSessionResponse> CancelSessionAsync(Guid id, CancellationToken ct = default);
}
