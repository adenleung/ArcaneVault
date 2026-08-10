/* Name: Aden Leung | Student Admin No.: 252744K | Tutorial Group: IT2814 */
using ArcaneVault.Web.Models;using ArcaneVault.Web.Services;using Microsoft.AspNetCore.Mvc;using Microsoft.AspNetCore.Mvc.RazorPages;
namespace ArcaneVault.Web.Pages.Categories;
public class CategoryDetailsModel(ApiClient api):PageModel{public CategoryDto? Category{get;set;}public async Task<IActionResult>OnGetAsync(string code){Category=await api.GetAsync<CategoryDto>($"api/categories/{code}");return Category is null?NotFound():Page();}}
