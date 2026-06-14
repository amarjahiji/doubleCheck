using DoubleCheck.Abstractions;
using DoubleCheck.Dtos;
using DoubleCheck.Entities;
using DoubleCheck.Enums;
using DoubleCheck.Exceptions;
using DoubleCheck.Repositories;

namespace DoubleCheck.Services;

public class VerificationService : IVerificationService
{
    private readonly IVerificationRepository _sessions;
    private readonly IProfessionalReadRepository _professionals;
    private readonly IWalletService _wallet;
    private readonly ICurrentUser _currentUser;

    public VerificationService(
        IVerificationRepository sessions,
        IProfessionalReadRepository professionals,
        IWalletService wallet,
        ICurrentUser currentUser)
    {
        _sessions = sessions;
        _professionals = professionals;
        _wallet = wallet;
        _currentUser = currentUser;
    }

    public async Task<VerificationSessionResponse> CreateSessionAsync(
        CreateVerificationSessionRequest request,
        CancellationToken ct = default)
    {
        EnsureAuthenticated();
        ValidateCreateRequest(request);
        EnsureRequesterIsNotProfessional(request);

        var professional = await _professionals.GetAvailableProfessionalForCategoryAsync(
            request.ProfessionalUserId,
            request.CategoryId,
            ct);

        if (professional is null)
            throw new ValidationException("Selected professional is unavailable or does not cover this category.");

        var session = new VerificationSession
        {
            RequesterUserId = _currentUser.UserId,
            ProfessionalUserId = professional.UserId,
            CategoryId = request.CategoryId,
            SourceMessageId = request.SourceMessageId,
            QuestionSnapshot = request.QuestionText.Trim(),
            AiAnswerSnapshot = request.AiAnswerText.Trim(),
            AgreedRate = professional.Rate,
            Status = SessionStatus.Open,
            Outcome = SessionOutcome.None
        };

        await _sessions.AddAsync(session, ct);
        await _sessions.SaveChangesAsync(ct);

        return Map(session);
    }

    public async Task<VerificationSessionResponse> ResolveSessionAsync(
        Guid id,
        ResolveVerificationSessionRequest request,
        CancellationToken ct = default)
    {
        EnsureAuthenticated();

        if (id == Guid.Empty)
            throw new ValidationException("Session id is required.");

        if (string.IsNullOrWhiteSpace(request.Solution))
            throw new ValidationException("Solution is required.");

        var session = await _sessions.GetByIdAsync(id, ct)
            ?? throw new NotFoundException("Verification session not found.");

        if (session.ProfessionalUserId != _currentUser.UserId)
            throw new ForbiddenException("Only the assigned professional can resolve this session.");

        EnsureOpen(session);

        await _sessions.ExecuteInTransactionAsync(async token =>
        {
            var debited = await _wallet.TryDebitAsync(
                session.RequesterUserId,
                session.AgreedRate,
                session.Id,
                "Verification session resolved",
                token);

            if (!debited)
                throw new DomainException("Requester has insufficient funds.");

            await _wallet.CreditAsync(
                session.ProfessionalUserId,
                session.AgreedRate,
                session.Id,
                "Verification session resolved",
                token);

            session.ExpertSolution = request.Solution.Trim();
            session.Outcome = SessionOutcome.Resolved;
            session.Status = SessionStatus.Closed;
            session.ClosedAt = DateTime.UtcNow;

            await _sessions.SaveChangesAsync(token);
        }, ct);

        return Map(session);
    }

    public async Task<VerificationSessionResponse> CancelSessionAsync(Guid id, CancellationToken ct = default)
    {
        EnsureAuthenticated();

        if (id == Guid.Empty)
            throw new ValidationException("Session id is required.");

        var session = await _sessions.GetByIdAsync(id, ct)
            ?? throw new NotFoundException("Verification session not found.");

        if (session.RequesterUserId != _currentUser.UserId)
            throw new ForbiddenException("Only the requester can cancel this session.");

        EnsureOpen(session);

        session.Outcome = SessionOutcome.Cancelled;
        session.Status = SessionStatus.Closed;
        session.ClosedAt = DateTime.UtcNow;

        await _sessions.SaveChangesAsync(ct);

        return Map(session);
    }

    public async Task<IReadOnlyList<VerificationSessionResponse>> GetMySessionsAsync(CancellationToken ct = default)
    {
        EnsureAuthenticated();

        var sessions = await _sessions.GetRequesterSessionsAsync(_currentUser.UserId, ct);
        return sessions.Select(Map).ToList();
    }

    public async Task<IReadOnlyList<VerificationSessionResponse>> GetIncomingSessionsAsync(CancellationToken ct = default)
    {
        EnsureAuthenticated();

        var sessions = await _sessions.GetOpenIncomingSessionsAsync(_currentUser.UserId, ct);
        return sessions.Select(Map).ToList();
    }

    public async Task<VerificationSessionResponse> GetSessionAsync(Guid id, CancellationToken ct = default)
    {
        EnsureAuthenticated();

        if (id == Guid.Empty)
            throw new ValidationException("Session id is required.");

        var session = await _sessions.GetByIdAsync(id, ct)
            ?? throw new NotFoundException("Verification session not found.");

        if (session.RequesterUserId != _currentUser.UserId && session.ProfessionalUserId != _currentUser.UserId)
            throw new ForbiddenException("You do not have access to this verification session.");

        return Map(session);
    }

    private void EnsureAuthenticated()
    {
        if (!_currentUser.IsAuthenticated || _currentUser.UserId == Guid.Empty)
            throw new UnauthorizedException("Authentication is required.");
    }

    private static void ValidateCreateRequest(CreateVerificationSessionRequest request)
    {
        if (request.ProfessionalUserId == Guid.Empty)
            throw new ValidationException("ProfessionalUserId is required.");

        if (request.CategoryId == Guid.Empty)
            throw new ValidationException("CategoryId is required.");

        if (string.IsNullOrWhiteSpace(request.QuestionText))
            throw new ValidationException("QuestionText is required.");

        if (string.IsNullOrWhiteSpace(request.AiAnswerText))
            throw new ValidationException("AiAnswerText is required.");
    }

    private void EnsureRequesterIsNotProfessional(CreateVerificationSessionRequest request)
    {
        if (request.ProfessionalUserId == _currentUser.UserId)
            throw new ValidationException("Requester cannot create a verification session with themselves.");
    }

    private static void EnsureOpen(VerificationSession session)
    {
        if (session.Status != SessionStatus.Open)
            throw new ConflictException("Verification session is already closed.");
    }

    private static VerificationSessionResponse Map(VerificationSession session)
    {
        return new VerificationSessionResponse(
            session.Id,
            session.RequesterUserId,
            session.ProfessionalUserId,
            session.CategoryId,
            session.SourceMessageId,
            session.QuestionSnapshot,
            session.AiAnswerSnapshot,
            session.AgreedRate,
            session.Status,
            session.Outcome,
            session.ExpertSolution,
            session.ClosedAt,
            session.CreatedAt);
    }
}
