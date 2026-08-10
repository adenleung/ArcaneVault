/* Name: Aden Leung | Student Admin No.: 252744K | Tutorial Group: IT2814 */
using ArcaneVault.Web.Models;
using ArcaneVault.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
namespace ArcaneVault.Web.Pages.Collection;
public class CreateModel(ApiClient api, IWebHostEnvironment environment) : ItemFormPageModel
{
    public async Task OnGetAsync() => Categories = await api.GetAsync<List<CategoryDto>>("api/categories") ?? [];
    public async Task<IActionResult> OnPostAsync()
    {
        Categories = await api.GetAsync<List<CategoryDto>>("api/categories") ?? [];
        if (!ModelState.IsValid) return Page();
        try { await api.PostAsync<ItemDto>("api/collectionitems", Input); TempData["Success"] = "Collectible added to your vault."; return RedirectToPage("Index"); }
        catch (ApiException ex) { ModelState.AddModelError("", ex.Message); return Page(); }
    }

    public async Task<IActionResult> OnPostIdentifyAsync(IFormFile image)
    {
        if (image is null || image.Length == 0) return new BadRequestObjectResult(new { message = "Choose an image first." });
        try
        {
            var result = await api.PostFileAsync<SmartAddResponse>("api/smartadd/identify", image);
            var extensions = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            { ["image/jpeg"] = ".jpg", ["image/png"] = ".png", ["image/webp"] = ".webp" };
            if (!extensions.TryGetValue(image.ContentType, out var extension))
                return new BadRequestObjectResult(new { message = "Use a JPEG, PNG or WebP image." });
            var uploadDirectory = Path.Combine(environment.WebRootPath, "uploads");
            Directory.CreateDirectory(uploadDirectory);
            var fileName = $"collectible-{Guid.NewGuid():N}{extension}";
            await using (var target = System.IO.File.Create(Path.Combine(uploadDirectory, fileName)))
                await image.CopyToAsync(target);
            return new JsonResult(new { result!.Identification, result.Matches, result.Disclaimer, uploadedImageUrl = $"/uploads/{fileName}" });
        }
        catch (ApiException ex) { return new ObjectResult(new { message = ex.Message }) { StatusCode = (int)ex.StatusCode }; }
    }

    public async Task<IActionResult> OnGetCatalogSearchAsync(string query)
    {
        var results = await api.GetAsync<List<CatalogMatchDto>>($"api/smartadd/search?query={Uri.EscapeDataString(query ?? string.Empty)}") ?? [];
        return new JsonResult(results);
    }
}
