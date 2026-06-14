using DoubleCheck.Abstractions;
using DoubleCheck.Dtos;
using DoubleCheck.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DoubleCheck.Controllers;

[ApiController]
[Route("api/professionals")]
public class ProfessionalsController : ControllerBase
{
    private readonly IProfessionalService _professionals;
    private readonly ICurrentUser _currentUser;

    public ProfessionalsController(IProfessionalService professionals, ICurrentUser currentUser)
    {
        _professionals = professionals;
        _currentUser = currentUser;
    }

    /// <summary>Apply to become a professional (any authenticated user).</summary>
    [HttpPost("applications")]
    [Authorize]
    public async Task<ActionResult<ProfessionalApplicationResponse>> Apply(ApplyProfessionalRequest request, CancellationToken ct)
    {
        var result = await _professionals.ApplyAsync(_currentUser.UserId, request, ct);
        return StatusCode(StatusCodes.Status201Created, result);
    }

    /// <summary>The caller's latest application + status.</summary>
    [HttpGet("applications/mine")]
    [Authorize]
    public async Task<ActionResult<ProfessionalApplicationResponse>> MyApplication(CancellationToken ct)
        => Ok(await _professionals.GetMyApplicationAsync(_currentUser.UserId, ct));

    /// <summary>Update the caller's professional profile.</summary>
    [HttpPut("me")]
    [Authorize(Roles = Roles.Professional)]
    public async Task<ActionResult<ProfessionalProfileResponse>> UpdateMe(UpdateProfessionalRequest request, CancellationToken ct)
        => Ok(await _professionals.UpdateMyProfileAsync(_currentUser.UserId, request, ct));

    /// <summary>Toggle the caller's availability.</summary>
    [HttpPut("me/availability")]
    [Authorize(Roles = Roles.Professional)]
    public async Task<ActionResult<ProfessionalProfileResponse>> SetAvailability(SetAvailabilityRequest request, CancellationToken ct)
        => Ok(await _professionals.SetAvailabilityAsync(_currentUser.UserId, request.IsAvailable, ct));

    /// <summary>Public professional profile.</summary>
    [HttpGet("{userId:guid}")]
    [AllowAnonymous]
    public async Task<ActionResult<ProfessionalProfileResponse>> Get(Guid userId, CancellationToken ct)
        => Ok(await _professionals.GetProfileAsync(userId, ct));
}
