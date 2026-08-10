/* Name: Aden Leung | Student Admin No.: 252744K | Tutorial Group: IT2814 */
using ArcaneVault.Web.Models;
using ArcaneVault.Web.Services;
using Microsoft.AspNetCore.Mvc.RazorPages;
namespace ArcaneVault.Web.Pages.Collection;
public class CollectionIndexModel(ApiClient api) : PageModel
{
    public List<ItemDto> Items { get; set; } = [];
    public List<CategoryDto> Categories { get; set; } = [];
    public string? Query { get; set; }
    public string? Category { get; set; }
    public string Sort { get; set; } = "recent";
    public string ViewMode { get; set; } = "grid";
    public int CategoryCount => Items.Select(x => x.CategoryCode).Distinct().Count();
    public int AddedThisMonth => Items.Count(x => x.DateAdded.Year == DateTime.UtcNow.Year && x.DateAdded.Month == DateTime.UtcNow.Month);
    public async Task OnGetAsync(string? query, string? category, string? sort, string? view)
    {
        Query = query; Category = category; Sort = string.IsNullOrWhiteSpace(sort) ? "recent" : sort;
        ViewMode = view == "list" ? "list" : "grid";
        var url = $"api/collectionitems?query={Uri.EscapeDataString(query ?? "")}&category={Uri.EscapeDataString(category ?? "")}";
        Items = await api.GetAsync<List<ItemDto>>(url) ?? [];
        Items = Sort switch
        {
            "name" => Items.OrderBy(x => x.ItemName).ToList(),
            "value-high" => Items.OrderByDescending(x => x.EstimatedUnitValue).ToList(),
            "value-low" => Items.OrderBy(x => x.EstimatedUnitValue).ToList(),
            "quantity" => Items.OrderByDescending(x => x.CurrentQuantity).ToList(),
            _ => Items.OrderByDescending(x => x.DateAdded).ToList()
        };
        Categories = await api.GetAsync<List<CategoryDto>>("api/categories") ?? [];
    }
}
