using DoubleCheck.Abstractions;
using DoubleCheck.Data;
using DoubleCheck.Dtos;
using DoubleCheck.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace DoubleCheck.Services;

public class AccountService : IAccountService
{
    private const int RecentCount = 10;
    private readonly AppDbContext _db;
    public AccountService(AppDbContext db) => _db = db;

    public async Task<BalanceResponse> GetBalanceAsync(Guid userId, CancellationToken ct = default)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == userId, ct)
            ?? throw new NotFoundException("User not found.");

        var recent = await _db.WalletTransactions
            .Where(t => t.UserId == userId)
            .OrderByDescending(t => t.CreatedAt)
            .Take(RecentCount)
            .Select(t => new WalletTransactionResponse(t.Amount, t.Type.ToString(), t.Reason, t.RelatedSessionId, t.CreatedAt))
            .ToListAsync(ct);

        return new BalanceResponse(user.Balance, recent);
    }
}
