using ArcaneVault.Web.Models;
using ArcaneVault.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ArcaneVault.Web.Pages;

[Authorize(Roles = "User")]
public class AssistantModel(ApiClient api) : PageModel
{
    public IActionResult OnGet() => NotFound();

    public async Task<IActionResult> OnPostAskAsync([FromBody] AssistantQuestion request)
    {
        if (string.IsNullOrWhiteSpace(request.Question)) return BadRequest(new { message = "Enter a question." });
        try
        {
            var response = await api.PostAsync<AssistantResponse>("api/vaultassistant", new { request.Question });
            return new JsonResult(response);
        }
        catch (ApiException ex) { return new ObjectResult(new { message = ex.Message }) { StatusCode = (int)ex.StatusCode }; }
    }
}

public sealed record AssistantQuestion(string Question);
