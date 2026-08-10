/* Name: Aden Leung | Student Admin No.: 252744K | Tutorial Group: IT2814 */
using ArcaneVault.Web.Models;
using ArcaneVault.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
namespace ArcaneVault.Web.Pages.Collection;
public class DetailsModel(ApiClient api) : PageModel { public ItemDto? Item { get; set; } public async Task<IActionResult> OnGetAsync(int id) { Item = await api.GetAsync<ItemDto>($"api/collectionitems/{id}"); return Item is null ? NotFound() : Page(); } }
