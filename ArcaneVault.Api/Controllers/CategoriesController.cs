/*
 * Name: Aden Leung
 * Student Admin No.: 252744K
 * Tutorial Group: IT2814
 */
using ArcaneVault.Api.Data;
using ArcaneVault.Api.DTOs;
using ArcaneVault.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ArcaneVault.Api.Controllers;

[ApiController, Route("api/categories")]
public class CategoriesController(ArcaneVaultDbContext db, ApiTokenService tokens) : ApiControllerBase(db, tokens)
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<CategoryDto>>> List()
    {
        if (RequireUser() is { } denied) return denied;
        var categories = await db.Categories.AsNoTracking()
            .Include(x => x.CollectionItemCategories).ThenInclude(x => x.Item)
            .OrderBy(x => x.CategoryName).ToListAsync();
        return Ok(categories.Select(x => new CategoryDto(x.CategoryCode, x.CategoryName,
            x.CollectionItemCategories.Count(y => y.Item is { IsDeleted: false }))));
    }

    [HttpGet("{code}")]
    public async Task<ActionResult<CategoryDto>> Details(string code)
    {
        if (RequireUser() is { } denied) return denied;
        var category = await db.Categories.AsNoTracking()
            .Include(x => x.CollectionItemCategories).ThenInclude(x => x.Item)
            .SingleOrDefaultAsync(x => x.CategoryCode == code);
        if (category is null) return NotFound();
        return new CategoryDto(category.CategoryCode, category.CategoryName,
            category.CollectionItemCategories.Count(y => y.Item is { IsDeleted: false }));
    }

    [HttpPost]
    public async Task<ActionResult<CategoryDto>> Create(CategoryRequest request)
    {
        if (RequireStaff() is { } denied) return denied;
        var code = request.CategoryCode.Trim().ToUpperInvariant();
        if (await db.Categories.AnyAsync(x => x.CategoryCode == code))
            return Conflict(new { message = "Category code already exists." });
        var category = new Category { CategoryCode = code, CategoryName = request.CategoryName.Trim() };
        db.Add(category); await db.SaveChangesAsync();
        return CreatedAtAction(nameof(Details), new { code }, new CategoryDto(code, category.CategoryName, 0));
    }

    [HttpPut("{code}")]
    public async Task<IActionResult> Update(string code, CategoryRequest request)
    {
        if (RequireStaff() is { } denied) return denied;
        var category = await db.Categories.FindAsync(code);
        if (category is null) return NotFound();
        category.CategoryName = request.CategoryName.Trim();
        await db.SaveChangesAsync(); return NoContent();
    }

    [HttpDelete("{code}")]
    public async Task<IActionResult> Delete(string code)
    {
        if (RequireStaff() is { } denied) return denied;
        var category = await db.Categories
            .Include(x => x.CollectionItemCategories).ThenInclude(x => x.Item)
            .SingleOrDefaultAsync(x => x.CategoryCode == code);
        if (category is null) return NotFound();
        if (category.CollectionItemCategories.Any(x => x.Item is { IsDeleted: false }))
            return Conflict(new { message = "This category is used by active collection items and cannot be deleted." });

        // Historical links to soft-deleted items should not prevent removal of an
        // otherwise unused category, but they must be removed before the category.
        db.RemoveRange(category.CollectionItemCategories);
        db.Remove(category); await db.SaveChangesAsync(); return NoContent();
    }
}