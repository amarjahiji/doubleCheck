using DoubleCheck.Data;
using DoubleCheck.Dtos;
using DoubleCheck.Entities;
using DoubleCheck.Exceptions;
using DoubleCheck.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Xunit;

namespace DoubleCheck.Tests;

public class CategoryServiceTests
{
    private static AppDbContext NewDb() =>
        new(new DbContextOptionsBuilder<AppDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);

    private static IMemoryCache NewCache() => new MemoryCache(new MemoryCacheOptions());

    [Fact]
    public async Task GetAllAsync_SecondCall_ServesFromCache_IgnoringDirectDbInsert()
    {
        // Arrange
        await using var db = NewDb();
        db.Categories.Add(new Category { Name = "Tech" });
        await db.SaveChangesAsync();
        var sut = new CategoryService(db, NewCache());

        // Act
        var first = await sut.GetAllAsync();
        // sneak a row directly into the DB (bypassing the service, so cache is stale on purpose)
        db.Categories.Add(new Category { Name = "Food" });
        await db.SaveChangesAsync();
        var second = await sut.GetAllAsync();

        // Assert — second call returns the cached snapshot (1), proving caching
        Assert.Single(first);
        Assert.Single(second);
    }

    [Fact]
    public async Task CreateAsync_BustsCache_SoNextGetAllSeesNewCategory()
    {
        // Arrange
        await using var db = NewDb();
        db.Categories.Add(new Category { Name = "Tech" });
        await db.SaveChangesAsync();
        var sut = new CategoryService(db, NewCache());
        await sut.GetAllAsync(); // prime the cache (count = 1)

        // Act
        await sut.CreateAsync(new CreateCategoryRequest { Name = "Food" });
        var after = await sut.GetAllAsync();

        // Assert — cache was invalidated, so the new category appears
        Assert.Equal(2, after.Count);
    }

    [Fact]
    public async Task CreateAsync_WithDuplicateName_ThrowsConflict()
    {
        // Arrange
        await using var db = NewDb();
        db.Categories.Add(new Category { Name = "Tech" });
        await db.SaveChangesAsync();
        var sut = new CategoryService(db, NewCache());

        // Act + Assert
        await Assert.ThrowsAsync<ConflictException>(
            () => sut.CreateAsync(new CreateCategoryRequest { Name = "Tech" }));
    }

    [Fact]
    public async Task UpdateAsync_UnknownId_ThrowsNotFound()
    {
        // Arrange
        await using var db = NewDb();
        var sut = new CategoryService(db, NewCache());

        // Act + Assert
        await Assert.ThrowsAsync<NotFoundException>(
            () => sut.UpdateAsync(Guid.NewGuid(), new UpdateCategoryRequest { Name = "X" }));
    }
}
