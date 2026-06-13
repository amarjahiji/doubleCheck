using DoubleCheck.Abstractions;
using DoubleCheck.Data;
using DoubleCheck.Entities;
using DoubleCheck.Enums;
using DoubleCheck.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace DoubleCheck.Services;

/// <summary>Simulated wallet (Amar). Consumed by the verification service via IWalletService.</summary>
public class WalletService : IWalletService
{
    private readonly AppDbContext _db;
    public WalletService(AppDbContext db) => _db = db;

    public async Task<decimal> GetBalanceAsync(Guid userId, CancellationToken ct = default)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == userId, ct)
            ?? throw new NotFoundException($"User {userId} not found.");
        return user.Balance;
    }

    public async Task<bool> TryDebitAsync(Guid userId, decimal amount, Guid? sessionId, string reason, CancellationToken ct = default)
    {
        if (amount < 0) throw new ValidationException("Amount must be non-negative.");
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == userId, ct)
            ?? throw new NotFoundException($"User {userId} not found.");

        if (user.Balance < amount) return false;

        user.Balance -= amount;
        _db.WalletTransactions.Add(new WalletTransaction
        {
            UserId = userId, Amount = amount, Type = TransactionType.Debit,
            RelatedSessionId = sessionId, Reason = reason
        });
        await _db.SaveChangesAsync(ct);
        return true;
    }

    public async Task CreditAsync(Guid userId, decimal amount, Guid? sessionId, string reason, CancellationToken ct = default)
    {
        if (amount < 0) throw new ValidationException("Amount must be non-negative.");
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == userId, ct)
            ?? throw new NotFoundException($"User {userId} not found.");

        user.Balance += amount;
        _db.WalletTransactions.Add(new WalletTransaction
        {
            UserId = userId, Amount = amount, Type = TransactionType.Credit,
            RelatedSessionId = sessionId, Reason = reason
        });
        await _db.SaveChangesAsync(ct);
    }
}
