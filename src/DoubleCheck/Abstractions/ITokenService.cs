using DoubleCheck.Entities;

namespace DoubleCheck.Abstractions;

public record TokenResult(string AccessToken, DateTime ExpiresAt);

/// <summary>Issues signed JWT access tokens. Pure + easily unit-tested (Amar).</summary>
public interface ITokenService
{
    TokenResult CreateToken(ApplicationUser user, IEnumerable<string> roles);
}
