using DoubleCheck.Data;
using DoubleCheck.Entities;
using DoubleCheck.Enums;
using DoubleCheck.Exceptions;
using DoubleCheck.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using Xunit;

namespace DoubleCheck.Tests;

public class AdminServiceTests
{
    private static AppDbContext NewDb() =>
        new(new DbContextOptionsBuilder<AppDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);

    private static UserManager<ApplicationUser> MockUserManager()
    {
        var store = Substitute.For<IUserStore<ApplicationUser>>();
        return Substitute.For<UserManager<ApplicationUser>>(store, null, null, null, null, null, null, null, null);
    }

    [Fact]
    public async Task ApproveApplicationAsync_CreatesProfile_AssignsProfessionalRole_MarksApproved()
    {
        // Arrange
        await using var db = NewDb();
        var userId = Guid.NewGuid();
        var catId = Guid.NewGuid();
        var app = new ProfessionalApplication
        {
            UserId = userId, RequestedRate = 15m, Status = ApplicationStatus.Pending,
            Categories = new List<ProfessionalApplicationCategory> { new() { CategoryId = catId } }
        };
        db.ProfessionalApplications.Add(app);
        await db.SaveChangesAsync();

        var users = MockUserManager();
        var applicant = new ApplicationUser { Id = userId, DisplayName = "Pro" };
        users.FindByIdAsync(userId.ToString()).Returns(applicant);
        users.AddToRoleAsync(applicant, Roles.Professional).Returns(IdentityResult.Success);
        var sut = new AdminService(db, users);

        // Act
        var result = await sut.ApproveApplicationAsync(app.Id);

        // Assert
        Assert.Equal(15m, result.RatePerAnswer);
        Assert.True(result.IsAvailable);
        Assert.Equal(ApplicationStatus.Approved, (await db.ProfessionalApplications.FindAsync(app.Id))!.Status);
        Assert.Equal(1, await db.ProfessionalProfiles.CountAsync());
        await users.Received(1).AddToRoleAsync(applicant, Roles.Professional);
    }

    [Fact]
    public async Task ApproveApplicationAsync_WhenNotPending_ThrowsConflict()
    {
        // Arrange
        await using var db = NewDb();
        var app = new ProfessionalApplication { UserId = Guid.NewGuid(), Status = ApplicationStatus.Approved };
        db.ProfessionalApplications.Add(app);
        await db.SaveChangesAsync();
        var sut = new AdminService(db, MockUserManager());

        // Act + Assert
        await Assert.ThrowsAsync<ConflictException>(() => sut.ApproveApplicationAsync(app.Id));
    }

    [Fact]
    public async Task AssignRoleAsync_WithUnknownRole_ThrowsValidation()
    {
        // Arrange
        await using var db = NewDb();
        var sut = new AdminService(db, MockUserManager());

        // Act + Assert
        await Assert.ThrowsAsync<ValidationException>(() => sut.AssignRoleAsync(Guid.NewGuid(), "Wizard"));
    }
}
