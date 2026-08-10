using System.Text.Json;
using ArcaneVault.Api.Data;
using ArcaneVault.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ArcaneVault.Api.Controllers;

public sealed record AssistantRequest(string Question);

[ApiController, Route("api/vaultassistant")]
public class VaultAssistantController(ArcaneVaultDbContext db, ApiTokenService tokens, OpenAiService ai)
    : ApiControllerBase(db, tokens)
{
    [HttpPost]
    public async Task<IActionResult> Ask(AssistantRequest request, CancellationToken cancellationToken)
    {
        if (RequireUser() is { } denied) return denied;
        if (IsStaff) return Forbid();
        var question = request.Question?.Trim() ?? string.Empty;
        if (question.Length is < 2 or > 500) return BadRequest(new { message = "Enter a question between 2 and 500 characters." });

        var items = await db.CollectionItems.AsNoTracking()
            .Include(x => x.CollectionItemCategories).ThenInclude(x => x.Category)
            .Where(x => !x.IsDeleted && x.UserName == CurrentUser)
            .OrderByDescending(x => x.DateAdded).ToListAsync(cancellationToken);
        var acquisitions = await db.AcquisitionRecords.AsNoTracking()
            .Where(x => x.UserName == CurrentUser).ToListAsync(cancellationToken);
        var context = JsonSerializer.Serialize(new
        {
            itemCount = items.Count,
            totalQuantity = items.Sum(x => x.CurrentQuantity),
            estimatedCollectionValue = items.Sum(x => x.CurrentQuantity * x.EstimatedUnitValue),
            recordedAcquisitionCost = acquisitions.Sum(x => x.Quantity * x.UnitPrice),
            categories = items.GroupBy(x => x.CollectionItemCategories.FirstOrDefault()?.Category?.CategoryName ?? "Uncategorised")
                .Select(x => new { name = x.Key, itemCount = x.Count(), estimatedValue = x.Sum(y => y.CurrentQuantity * y.EstimatedUnitValue) }),
            items = items.Take(100).Select(x => new { x.ItemName, x.CurrentQuantity, x.EstimatedUnitValue, x.DateAdded,
                category = x.CollectionItemCategories.FirstOrDefault()?.Category?.CategoryName ?? "Uncategorised" })
        });
        try
        {
            var answer = await ai.AnswerAsync(question, context, cancellationToken);
            return Ok(new { answer, generatedAt = DateTime.UtcNow });
        }
        catch (InvalidOperationException)
        {
            return Ok(new { answer = $"You have {items.Count} active collection items. AI explanation is temporarily unavailable, but your collection data is still safe and accessible.", generatedAt = DateTime.UtcNow });
        }
    }
}
