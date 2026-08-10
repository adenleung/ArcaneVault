/* Name: Aden Leung | Student Admin No.: 252744K | Tutorial Group: IT2814 */
using ArcaneVault.Web.Models;
using ArcaneVault.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
namespace ArcaneVault.Web.Pages.Collection;
public class EditModel(ApiClient api) : ItemFormPageModel
{
    public override bool IsEdit => true;
    [BindProperty] public int Id { get; set; }
    public async Task<IActionResult> OnGetAsync(int id)
    {
        Categories = await api.GetAsync<List<CategoryDto>>("api/categories") ?? [];
        var item = await api.GetAsync<ItemDto>($"api/collectionitems/{id}"); if (item is null) return NotFound();
        Id = id; Input = new ItemInput { ItemName = item.ItemName, Description = item.Description, StartingQuantity = item.StartingQuantity, CurrentQuantity = item.CurrentQuantity, EstimatedUnitValue = item.EstimatedUnitValue, CategoryCode = item.CategoryCode, ImageUrl = item.ImageUrl };
        return Page();
    }
    public async Task<IActionResult> OnPostAsync()
    {
        Categories = await api.GetAsync<List<CategoryDto>>("api/categories") ?? [];
        if (!ModelState.IsValid) return Page();
        try { await api.PutAsync($"api/collectionitems/{Id}", Input); TempData["Success"] = "Collection record updated."; return RedirectToPage("Details", new { id = Id }); }
        catch (ApiException ex) { ModelState.AddModelError("", ex.Message); return Page(); }
    }
}
