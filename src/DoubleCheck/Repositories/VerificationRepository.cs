using System.Data;
using DoubleCheck.Data;
using DoubleCheck.Entities;
using DoubleCheck.Enums;
using Microsoft.EntityFrameworkCore;

namespace DoubleCheck.Repositories;

/// <summary>Provides EF Core persistence operations for verification sessions.</summary>
public class VerificationRepository : IVerificationRepository
{
    private readonly AppDbContext _db;

    /// <summary>Creates a repository backed by the application database context.</summary>
    public VerificationRepository(AppDbContext db) => _db = db;

    /// <inheritdoc />
    public async Task AddAsync(VerificationSession session, CancellationToken ct = default)
        => await _db.VerificationSessions.AddAsync(session, ct);

    /// <inheritdoc />
    public async Task<VerificationSession?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await _db.VerificationSessions.FirstOrDefaultAsync(session => session.Id == id, ct);

    /// <inheritdoc />
    public async Task<IReadOnlyList<VerificationSession>> GetRequesterSessionsAsync(
        Guid requesterUserId,
        CancellationToken ct = default)
    {
        return await _db.VerificationSessions
            .AsNoTracking()
            .Where(session => session.RequesterUserId == requesterUserId)
            .OrderByDescending(session => session.CreatedAt)
            .ToListAsync(ct);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<VerificationSession>> GetOpenIncomingSessionsAsync(
        Guid professionalUserId,
        CancellationToken ct = default)
    {
        return await _db.VerificationSessions
            .AsNoTracking()
            .Where(session => session.ProfessionalUserId == professionalUserId && session.Status == SessionStatus.Open)
            .OrderBy(session => session.CreatedAt)
            .ToListAsync(ct);
    }

    /// <inheritdoc />
    public async Task SaveChangesAsync(CancellationToken ct = default)
        => await _db.SaveChangesAsync(ct);

    /// <inheritdoc />
    public async Task ExecuteInTransactionAsync(Func<CancellationToken, Task> action, CancellationToken ct = default)
    {
        await using var transaction = await _db.Database.BeginTransactionAsync(IsolationLevel.Serializable, ct);
        await action(ct);
        await transaction.CommitAsync(ct);
    }
}
