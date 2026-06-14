using DoubleCheck.Data;
using DoubleCheck.Entities;
using DoubleCheck.Enums;
using DoubleCheck.Exceptions;
using DoubleCheck.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace DoubleCheck.Tests;

public class AccountServiceTests
{
    private static AppDbContext NewDb() =>
        new(new DbContextOptionsBuilder<AppDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);

    [Fact]
    public async Task GetBalanceAsync_ReturnsBalanceAndRecentTransactions()
    {
        // Arrange
        await using var db = NewDb();
        var user = new ApplicationUser { Id = Guid.NewGuid(), UserName = "u", Email = "u@e.com", Balance = 42m };
        db.Users.Add(user);
        db.WalletTransactions.Add(new WalletTransaction { UserId = user.Id, Amount = 10m, Type = TransactionType.Debit, Reason = "x" });
        await db.SaveChangesAsync();
        var sut = new AccountService(db);

        // Act
        var result = await sut.GetBalanceAsync(user.Id);

        // Assert
        Assert.Equal(42m, result.Balance);
        Assert.Single(result.RecentTransactions);
    }

    [Fact]
    public async Task GetBalanceAsync_UnknownUser_ThrowsNotFound()
    {
        // Arrange
        await using var db = NewDb();
        var sut = new AccountService(db);

        // Act + Assert
        await Assert.ThrowsAsync<NotFoundException>(() => sut.GetBalanceAsync(Guid.NewGuid()));
    }
}
