using DoubleCheck.Abstractions;
using DoubleCheck.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DoubleCheck.Controllers;

[ApiController]
[Route("api/account")]
[Authorize]
public class AccountController : ControllerBase
{
    private readonly IAccountService _account;
    private readonly ICurrentUser _currentUser;

    public AccountController(IAccountService account, ICurrentUser currentUser)
    {
        _account = account;
        _currentUser = currentUser;
    }

    /// <summary>Current wallet balance + recent transactions.</summary>
    [HttpGet("balance")]
    public async Task<ActionResult<BalanceResponse>> Balance(CancellationToken ct)
        => Ok(await _account.GetBalanceAsync(_currentUser.UserId, ct));
}
