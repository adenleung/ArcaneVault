/* Name: Aden Leung | Student Admin No.: 252744K | Tutorial Group: IT2814 */
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using ArcaneVault.Web.Models;
using ArcaneVault.Web.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ArcaneVault.Web.Pages.Account;
public class LoginModel(ApiClient api) : PageModel
{
    [BindProperty] public LoginInput Input { get; set; } = new();
    public class LoginInput { [Required, EmailAddress] public string Email { get; set; } = ""; [Required, DataType(DataType.Password)] public string Password { get; set; } = ""; }
    public void OnGet() { }
    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid) return Page();
        try
        {
            var result = await api.PostAsync<LoginResponse>("api/accounts/login", Input);
            if (result is null) { ModelState.AddModelError("", "Login failed."); return Page(); }
            var claims = new[] { new Claim(ClaimTypes.Name, result.UserName), new Claim(ClaimTypes.Email, result.Email), new Claim(ClaimTypes.Role, result.RoleName), new Claim("ArcaneVaultApiToken", result.AccessToken) };
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme)));
            return RedirectToPage(result.RoleName == "Staff" ? "/Staff/Analytics" : "/Collection/Index");
        }
        catch (ApiException ex) { ModelState.AddModelError("", ex.Message); return Page(); }
    }
}
