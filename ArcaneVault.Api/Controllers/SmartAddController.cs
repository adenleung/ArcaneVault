using ArcaneVault.Api.Data;
using ArcaneVault.Api.Models;
using ArcaneVault.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ArcaneVault.Api.Controllers;

[ApiController, Route("api/smartadd")]
public class SmartAddController(ArcaneVaultDbContext db, ApiTokenService tokens, OpenAiService ai)
    : ApiControllerBase(db, tokens)
{
    [HttpGet("search")]
    public async Task<IActionResult> Search([FromQuery] string query)
    {
        if (RequireUser() is { } denied) return denied;
        if (string.IsNullOrWhiteSpace(query)) return Ok(Array.Empty<object>());
        var catalog = await db.CollectibleCatalog.AsNoTracking().ToListAsync();
        return Ok(Rank(catalog, query).Take(5).Select(ToResult));
    }

    [HttpPost("identify"), RequestSizeLimit(5 * 1024 * 1024)]
    public async Task<IActionResult> Identify(IFormFile image, CancellationToken cancellationToken)
    {
        if (RequireUser() is { } denied) return denied;
        if (image is null || image.Length == 0) return BadRequest(new { message = "Choose an image first." });
        if (image.Length > 5 * 1024 * 1024) return BadRequest(new { message = "The image must be 5 MB or smaller." });
        var allowed = new[] { "image/jpeg", "image/png", "image/webp" };
        if (!allowed.Contains(image.ContentType, StringComparer.OrdinalIgnoreCase))
            return BadRequest(new { message = "Use a JPEG, PNG or WebP image." });
        try
        {
            var identification = await ai.IdentifyAsync(image, cancellationToken);
            var query = string.Join(' ', new[] { identification.PossibleName, identification.Brand,
                identification.Series, identification.ReferenceNumber }.Where(x => !string.IsNullOrWhiteSpace(x)));
            var catalog = await db.CollectibleCatalog.AsNoTracking().ToListAsync(cancellationToken);
            var matches = Rank(catalog, query).Take(3).Select(ToResult).ToArray();
            return Ok(new { identification, matches, disclaimer = "Suggested identification only. Review the details before saving." });
        }
        catch (InvalidOperationException ex) { return StatusCode(503, new { message = ex.Message }); }
    }

    private static IEnumerable<CollectibleCatalog> Rank(IEnumerable<CollectibleCatalog> source, string query)
    {
        var terms = Terms(query);
        return source.Select(item => new { Item = item, Score = Score(item, terms) })
            .Where(x => x.Score > 0).OrderByDescending(x => x.Score).ThenBy(x => x.Item.ItemName)
            .Select(x => x.Item);
    }

    private static int Score(CollectibleCatalog item, string[] terms)
    {
        var name = item.ItemName.ToLowerInvariant(); var brand = item.Brand.ToLowerInvariant();
        var series = item.Series.ToLowerInvariant(); var reference = item.ReferenceNumber.ToLowerInvariant();
        return terms.Sum(term => (name.Contains(term) ? 5 : 0) + (reference.Contains(term) ? 6 : 0)
            + (brand.Contains(term) ? 3 : 0) + (series.Contains(term) ? 2 : 0));
    }

    private static string[] Terms(string value) => value.ToLowerInvariant()
        .Split(new[] { ' ', '-', '/', '#', '\'', '"', ',', '.' }, StringSplitOptions.RemoveEmptyEntries)
        .Where(x => x.Length > 1).Distinct().ToArray();

    private static object ToResult(CollectibleCatalog item) => new
    {
        item.CatalogItemId, item.ItemName, item.CategoryCode, item.Brand, item.Series,
        item.ReferenceNumber, item.ReleaseYear, item.Description, item.ImageUrl
    };
}
