using DoubleCheck.Data;
using DoubleCheck.Entities;
using DoubleCheck.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace DoubleCheck.Tests;

/// <summary>Service-layer example (Amar): EF InMemory + Arrange-Act-Assert. Happy + sad.</summary>
public class WalletServiceTests
{
    private static AppDbContext NewDb() =>
        new(new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);

    [Fact]
    public async Task TryDebitAsync_WithSufficientBalance_DebitsAndRecordsTransaction()
    {
        // Arrange
        await using var db = NewDb();
        var user = new ApplicationUser { Id = Guid.NewGuid(), UserName = "u", Email = "u@e.com", Balance = 100m };
        db.Users.Add(user);
        await db.SaveChangesAsync();
        var sut = new WalletService(db);

        // Act
        var result = await sut.TryDebitAsync(user.Id, 30m, null, "unit-test");

        // Assert
        Assert.True(result);
        Assert.Equal(70m, (await db.Users.FindAsync(user.Id))!.Balance);
        Assert.Equal(1, await db.WalletTransactions.CountAsync());
    }

    [Fact]
    public async Task TryDebitAsync_WithInsufficientBalance_ReturnsFalseAndChangesNothing()
    {
        // Arrange
        await using var db = NewDb();
        var user = new ApplicationUser { Id = Guid.NewGuid(), UserName = "u", Email = "u@e.com", Balance = 10m };
        db.Users.Add(user);
        await db.SaveChangesAsync();
        var sut = new WalletService(db);

        // Act
        var result = await sut.TryDebitAsync(user.Id, 50m, null, "unit-test");

        // Assert
        Assert.False(result);
        Assert.Equal(10m, (await db.Users.FindAsync(user.Id))!.Balance);
        Assert.Equal(0, await db.WalletTransactions.CountAsync());
    }
}
