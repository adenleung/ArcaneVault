/* Name: Aden Leung | Student Admin No.: 252744K | Tutorial Group: IT2814 */
using ArcaneVault.Web.Models; using ArcaneVault.Web.Services; using Microsoft.AspNetCore.Mvc.RazorPages;
namespace ArcaneVault.Web.Pages.Categories;
public class CategoriesIndexModel(ApiClient api):PageModel { public List<CategoryDto> Categories{get;set;}=[]; public async Task OnGetAsync()=>Categories=await api.GetAsync<List<CategoryDto>>("api/categories")??[]; }
