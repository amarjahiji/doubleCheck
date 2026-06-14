using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using DoubleCheck.Abstractions;
using DoubleCheck.Entities;
using DoubleCheck.Exceptions;
using Microsoft.IdentityModel.Tokens;

namespace DoubleCheck.Services;

/// <summary>Builds signed JWTs from configuration (Jwt:Key/Issuer/Audience/ExpiryMinutes).</summary>
public class JwtTokenService : ITokenService
{
    private readonly IConfiguration _config;
    public JwtTokenService(IConfiguration config) => _config = config;

    public TokenResult CreateToken(ApplicationUser user, IEnumerable<string> roles)
    {
        var key = _config["Jwt:Key"];
        if (string.IsNullOrWhiteSpace(key) || Encoding.UTF8.GetByteCount(key) < 32)
            throw new DomainException("JWT signing key is missing or too short (need >= 32 bytes).");

        var issuer = _config["Jwt:Issuer"] ?? "DoubleCheck";
        var audience = _config["Jwt:Audience"] ?? "DoubleCheckClients";
        var minutes = int.TryParse(_config["Jwt:ExpiryMinutes"], out var m) ? m : 120;
        var expiresAt = DateTime.UtcNow.AddMinutes(minutes);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Email, user.Email ?? string.Empty),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new("displayName", user.DisplayName)
        };
        claims.AddRange(roles.Select(r => new Claim(ClaimTypes.Role, r)));

        var credentials = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
            SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(issuer, audience, claims, expires: expiresAt, signingCredentials: credentials);
        var jwt = new JwtSecurityTokenHandler().WriteToken(token);
        return new TokenResult(jwt, expiresAt);
    }
}
