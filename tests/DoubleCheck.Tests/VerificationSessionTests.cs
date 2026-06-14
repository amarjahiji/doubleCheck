using DoubleCheck.Abstractions;
using DoubleCheck.Data;
using DoubleCheck.Entities;
using DoubleCheck.Enums;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using Xunit;

namespace DoubleCheck.Tests;

/// <summary>Examples for Drin:(1) EF InMemory data-layer query, (2) NSubstitute mock of IWalletService.</summary>
public class VerificationSessionTests
{
    private static AppDbContext NewDb() =>
        new(new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);

    [Fact]
    public async Task IncomingQuery_ReturnsOnlyOpenSessionsForThatProfessional()
    {
        // Arrange
        await using var db = NewDb();
        var pro = Guid.NewGuid();
        db.VerificationSessions.AddRange(
            new VerificationSession { ProfessionalUserId = pro, Status = SessionStatus.Open, QuestionSnapshot = "q", AiAnswerSnapshot = "a" },
            new VerificationSession { ProfessionalUserId = pro, Status = SessionStatus.Closed, QuestionSnapshot = "q", AiAnswerSnapshot = "a" },
            new VerificationSession { ProfessionalUserId = Guid.NewGuid(), Status = SessionStatus.Open, QuestionSnapshot = "q", AiAnswerSnapshot = "a" });
        await db.SaveChangesAsync();

        // Act
        var incoming = await db.VerificationSessions
            .Where(s => s.ProfessionalUserId == pro && s.Status == SessionStatus.Open).ToListAsync();

        // Assert
        Assert.Single(incoming);
    }

    [Fact]
    public async Task WalletService_MockedDebit_CanBeArrangedAndAsserted()
    {
        // Arrange (the NSubstitute pattern Drin uses for IWalletService)
        var wallet = Substitute.For<IWalletService>();
        var requester = Guid.NewGuid();
        wallet.TryDebitAsync(requester, 25m, Arg.Any<Guid?>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
              .Returns(false);

        // Act
        var ok = await wallet.TryDebitAsync(requester, 25m, null, "test");

        // Assert
        Assert.False(ok);
        await wallet.Received(1).TryDebitAsync(requester, 25m, Arg.Any<Guid?>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
    }
}
