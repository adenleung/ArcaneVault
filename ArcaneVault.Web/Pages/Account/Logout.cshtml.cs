/* Name: Aden Leung | Student Admin No.: 252744K | Tutorial Group: IT2814 */
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
namespace ArcaneVault.Web.Pages.Account;
public class LogoutModel : PageModel { public async Task<IActionResult> OnPostAsync() { await HttpContext.SignOutAsync(); return RedirectToPage("/Index"); } }
