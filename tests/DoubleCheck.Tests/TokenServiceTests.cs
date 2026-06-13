using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using DoubleCheck.Entities;
using DoubleCheck.Enums;
using DoubleCheck.Exceptions;
using DoubleCheck.Services;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace DoubleCheck.Tests;

public class TokenServiceTests
{
    private static IConfiguration Config(string? key) =>
        new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["Jwt:Key"] = key,
            ["Jwt:Issuer"] = "DoubleCheck",
            ["Jwt:Audience"] = "DoubleCheckClients",
            ["Jwt:ExpiryMinutes"] = "60"
        }).Build();

    [Fact]
    public void CreateToken_WithValidConfig_EmitsTokenWithUserAndRoleClaims()
    {
        // Arrange
        var sut = new JwtTokenService(Config("this-is-a-test-signing-key-32bytes-long!!"));
        var user = new ApplicationUser { Id = Guid.NewGuid(), Email = "a@b.com", DisplayName = "Amar" };

        // Act
        var result = sut.CreateToken(user, new[] { Roles.Common });

        // Assert
        Assert.False(string.IsNullOrWhiteSpace(result.AccessToken));
        Assert.True(result.ExpiresAt > DateTime.UtcNow);
        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(result.AccessToken);
        Assert.Contains(jwt.Claims, c => c.Type == ClaimTypes.NameIdentifier && c.Value == user.Id.ToString());
        Assert.Contains(jwt.Claims, c => c.Type == ClaimTypes.Role && c.Value == Roles.Common);
    }

    [Fact]
    public void CreateToken_WithMissingKey_ThrowsDomainException()
    {
        // Arrange
        var sut = new JwtTokenService(Config(null));
        var user = new ApplicationUser { Id = Guid.NewGuid(), Email = "a@b.com", DisplayName = "Amar" };

        // Act + Assert
        Assert.Throws<DomainException>(() => sut.CreateToken(user, new[] { Roles.Common }));
    }
}
