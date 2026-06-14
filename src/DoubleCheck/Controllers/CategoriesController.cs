using DoubleCheck.Abstractions;
using DoubleCheck.Dtos;
using DoubleCheck.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DoubleCheck.Controllers;

[ApiController]
[Route("api/categories")]
public class CategoriesController : ControllerBase
{
    private readonly ICategoryService _categories;
    public CategoriesController(ICategoryService categories) => _categories = categories;

    /// <summary>List all categories (cached).</summary>
    [HttpGet]
    [Authorize]
    public async Task<ActionResult<IReadOnlyList<CategoryResponse>>> GetAll(CancellationToken ct)
        => Ok(await _categories.GetAllAsync(ct));

    /// <summary>Create a category (admin only).</summary>
    [HttpPost]
    [Authorize(Roles = Roles.Admin)]
    public async Task<ActionResult<CategoryResponse>> Create(CreateCategoryRequest request, CancellationToken ct)
    {
        var result = await _categories.CreateAsync(request, ct);
        return StatusCode(StatusCodes.Status201Created, result);
    }

    /// <summary>Update a category (admin only).</summary>
    [HttpPut("{id:guid}")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<ActionResult<CategoryResponse>> Update(Guid id, UpdateCategoryRequest request, CancellationToken ct)
        => Ok(await _categories.UpdateAsync(id, request, ct));

    /// <summary>Delete a category (admin only).</summary>
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await _categories.DeleteAsync(id, ct);
        return NoContent();
    }
}
