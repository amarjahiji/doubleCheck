using DoubleCheck.Abstractions;
using DoubleCheck.Data;
using DoubleCheck.Dtos;
using DoubleCheck.Entities;
using DoubleCheck.Exceptions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace DoubleCheck.Services;

/// <summary>
/// Category CRUD. The list is read constantly and changes rarely, so GetAll is cached
/// in IMemoryCache and the cache is invalidated on every create/update/delete.
/// </summary>
public class CategoryService : ICategoryService
{
    private const string CacheKey = "categories:all";

    private readonly AppDbContext _db;
    private readonly IMemoryCache _cache;

    public CategoryService(AppDbContext db, IMemoryCache cache)
    {
        _db = db;
        _cache = cache;
    }

    public async Task<IReadOnlyList<CategoryResponse>> GetAllAsync(CancellationToken ct = default)
    {
        if (_cache.TryGetValue(CacheKey, out IReadOnlyList<CategoryResponse>? cached) && cached is not null)
            return cached;

        var list = await _db.Categories
            .OrderBy(c => c.Name)
            .Select(c => new CategoryResponse(c.Id, c.Name, c.Description))
            .ToListAsync(ct);

        _cache.Set(CacheKey, (IReadOnlyList<CategoryResponse>)list);
        return list;
    }

    public async Task<CategoryResponse> CreateAsync(CreateCategoryRequest request, CancellationToken ct = default)
    {
        if (await _db.Categories.AnyAsync(c => c.Name == request.Name, ct))
            throw new ConflictException($"A category named '{request.Name}' already exists.");

        var category = new Category { Name = request.Name, Description = request.Description };
        _db.Categories.Add(category);
        await _db.SaveChangesAsync(ct);
        Invalidate();

        return new CategoryResponse(category.Id, category.Name, category.Description);
    }

    public async Task<CategoryResponse> UpdateAsync(Guid id, UpdateCategoryRequest request, CancellationToken ct = default)
    {
        var category = await _db.Categories.FirstOrDefaultAsync(c => c.Id == id, ct)
            ?? throw new NotFoundException("Category not found.");

        if (await _db.Categories.AnyAsync(c => c.Name == request.Name && c.Id != id, ct))
            throw new ConflictException($"A category named '{request.Name}' already exists.");

        category.Name = request.Name;
        category.Description = request.Description;
        await _db.SaveChangesAsync(ct);
        Invalidate();

        return new CategoryResponse(category.Id, category.Name, category.Description);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var category = await _db.Categories.FirstOrDefaultAsync(c => c.Id == id, ct)
            ?? throw new NotFoundException("Category not found.");

        _db.Categories.Remove(category);
        await _db.SaveChangesAsync(ct);
        Invalidate();
    }

    private void Invalidate() => _cache.Remove(CacheKey);
}
