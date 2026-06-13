using DoubleCheck.Abstractions;
using DoubleCheck.Dtos;
using DoubleCheck.Entities;
using DoubleCheck.Enums;
using DoubleCheck.Exceptions;
using Microsoft.AspNetCore.Identity;

namespace DoubleCheck.Services;

/// <summary>Auth orchestration over ASP.NET Identity. New users get the Common role + a starter balance.</summary>
public class AuthService : IAuthService
{
    private const decimal StartingBalance = 100m;

    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ITokenService _tokenService;

    public AuthService(UserManager<ApplicationUser> userManager, ITokenService tokenService)
    {
        _userManager = userManager;
        _tokenService = tokenService;
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request, CancellationToken ct = default)
    {
        var user = new ApplicationUser
        {
            UserName = request.Email,
            Email = request.Email,
            DisplayName = request.DisplayName,
            EmailConfirmed = true,
            Balance = StartingBalance
        };

        var created = await _userManager.CreateAsync(user, request.Password);
        if (!created.Succeeded)
        {
            if (created.Errors.Any(e => e.Code.Contains("Duplicate", StringComparison.OrdinalIgnoreCase)))
                throw new ConflictException("A user with this email already exists.");
            throw new ValidationException(string.Join("; ", created.Errors.Select(e => e.Description)));
        }

        await _userManager.AddToRoleAsync(user, Roles.Common);
        var roles = new[] { Roles.Common };
        var token = _tokenService.CreateToken(user, roles);
        return new AuthResponse(token.AccessToken, token.ExpiresAt, user.Id, user.Email!, user.DisplayName, roles);
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken ct = default)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user is null || !await _userManager.CheckPasswordAsync(user, request.Password))
            throw new UnauthorizedException("Invalid email or password.");

        var roles = await _userManager.GetRolesAsync(user);
        var token = _tokenService.CreateToken(user, roles);
        return new AuthResponse(token.AccessToken, token.ExpiresAt, user.Id, user.Email!, user.DisplayName, roles.ToArray());
    }

    public async Task<MeResponse> GetMeAsync(Guid userId, CancellationToken ct = default)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString())
            ?? throw new NotFoundException("User not found.");
        var roles = await _userManager.GetRolesAsync(user);
        return new MeResponse(user.Id, user.Email!, user.DisplayName, user.Balance, roles.ToArray());
    }
}
