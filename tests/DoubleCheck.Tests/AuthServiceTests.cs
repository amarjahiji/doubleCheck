using DoubleCheck.Abstractions;
using DoubleCheck.Dtos;
using DoubleCheck.Entities;
using DoubleCheck.Enums;
using DoubleCheck.Exceptions;
using DoubleCheck.Services;
using Microsoft.AspNetCore.Identity;
using NSubstitute;
using Xunit;

namespace DoubleCheck.Tests;

public class AuthServiceTests
{
    private static UserManager<ApplicationUser> MockUserManager()
    {
        var store = Substitute.For<IUserStore<ApplicationUser>>();
        return Substitute.For<UserManager<ApplicationUser>>(
            store, null, null, null, null, null, null, null, null);
    }

    private static ITokenService MockTokenService()
    {
        var token = Substitute.For<ITokenService>();
        token.CreateToken(Arg.Any<ApplicationUser>(), Arg.Any<IEnumerable<string>>())
             .Returns(new TokenResult("jwt-token", DateTime.UtcNow.AddHours(1)));
        return token;
    }

    [Fact]
    public async Task RegisterAsync_WithValidInput_CreatesUser_AssignsCommonRole_ReturnsToken()
    {
        // Arrange
        var users = MockUserManager();
        users.CreateAsync(Arg.Any<ApplicationUser>(), Arg.Any<string>()).Returns(IdentityResult.Success);
        users.AddToRoleAsync(Arg.Any<ApplicationUser>(), Roles.Common).Returns(IdentityResult.Success);
        var sut = new AuthService(users, MockTokenService());

        // Act
        var result = await sut.RegisterAsync(new RegisterRequest
        {
            Email = "new@user.com", Password = "Password1", DisplayName = "New User"
        });

        // Assert
        Assert.Equal("new@user.com", result.Email);
        Assert.Contains(Roles.Common, result.Roles);
        Assert.Equal("jwt-token", result.AccessToken);
        await users.Received(1).CreateAsync(Arg.Any<ApplicationUser>(), "Password1");
        await users.Received(1).AddToRoleAsync(Arg.Any<ApplicationUser>(), Roles.Common);
    }

    [Fact]
    public async Task RegisterAsync_WhenEmailAlreadyExists_ThrowsConflict()
    {
        // Arrange
        var users = MockUserManager();
        users.CreateAsync(Arg.Any<ApplicationUser>(), Arg.Any<string>())
             .Returns(IdentityResult.Failed(new IdentityError { Code = "DuplicateUserName", Description = "taken" }));
        var sut = new AuthService(users, MockTokenService());

        // Act + Assert
        await Assert.ThrowsAsync<ConflictException>(() => sut.RegisterAsync(new RegisterRequest
        {
            Email = "dupe@user.com", Password = "Password1", DisplayName = "Dupe"
        }));
    }

    [Fact]
    public async Task LoginAsync_WithValidCredentials_ReturnsToken()
    {
        // Arrange
        var user = new ApplicationUser { Id = Guid.NewGuid(), Email = "a@b.com", DisplayName = "Amar" };
        var users = MockUserManager();
        users.FindByEmailAsync("a@b.com").Returns(user);
        users.CheckPasswordAsync(user, "Password1").Returns(true);
        users.GetRolesAsync(user).Returns(new List<string> { Roles.Common });
        var sut = new AuthService(users, MockTokenService());

        // Act
        var result = await sut.LoginAsync(new LoginRequest { Email = "a@b.com", Password = "Password1" });

        // Assert
        Assert.Equal("jwt-token", result.AccessToken);
        Assert.Equal(user.Id, result.UserId);
    }

    [Fact]
    public async Task LoginAsync_WithWrongPassword_ThrowsUnauthorized()
    {
        // Arrange
        var user = new ApplicationUser { Id = Guid.NewGuid(), Email = "a@b.com", DisplayName = "Amar" };
        var users = MockUserManager();
        users.FindByEmailAsync("a@b.com").Returns(user);
        users.CheckPasswordAsync(user, Arg.Any<string>()).Returns(false);
        var sut = new AuthService(users, MockTokenService());

        // Act + Assert
        await Assert.ThrowsAsync<UnauthorizedException>(
            () => sut.LoginAsync(new LoginRequest { Email = "a@b.com", Password = "wrong" }));
    }
}
