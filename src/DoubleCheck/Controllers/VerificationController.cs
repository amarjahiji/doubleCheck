using DoubleCheck.Dtos;
using DoubleCheck.Enums;
using DoubleCheck.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DoubleCheck.Controllers;

/// <summary>Exposes verification expert matching and session workflow endpoints.</summary>
[ApiController]
[Authorize]
[Route("api/verification")]
public class VerificationController : ControllerBase
{
    private readonly IExpertMatchingService _expertMatching;
    private readonly IVerificationService _verification;

    /// <summary>Creates a verification controller with expert matching and session services.</summary>
    public VerificationController(IExpertMatchingService expertMatching, IVerificationService verification)
    {
        _expertMatching = expertMatching;
        _verification = verification;
    }

    /// <summary>Lists available experts for a category.</summary>
    [HttpGet("experts")]
    public async Task<ActionResult<IReadOnlyList<ExpertMatchResponse>>> GetExperts(
        [FromQuery] Guid categoryId,
        CancellationToken ct)
    {
        var experts = await _expertMatching.GetMatchingExpertsAsync(categoryId, ct);
        return Ok(experts);
    }

    /// <summary>Creates a new verification session for the current requester.</summary>
    [HttpPost("sessions")]
    [Authorize(Roles = Roles.Common)]
    public async Task<ActionResult<VerificationSessionResponse>> CreateSession(
        CreateVerificationSessionRequest request,
        CancellationToken ct)
    {
        var session = await _verification.CreateSessionAsync(request, ct);
        return CreatedAtAction(nameof(GetSession), new { id = session.Id }, session);
    }

    /// <summary>Lists verification sessions created by the current requester.</summary>
    [HttpGet("sessions/mine")]
    public async Task<ActionResult<IReadOnlyList<VerificationSessionResponse>>> GetMySessions(CancellationToken ct)
    {
        var sessions = await _verification.GetMySessionsAsync(ct);
        return Ok(sessions);
    }

    /// <summary>Lists open verification sessions assigned to the current professional.</summary>
    [HttpGet("sessions/incoming")]
    [Authorize(Roles = Roles.Professional)]
    public async Task<ActionResult<IReadOnlyList<VerificationSessionResponse>>> GetIncomingSessions(CancellationToken ct)
    {
        var sessions = await _verification.GetIncomingSessionsAsync(ct);
        return Ok(sessions);
    }

    /// <summary>Gets a verification session visible to the current requester or assigned professional.</summary>
    [HttpGet("sessions/{id:guid}")]
    public async Task<ActionResult<VerificationSessionResponse>> GetSession(Guid id, CancellationToken ct)
    {
        var session = await _verification.GetSessionAsync(id, ct);
        return Ok(session);
    }

    /// <summary>Resolves an assigned verification session as a professional.</summary>
    [HttpPost("sessions/{id:guid}/resolve")]
    [Authorize(Roles = Roles.Professional)]
    public async Task<ActionResult<VerificationSessionResponse>> ResolveSession(
        Guid id,
        ResolveVerificationSessionRequest request,
        CancellationToken ct)
    {
        var session = await _verification.ResolveSessionAsync(id, request, ct);
        return Ok(session);
    }

    /// <summary>Cancels an open verification session as the requester.</summary>
    [HttpPost("sessions/{id:guid}/cancel")]
    public async Task<ActionResult<VerificationSessionResponse>> CancelSession(Guid id, CancellationToken ct)
    {
        var session = await _verification.CancelSessionAsync(id, ct);
        return Ok(session);
    }
}
