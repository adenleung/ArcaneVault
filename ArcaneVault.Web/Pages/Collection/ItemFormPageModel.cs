/* Name: Aden Leung | Student Admin No.: 252744K | Tutorial Group: IT2814 */
using ArcaneVault.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
namespace ArcaneVault.Web.Pages.Collection;
public abstract class ItemFormPageModel : PageModel
{
    [BindProperty] public ItemInput Input { get; set; } = new();
    public List<CategoryDto> Categories { get; set; } = [];
    public virtual bool IsEdit => false;
}
