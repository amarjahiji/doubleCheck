using DoubleCheck.Dtos;
using DoubleCheck.Enums;
using DoubleCheck.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DoubleCheck.Controllers;

[ApiController]
[Authorize]
[Route("api/verification")]
public class VerificationController : ControllerBase
{
    private readonly IExpertMatchingService _expertMatching;
    private readonly IVerificationService _verification;

    public VerificationController(IExpertMatchingService expertMatching, IVerificationService verification)
    {
        _expertMatching = expertMatching;
        _verification = verification;
    }

    [HttpGet("experts")]
    public async Task<ActionResult<IReadOnlyList<ExpertMatchResponse>>> GetExperts(
        [FromQuery] Guid categoryId,
        CancellationToken ct)
    {
        var experts = await _expertMatching.GetMatchingExpertsAsync(categoryId, ct);
        return Ok(experts);
    }

    [HttpPost("sessions")]
    [Authorize(Roles = Roles.Common)]
    public async Task<ActionResult<VerificationSessionResponse>> CreateSession(
        CreateVerificationSessionRequest request,
        CancellationToken ct)
    {
        var session = await _verification.CreateSessionAsync(request, ct);
        return CreatedAtAction(nameof(GetSession), new { id = session.Id }, session);
    }

    [HttpGet("sessions/mine")]
    public async Task<ActionResult<IReadOnlyList<VerificationSessionResponse>>> GetMySessions(CancellationToken ct)
    {
        var sessions = await _verification.GetMySessionsAsync(ct);
        return Ok(sessions);
    }

    [HttpGet("sessions/incoming")]
    [Authorize(Roles = Roles.Professional)]
    public async Task<ActionResult<IReadOnlyList<VerificationSessionResponse>>> GetIncomingSessions(CancellationToken ct)
    {
        var sessions = await _verification.GetIncomingSessionsAsync(ct);
        return Ok(sessions);
    }

    [HttpGet("sessions/{id:guid}")]
    public async Task<ActionResult<VerificationSessionResponse>> GetSession(Guid id, CancellationToken ct)
    {
        var session = await _verification.GetSessionAsync(id, ct);
        return Ok(session);
    }

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

    [HttpPost("sessions/{id:guid}/cancel")]
    public async Task<ActionResult<VerificationSessionResponse>> CancelSession(Guid id, CancellationToken ct)
    {
        var session = await _verification.CancelSessionAsync(id, ct);
        return Ok(session);
    }
}
