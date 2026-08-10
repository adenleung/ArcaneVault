/* Name: Aden Leung | Student Admin No.: 252744K | Tutorial Group: IT2814 */
using ArcaneVault.Web.Models;
using ArcaneVault.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
namespace ArcaneVault.Web.Pages.Collection;
public class DeleteModel(ApiClient api) : PageModel { [BindProperty] public int Id { get; set; } public ItemDto? Item { get; set; } public async Task<IActionResult> OnGetAsync(int id) { Id=id; Item=await api.GetAsync<ItemDto>($"api/collectionitems/{id}"); return Item is null?NotFound():Page(); } public async Task<IActionResult> OnPostAsync(){ await api.DeleteAsync($"api/collectionitems/{Id}"); TempData["Success"]="Collectible removed from your active vault."; return RedirectToPage("Index"); } }
