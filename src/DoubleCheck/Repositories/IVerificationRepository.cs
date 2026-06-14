using DoubleCheck.Entities;

namespace DoubleCheck.Repositories;

/// <summary>Defines persistence operations used by the verification workflow.</summary>
public interface IVerificationRepository
{
    /// <summary>Adds a new verification session to the current unit of work.</summary>
    Task AddAsync(VerificationSession session, CancellationToken ct = default);

    /// <summary>Gets a verification session by identifier, returning a tracked entity when found.</summary>
    Task<VerificationSession?> GetByIdAsync(Guid id, CancellationToken ct = default);

    /// <summary>Lists sessions created by the specified requester.</summary>
    Task<IReadOnlyList<VerificationSession>> GetRequesterSessionsAsync(Guid requesterUserId, CancellationToken ct = default);

    /// <summary>Lists open sessions assigned to the specified professional.</summary>
    Task<IReadOnlyList<VerificationSession>> GetOpenIncomingSessionsAsync(Guid professionalUserId, CancellationToken ct = default);

    /// <summary>Persists pending verification session changes.</summary>
    Task SaveChangesAsync(CancellationToken ct = default);

    /// <summary>Runs the supplied action inside a serializable database transaction.</summary>
    Task ExecuteInTransactionAsync(Func<CancellationToken, Task> action, CancellationToken ct = default);
}
