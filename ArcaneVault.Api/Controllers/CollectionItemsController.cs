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

[ApiController, Route("api/collectionitems")]
public class CollectionItemsController(ArcaneVaultDbContext db, ApiTokenService tokens) : ApiControllerBase(db, tokens)
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ItemDto>>> List([FromQuery] string? query, [FromQuery] string? category)
    {
        if (RequireUser() is { } denied) return denied;
        // Materialise the small prototype collection before the cross-field search.
        // This avoids provider-specific SQLite translation failures for numeric ToString searches.
        var items = await db.CollectionItems.AsNoTracking()
            .Include(x => x.CollectionItemCategories).ThenInclude(x => x.Category)
            .Where(x => !x.IsDeleted && (IsStaff || x.UserName == CurrentUser))
            .OrderByDescending(x => x.DateAdded)
            .ToListAsync();
        if (!string.IsNullOrWhiteSpace(category))
            items = items.Where(x => x.CollectionItemCategories.Any(y => y.CategoryCode == category)).ToList();
        if (!string.IsNullOrWhiteSpace(query))
        {
            var q = query.Trim().ToLower();
            items = items.Where(x => x.ItemName.ToLower().Contains(q) || x.Description.ToLower().Contains(q)
                || x.UserName.ToLower().Contains(q) || x.CurrentQuantity.ToString().Contains(q)
                || x.StartingQuantity.ToString().Contains(q) || x.EstimatedUnitValue.ToString().Contains(q)
                || x.CollectionItemCategories.Any(c => c.CategoryCode.ToLower().Contains(q) || (c.Category?.CategoryName ?? "").ToLower().Contains(q))).ToList();
        }
        return Ok(items.Select(ToDto));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ItemDto>> Details(int id)
    {
        if (RequireUser() is { } denied) return denied;
        var item = await db.CollectionItems.AsNoTracking()
            .Include(x => x.CollectionItemCategories).ThenInclude(x => x.Category)
            .SingleOrDefaultAsync(x => x.ItemId == id && !x.IsDeleted && (IsStaff || x.UserName == CurrentUser));
        if (item is null) return NotFound();
        return ToDto(item);
    }

    [HttpPost]
    public async Task<ActionResult<ItemDto>> Create(CollectionItemRequest request)
    {
        if (RequireUser() is { } denied) return denied;
        if (!await db.Categories.AnyAsync(x => x.CategoryCode == request.CategoryCode))
            return BadRequest(new { message = "Choose a valid category." });
        await using var transaction = await db.Database.BeginTransactionAsync();
        var item = new CollectionItem
        {
            ItemName = request.ItemName.Trim(), Description = request.Description.Trim(),
            StartingQuantity = request.StartingQuantity, CurrentQuantity = request.CurrentQuantity,
            EstimatedUnitValue = request.EstimatedUnitValue, ImageUrl = NormalizeImage(request.ImageUrl), UserName = CurrentUser
        };
        db.CollectionItems.Add(item); await db.SaveChangesAsync();
        db.CollectionItemCategories.Add(new CollectionItemCategory { ItemId = item.ItemId, CategoryCode = request.CategoryCode });
        if (request.StartingQuantity > 0)
            db.AcquisitionRecords.Add(new AcquisitionRecord
            {
                ItemId = item.ItemId, UserName = CurrentUser, Quantity = request.StartingQuantity,
                UnitPrice = request.PurchasePrice, PurchaseDate = request.PurchaseDate,
                PurchaseSource = request.PurchaseSource, Condition = request.Condition
            });
        await db.SaveChangesAsync();
        await transaction.CommitAsync();
        var created = await db.CollectionItems.AsNoTracking().Include(x => x.CollectionItemCategories).ThenInclude(x => x.Category)
            .SingleAsync(x => x.ItemId == item.ItemId);
        return CreatedAtAction(nameof(Details), new { id = item.ItemId }, ToDto(created));
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, CollectionItemRequest request)
    {
        if (RequireUser() is { } denied) return denied;
        var item = await db.CollectionItems.Include(x => x.CollectionItemCategories).SingleOrDefaultAsync(x => x.ItemId == id && !x.IsDeleted);
        if (item is null) return NotFound();
        if (!IsStaff && item.UserName != CurrentUser) return Forbid();
        if (!await db.Categories.AnyAsync(x => x.CategoryCode == request.CategoryCode))
            return BadRequest(new { message = "Choose a valid category." });
        item.ItemName = request.ItemName.Trim(); item.Description = request.Description.Trim();
        item.StartingQuantity = request.StartingQuantity; item.CurrentQuantity = request.CurrentQuantity;
        item.EstimatedUnitValue = request.EstimatedUnitValue; item.ImageUrl = NormalizeImage(request.ImageUrl);
        var existingCategory = item.CollectionItemCategories.FirstOrDefault();
        if (existingCategory?.CategoryCode != request.CategoryCode)
        {
            if (existingCategory is not null) db.CollectionItemCategories.Remove(existingCategory);
            db.CollectionItemCategories.Add(new CollectionItemCategory { ItemId = id, CategoryCode = request.CategoryCode });
        }
        await db.SaveChangesAsync(); return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        if (RequireUser() is { } denied) return denied;
        var item = await db.CollectionItems.FindAsync(id);
        if (item is null || item.IsDeleted) return NotFound();
        if (!IsStaff && item.UserName != CurrentUser) return Forbid();
        item.IsDeleted = true; await db.SaveChangesAsync(); return NoContent();
    }

    private static ItemDto ToDto(CollectionItem item)
    {
        var category = item.CollectionItemCategories.FirstOrDefault();
        return new ItemDto(item.ItemId, item.ItemName, item.Description, item.StartingQuantity,
            item.CurrentQuantity, item.EstimatedUnitValue, item.DateAdded, item.ImageUrl, item.UserName,
            category?.CategoryCode ?? "", category?.Category?.CategoryName ?? "Uncategorised");
    }
    private static string NormalizeImage(string? value) => string.IsNullOrWhiteSpace(value) ? "/images/products/placeholder.webp" : value.Trim();
}
