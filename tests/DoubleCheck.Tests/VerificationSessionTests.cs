using DoubleCheck.Abstractions;
using DoubleCheck.Controllers;
using DoubleCheck.Data;
using DoubleCheck.Dtos;
using DoubleCheck.Entities;
using DoubleCheck.Enums;
using DoubleCheck.Exceptions;
using DoubleCheck.Repositories;
using DoubleCheck.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using NSubstitute;
using Xunit;

namespace DoubleCheck.Tests;

/// <summary>Tests the verification workflow services, controller actions, and repositories.</summary>
public class VerificationSessionTests
{
    private static AppDbContext NewDb() =>
        new(new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);

    private static ICurrentUser CurrentUser(Guid userId)
    {
        var currentUser = Substitute.For<ICurrentUser>();
        currentUser.IsAuthenticated.Returns(true);
        currentUser.UserId.Returns(userId);
        return currentUser;
    }

    private static void ExecuteTransactionsInline(IVerificationRepository sessions)
    {
        sessions.ExecuteInTransactionAsync(Arg.Any<Func<CancellationToken, Task>>(), Arg.Any<CancellationToken>())
            .Returns(call =>
            {
                var action = call.Arg<Func<CancellationToken, Task>>();
                var ct = call.ArgAt<CancellationToken>(1);
                return action(ct);
            });
    }

    /// <summary>Verifies that expert matching returns available professionals for a category.</summary>
    [Fact]
    public async Task ExpertMatchingService_ReturnsAvailableExpertsForCategory()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var repo = Substitute.For<IProfessionalReadRepository>();
        IReadOnlyList<ProfessionalExpertMatch> matches = new[]
        {
            new ProfessionalExpertMatch(
                Guid.NewGuid(),
                "Ada",
                20m,
                new[] { new ProfessionalExpertCategory(categoryId, "Tech") })
        };
        repo.GetAvailableExpertsByCategoryAsync(categoryId, Arg.Any<CancellationToken>())
            .Returns(matches);
        using var cache = new MemoryCache(new MemoryCacheOptions());
        var sut = new ExpertMatchingService(repo, cache);

        // Act
        var result = await sut.GetMatchingExpertsAsync(categoryId);

        // Assert
        var expert = Assert.Single(result);
        Assert.Equal("Ada", expert.DisplayName);
        Assert.Equal(20m, expert.Rate);
        await repo.Received(1).GetAvailableExpertsByCategoryAsync(categoryId, Arg.Any<CancellationToken>());
    }

    /// <summary>Verifies that expert matching returns an empty list when no professionals are available.</summary>
    [Fact]
    public async Task ExpertMatchingService_WhenNoneAvailable_ReturnsEmptyList()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var repo = Substitute.For<IProfessionalReadRepository>();
        IReadOnlyList<ProfessionalExpertMatch> matches = Array.Empty<ProfessionalExpertMatch>();
        repo.GetAvailableExpertsByCategoryAsync(categoryId, Arg.Any<CancellationToken>())
            .Returns(matches);
        using var cache = new MemoryCache(new MemoryCacheOptions());
        var sut = new ExpertMatchingService(repo, cache);

        // Act
        var result = await sut.GetMatchingExpertsAsync(categoryId);

        // Assert
        Assert.Empty(result);
    }

    /// <summary>Verifies that repeated expert matching for the same category uses the cache.</summary>
    [Fact]
    public async Task ExpertMatchingService_RepeatedCategory_UsesCache()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var repo = Substitute.For<IProfessionalReadRepository>();
        IReadOnlyList<ProfessionalExpertMatch> matches = new[]
        {
            new ProfessionalExpertMatch(
                Guid.NewGuid(),
                "Grace",
                25m,
                new[] { new ProfessionalExpertCategory(categoryId, "Tech") })
        };
        repo.GetAvailableExpertsByCategoryAsync(categoryId, Arg.Any<CancellationToken>())
            .Returns(matches);
        using var cache = new MemoryCache(new MemoryCacheOptions());
        var sut = new ExpertMatchingService(repo, cache);

        // Act
        var first = await sut.GetMatchingExpertsAsync(categoryId);
        var second = await sut.GetMatchingExpertsAsync(categoryId);

        // Assert
        Assert.Single(first);
        Assert.Single(second);
        await repo.Received(1).GetAvailableExpertsByCategoryAsync(categoryId, Arg.Any<CancellationToken>());
    }

    /// <summary>Verifies that creating a session stores snapshots and the agreed professional rate.</summary>
    [Fact]
    public async Task CreateSessionAsync_CreatesOpenSessionWithSnapshotsAndAgreedRate()
    {
        // Arrange
        var requesterId = Guid.NewGuid();
        var professionalId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();
        var sessions = Substitute.For<IVerificationRepository>();
        var professionals = Substitute.For<IProfessionalReadRepository>();
        var wallet = Substitute.For<IWalletService>();
        VerificationSession? saved = null;

        professionals.GetAvailableProfessionalForCategoryAsync(professionalId, categoryId, Arg.Any<CancellationToken>())
            .Returns(new ProfessionalSelection(professionalId, 35m));
        sessions.AddAsync(Arg.Do<VerificationSession>(session => saved = session), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        var sut = new VerificationService(sessions, professionals, wallet, CurrentUser(requesterId));

        // Act
        var result = await sut.CreateSessionAsync(new CreateVerificationSessionRequest
        {
            ProfessionalUserId = professionalId,
            CategoryId = categoryId,
            QuestionText = "  check this  ",
            AiAnswerText = "  ai answer  "
        });

        // Assert
        Assert.NotNull(saved);
        Assert.Equal(requesterId, saved!.RequesterUserId);
        Assert.Equal(professionalId, saved.ProfessionalUserId);
        Assert.Equal("check this", saved.QuestionSnapshot);
        Assert.Equal("ai answer", saved.AiAnswerSnapshot);
        Assert.Equal(35m, saved.AgreedRate);
        Assert.Equal(SessionStatus.Open, result.Status);
        Assert.Equal(SessionOutcome.None, result.Outcome);
        await sessions.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    /// <summary>Verifies that session creation rejects unavailable professionals.</summary>
    [Fact]
    public async Task CreateSessionAsync_WhenProfessionalUnavailable_ThrowsValidationException()
    {
        // Arrange
        var sessions = Substitute.For<IVerificationRepository>();
        var professionals = Substitute.For<IProfessionalReadRepository>();
        var wallet = Substitute.For<IWalletService>();
        var sut = new VerificationService(sessions, professionals, wallet, CurrentUser(Guid.NewGuid()));

        // Act
        var act = () => sut.CreateSessionAsync(new CreateVerificationSessionRequest
        {
            ProfessionalUserId = Guid.NewGuid(),
            CategoryId = Guid.NewGuid(),
            QuestionText = "question",
            AiAnswerText = "answer"
        });

        // Assert
        await Assert.ThrowsAsync<ValidationException>(act);
        await sessions.DidNotReceive().AddAsync(Arg.Any<VerificationSession>(), Arg.Any<CancellationToken>());
    }

    /// <summary>Verifies that requesters cannot create a verification session with themselves.</summary>
    [Fact]
    public async Task CreateSessionAsync_WhenRequesterSelectsSelf_ThrowsValidationException()
    {
        // Arrange
        var requesterId = Guid.NewGuid();
        var sessions = Substitute.For<IVerificationRepository>();
        var professionals = Substitute.For<IProfessionalReadRepository>();
        var wallet = Substitute.For<IWalletService>();
        var sut = new VerificationService(sessions, professionals, wallet, CurrentUser(requesterId));

        // Act
        var act = () => sut.CreateSessionAsync(new CreateVerificationSessionRequest
        {
            ProfessionalUserId = requesterId,
            CategoryId = Guid.NewGuid(),
            QuestionText = "question",
            AiAnswerText = "answer"
        });

        // Assert
        await Assert.ThrowsAsync<ValidationException>(act);
        await professionals.DidNotReceive().GetAvailableProfessionalForCategoryAsync(
            Arg.Any<Guid>(),
            Arg.Any<Guid>(),
            Arg.Any<CancellationToken>());
    }

    /// <summary>Verifies that resolving a session closes it and credits the professional after debit succeeds.</summary>
    [Fact]
    public async Task ResolveSessionAsync_WhenDebitSucceeds_ClosesSessionAndCreditsProfessional()
    {
        // Arrange
        var requesterId = Guid.NewGuid();
        var professionalId = Guid.NewGuid();
        var session = new VerificationSession
        {
            Id = Guid.NewGuid(),
            RequesterUserId = requesterId,
            ProfessionalUserId = professionalId,
            CategoryId = Guid.NewGuid(),
            AgreedRate = 45m,
            Status = SessionStatus.Open,
            QuestionSnapshot = "question",
            AiAnswerSnapshot = "answer"
        };
        var sessions = Substitute.For<IVerificationRepository>();
        var professionals = Substitute.For<IProfessionalReadRepository>();
        var wallet = Substitute.For<IWalletService>();
        sessions.GetByIdAsync(session.Id, Arg.Any<CancellationToken>()).Returns(session);
        ExecuteTransactionsInline(sessions);
        wallet.TryDebitAsync(requesterId, 45m, session.Id, Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(true);
        var sut = new VerificationService(sessions, professionals, wallet, CurrentUser(professionalId));

        // Act
        var result = await sut.ResolveSessionAsync(session.Id, new ResolveVerificationSessionRequest { Solution = "  fixed  " });

        // Assert
        Assert.Equal(SessionStatus.Closed, session.Status);
        Assert.Equal(SessionOutcome.Resolved, session.Outcome);
        Assert.Equal("fixed", session.ExpertSolution);
        Assert.NotNull(session.ClosedAt);
        Assert.Equal(SessionOutcome.Resolved, result.Outcome);
        await wallet.Received(1).CreditAsync(professionalId, 45m, session.Id, Arg.Any<string>(), Arg.Any<CancellationToken>());
        await sessions.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    /// <summary>Verifies that failed requester debit leaves the session open and skips professional credit.</summary>
    [Fact]
    public async Task ResolveSessionAsync_WhenDebitFails_LeavesSessionOpenAndDoesNotCredit()
    {
        // Arrange
        var requesterId = Guid.NewGuid();
        var professionalId = Guid.NewGuid();
        var session = new VerificationSession
        {
            Id = Guid.NewGuid(),
            RequesterUserId = requesterId,
            ProfessionalUserId = professionalId,
            CategoryId = Guid.NewGuid(),
            AgreedRate = 45m,
            Status = SessionStatus.Open,
            QuestionSnapshot = "question",
            AiAnswerSnapshot = "answer"
        };
        var sessions = Substitute.For<IVerificationRepository>();
        var professionals = Substitute.For<IProfessionalReadRepository>();
        var wallet = Substitute.For<IWalletService>();
        sessions.GetByIdAsync(session.Id, Arg.Any<CancellationToken>()).Returns(session);
        ExecuteTransactionsInline(sessions);
        wallet.TryDebitAsync(requesterId, 45m, session.Id, Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(false);
        var sut = new VerificationService(sessions, professionals, wallet, CurrentUser(professionalId));

        // Act
        var act = () => sut.ResolveSessionAsync(session.Id, new ResolveVerificationSessionRequest { Solution = "fixed" });

        // Assert
        await Assert.ThrowsAsync<DomainException>(act);
        Assert.Equal(SessionStatus.Open, session.Status);
        Assert.Equal(SessionOutcome.None, session.Outcome);
        await wallet.DidNotReceive().CreditAsync(Arg.Any<Guid>(), Arg.Any<decimal>(), Arg.Any<Guid?>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
        await sessions.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    /// <summary>Verifies that non-assigned professionals cannot resolve a verification session.</summary>
    [Fact]
    public async Task ResolveSessionAsync_WhenNotAssignedProfessional_ThrowsForbiddenException()
    {
        // Arrange
        var session = new VerificationSession
        {
            Id = Guid.NewGuid(),
            RequesterUserId = Guid.NewGuid(),
            ProfessionalUserId = Guid.NewGuid(),
            CategoryId = Guid.NewGuid(),
            Status = SessionStatus.Open,
            QuestionSnapshot = "question",
            AiAnswerSnapshot = "answer"
        };
        var sessions = Substitute.For<IVerificationRepository>();
        var professionals = Substitute.For<IProfessionalReadRepository>();
        var wallet = Substitute.For<IWalletService>();
        sessions.GetByIdAsync(session.Id, Arg.Any<CancellationToken>()).Returns(session);
        var sut = new VerificationService(sessions, professionals, wallet, CurrentUser(Guid.NewGuid()));

        // Act
        var act = () => sut.ResolveSessionAsync(session.Id, new ResolveVerificationSessionRequest { Solution = "fixed" });

        // Assert
        await Assert.ThrowsAsync<ForbiddenException>(act);
        await wallet.DidNotReceive().TryDebitAsync(Arg.Any<Guid>(), Arg.Any<decimal>(), Arg.Any<Guid?>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    /// <summary>Verifies that the requester can cancel an open verification session.</summary>
    [Fact]
    public async Task CancelSessionAsync_WhenRequesterOwnsOpenSession_ClosesAsCancelled()
    {
        // Arrange
        var requesterId = Guid.NewGuid();
        var session = new VerificationSession
        {
            Id = Guid.NewGuid(),
            RequesterUserId = requesterId,
            ProfessionalUserId = Guid.NewGuid(),
            CategoryId = Guid.NewGuid(),
            Status = SessionStatus.Open,
            QuestionSnapshot = "question",
            AiAnswerSnapshot = "answer"
        };
        var sessions = Substitute.For<IVerificationRepository>();
        var professionals = Substitute.For<IProfessionalReadRepository>();
        var wallet = Substitute.For<IWalletService>();
        sessions.GetByIdAsync(session.Id, Arg.Any<CancellationToken>()).Returns(session);
        var sut = new VerificationService(sessions, professionals, wallet, CurrentUser(requesterId));

        // Act
        var result = await sut.CancelSessionAsync(session.Id);

        // Assert
        Assert.Equal(SessionStatus.Closed, session.Status);
        Assert.Equal(SessionOutcome.Cancelled, result.Outcome);
        Assert.NotNull(session.ClosedAt);
        await sessions.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    /// <summary>Verifies that cancelling an already closed session raises a conflict.</summary>
    [Fact]
    public async Task CancelSessionAsync_WhenAlreadyClosed_ThrowsConflictException()
    {
        // Arrange
        var requesterId = Guid.NewGuid();
        var session = new VerificationSession
        {
            Id = Guid.NewGuid(),
            RequesterUserId = requesterId,
            ProfessionalUserId = Guid.NewGuid(),
            CategoryId = Guid.NewGuid(),
            Status = SessionStatus.Closed,
            Outcome = SessionOutcome.Resolved,
            QuestionSnapshot = "question",
            AiAnswerSnapshot = "answer"
        };
        var sessions = Substitute.For<IVerificationRepository>();
        var professionals = Substitute.For<IProfessionalReadRepository>();
        var wallet = Substitute.For<IWalletService>();
        sessions.GetByIdAsync(session.Id, Arg.Any<CancellationToken>()).Returns(session);
        var sut = new VerificationService(sessions, professionals, wallet, CurrentUser(requesterId));

        // Act
        var act = () => sut.CancelSessionAsync(session.Id);

        // Assert
        await Assert.ThrowsAsync<ConflictException>(act);
    }

    /// <summary>Verifies that the create endpoint returns a CreatedAtAction result.</summary>
    [Fact]
    public async Task VerificationController_CreateSession_ReturnsCreatedAtAction()
    {
        // Arrange
        var service = Substitute.For<IVerificationService>();
        var matching = Substitute.For<IExpertMatchingService>();
        var response = new VerificationSessionResponse(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            null,
            "question",
            "answer",
            20m,
            SessionStatus.Open,
            SessionOutcome.None,
            null,
            null,
            DateTime.UtcNow);
        service.CreateSessionAsync(Arg.Any<CreateVerificationSessionRequest>(), Arg.Any<CancellationToken>())
            .Returns(response);
        var controller = new VerificationController(matching, service);

        // Act
        var result = await controller.CreateSession(new CreateVerificationSessionRequest(), CancellationToken.None);

        // Assert
        var created = Assert.IsType<CreatedAtActionResult>(result.Result);
        Assert.Equal(nameof(VerificationController.GetSession), created.ActionName);
        Assert.Same(response, created.Value);
    }

    /// <summary>Verifies that forbidden resolve attempts propagate through the controller.</summary>
    [Fact]
    public async Task VerificationController_ResolveSession_WhenServiceForbids_ThrowsForbiddenException()
    {
        // Arrange
        var service = Substitute.For<IVerificationService>();
        var matching = Substitute.For<IExpertMatchingService>();
        var id = Guid.NewGuid();
        service.ResolveSessionAsync(id, Arg.Any<ResolveVerificationSessionRequest>(), Arg.Any<CancellationToken>())
            .Returns(_ => Task.FromException<VerificationSessionResponse>(new ForbiddenException("forbidden")));
        var controller = new VerificationController(matching, service);

        // Act
        var act = () => controller.ResolveSession(id, new ResolveVerificationSessionRequest { Solution = "fixed" }, CancellationToken.None);

        // Assert
        await Assert.ThrowsAsync<ForbiddenException>(act);
    }

    /// <summary>Verifies that incoming repository queries return only open sessions for the professional.</summary>
    [Fact]
    public async Task VerificationRepository_Incoming_ReturnsOnlyOpenSessionsForProfessional()
    {
        // Arrange
        await using var db = NewDb();
        var pro = Guid.NewGuid();
        db.VerificationSessions.AddRange(
            new VerificationSession { ProfessionalUserId = pro, Status = SessionStatus.Open, QuestionSnapshot = "q", AiAnswerSnapshot = "a" },
            new VerificationSession { ProfessionalUserId = pro, Status = SessionStatus.Closed, QuestionSnapshot = "q", AiAnswerSnapshot = "a" },
            new VerificationSession { ProfessionalUserId = Guid.NewGuid(), Status = SessionStatus.Open, QuestionSnapshot = "q", AiAnswerSnapshot = "a" });
        await db.SaveChangesAsync();
        var sut = new VerificationRepository(db);

        // Act
        var incoming = await sut.GetOpenIncomingSessionsAsync(pro);

        // Assert
        Assert.Single(incoming);
        Assert.All(incoming, session => Assert.Equal(SessionStatus.Open, session.Status));
    }

    /// <summary>Verifies that incoming repository queries exclude closed sessions.</summary>
    [Fact]
    public async Task VerificationRepository_Incoming_ExcludesClosedSessions()
    {
        // Arrange
        await using var db = NewDb();
        var pro = Guid.NewGuid();
        db.VerificationSessions.Add(
            new VerificationSession { ProfessionalUserId = pro, Status = SessionStatus.Closed, QuestionSnapshot = "q", AiAnswerSnapshot = "a" });
        await db.SaveChangesAsync();
        var sut = new VerificationRepository(db);

        // Act
        var incoming = await sut.GetOpenIncomingSessionsAsync(pro);

        // Assert
        Assert.Empty(incoming);
    }
}
