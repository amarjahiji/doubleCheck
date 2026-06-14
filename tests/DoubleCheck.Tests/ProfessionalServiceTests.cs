using DoubleCheck.Data;
using DoubleCheck.Dtos;
using DoubleCheck.Entities;
using DoubleCheck.Enums;
using DoubleCheck.Exceptions;
using DoubleCheck.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace DoubleCheck.Tests;

public class ProfessionalServiceTests
{
    private static AppDbContext NewDb() =>
        new(new DbContextOptionsBuilder<AppDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);

    [Fact]
    public async Task ApplyAsync_WithValidCategories_CreatesPendingApplication()
    {
        // Arrange
        await using var db = NewDb();
        var cat = new Category { Name = "Tech" };
        db.Categories.Add(cat);
        await db.SaveChangesAsync();
        var sut = new ProfessionalService(db);
        var userId = Guid.NewGuid();

        // Act
        var result = await sut.ApplyAsync(userId, new ApplyProfessionalRequest
        {
            RequestedRate = 20m, Bio = "expert", CategoryIds = new() { cat.Id }
        });

        // Assert
        Assert.Equal(ApplicationStatus.Pending.ToString(), result.Status);
        Assert.Contains(cat.Id, result.CategoryIds);
        Assert.Equal(1, await db.ProfessionalApplications.CountAsync());
    }

    [Fact]
    public async Task ApplyAsync_WhenPendingApplicationExists_ThrowsConflict()
    {
        // Arrange
        await using var db = NewDb();
        var cat = new Category { Name = "Tech" };
        db.Categories.Add(cat);
        var userId = Guid.NewGuid();
        db.ProfessionalApplications.Add(new ProfessionalApplication { UserId = userId, Status = ApplicationStatus.Pending });
        await db.SaveChangesAsync();
        var sut = new ProfessionalService(db);

        // Act + Assert
        await Assert.ThrowsAsync<ConflictException>(() => sut.ApplyAsync(userId, new ApplyProfessionalRequest
        {
            RequestedRate = 20m, CategoryIds = new() { cat.Id }
        }));
    }

    [Fact]
    public async Task ApplyAsync_WithUnknownCategory_ThrowsValidation()
    {
        // Arrange
        await using var db = NewDb();
        var sut = new ProfessionalService(db);

        // Act + Assert
        await Assert.ThrowsAsync<ValidationException>(() => sut.ApplyAsync(Guid.NewGuid(), new ApplyProfessionalRequest
        {
            RequestedRate = 20m, CategoryIds = new() { Guid.NewGuid() }
        }));
    }
}
