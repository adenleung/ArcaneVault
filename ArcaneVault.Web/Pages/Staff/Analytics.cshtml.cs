/* Name: Aden Leung | Student Admin No.: 252744K | Tutorial Group: IT2814 */
using ArcaneVault.Web.Models;
using ArcaneVault.Web.Services;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ArcaneVault.Web.Pages.Staff;

/// <summary>
/// Loads every dashboard dataset from the required Web API. The Razor Page never queries SQLite directly.
/// </summary>
public class AnalyticsModel(ApiClient api, ILogger<AnalyticsModel> logger) : PageModel
{
    public DateTime From { get; set; }
    public DateTime To { get; set; }
    public string? Category { get; set; }
    public string? Source { get; set; }
    public string? Product { get; set; }
    public string GroupBy { get; set; } = "month";
    public string Metric { get; set; } = "value";
    public string ChartType { get; set; } = "line";
    public bool ComparePrevious { get; set; } = true;
    public bool HasLoadWarning { get; set; }

    public List<string> Products { get; set; } = [];
    public AnalyticsSummary? Summary { get; set; }
    public List<CategoryDto> Categories { get; set; } = [];
    public List<ChartPoint> Trend { get; set; } = [];
    public List<ChartPoint> CategoryShare { get; set; } = [];
    public List<ChartPoint> Sources { get; set; } = [];
    public List<ChartPoint> TopItems { get; set; } = [];
    public List<InsightDto> Insights { get; set; } = [];

    public async Task OnGetAsync(DateTime? from, DateTime? to, string? category, string? source,
        string? product, string? groupBy, string? metric, string? chartType, bool? comparePrevious)
    {
        // Normalise and whitelist query-string values before forwarding them to the API.
        From = (from ?? DateTime.Today.AddDays(-89)).Date;
        To = (to ?? DateTime.Today).Date;
        if (From > To) (From, To) = (To, From);

        Category = category;
        Source = source;
        Product = product;
        GroupBy = new[] { "day", "week", "month", "quarter", "year" }.Contains(groupBy) ? groupBy! : "month";
        Metric = new[] { "value", "quantity", "estimated" }.Contains(metric) ? metric! : "value";
        ChartType = new[] { "line", "bars" }.Contains(chartType) ? chartType! : "line";
        ComparePrevious = comparePrevious ?? true;

        var query = $"from={From:yyyy-MM-dd}&to={To:yyyy-MM-dd}" +
                    $"&category={Uri.EscapeDataString(category ?? "")}" +
                    $"&source={Uri.EscapeDataString(source ?? "")}" +
                    $"&product={Uri.EscapeDataString(product ?? "")}" +
                    $"&groupBy={GroupBy}&compare={ComparePrevious.ToString().ToLowerInvariant()}";

        Categories = await SafeGet<List<CategoryDto>>("api/categories") ?? [];
        Products = await SafeGet<List<string>>("api/staffanalytics/products") ?? [];
        Summary = await SafeGet<AnalyticsSummary>($"api/staffanalytics/summary?{query}");
        Trend = await SafeGet<List<ChartPoint>>($"api/staffanalytics/trend?{query}") ?? [];
        CategoryShare = await SafeGet<List<ChartPoint>>($"api/staffanalytics/categories?{query}") ?? [];
        Sources = await SafeGet<List<ChartPoint>>($"api/staffanalytics/sources?{query}") ?? [];
        TopItems = await SafeGet<List<ChartPoint>>($"api/staffanalytics/top-items?{query}") ?? [];
        Insights = await SafeGet<List<InsightDto>>($"api/staffanalytics/insights?{query}") ?? [];
    }

    /// <summary>
    /// Converts a failed dataset into a friendly partial dashboard while logging the technical detail for staff.
    /// </summary>
    private async Task<T?> SafeGet<T>(string url)
    {
        try
        {
            return await api.GetAsync<T>(url);
        }
        catch (Exception exception)
        {
            HasLoadWarning = true;
            logger.LogWarning(exception, "Analytics dataset failed: {Url}", url);
            return default;
        }
    }
}
