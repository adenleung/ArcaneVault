/* Name: Aden Leung | Student Admin No.: 252744K | Tutorial Group: IT2814 */
using System.ComponentModel.DataAnnotations;
using ArcaneVault.Web.Models;
using ArcaneVault.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ArcaneVault.Web.Pages.Account;
public class RegisterModel(ApiClient api) : PageModel
{
    [BindProperty] public RegisterInput Input { get; set; } = new();
    public class RegisterInput
    {
        [Required, StringLength(40, MinimumLength = 3), Display(Name = "Username")] public string UserName { get; set; } = "";
        [Required, EmailAddress] public string Email { get; set; } = "";
        [Required, MinLength(8), DataType(DataType.Password)] public string Password { get; set; } = "";
        [Required, Compare(nameof(Password)), DataType(DataType.Password), Display(Name = "Confirm password")] public string ConfirmPassword { get; set; } = "";
    }
    public void OnGet() { }
    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid) return Page();
        try { await api.PostAsync<LoginResponse>("api/accounts/register", new { Input.UserName, Input.Email, Input.Password }); TempData["Success"] = "Account created. You can now log in."; return RedirectToPage("Login"); }
        catch (ApiException ex) { ModelState.AddModelError("", ex.Message); return Page(); }
    }
}
