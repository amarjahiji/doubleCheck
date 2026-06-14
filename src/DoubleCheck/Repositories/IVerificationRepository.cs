using DoubleCheck.Entities;

namespace DoubleCheck.Repositories;

public interface IVerificationRepository
{
    Task AddAsync(VerificationSession session, CancellationToken ct = default);
    Task<VerificationSession?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<VerificationSession>> GetRequesterSessionsAsync(Guid requesterUserId, CancellationToken ct = default);
    Task<IReadOnlyList<VerificationSession>> GetOpenIncomingSessionsAsync(Guid professionalUserId, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
    Task ExecuteInTransactionAsync(Func<CancellationToken, Task> action, CancellationToken ct = default);
}
