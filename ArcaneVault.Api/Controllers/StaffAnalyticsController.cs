/*
 * Name: Aden Leung
 * Student Admin No.: 252744K
 * Tutorial Group: IT2814
 */
using ArcaneVault.Api.Data;
using ArcaneVault.Api.DTOs;
using ArcaneVault.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ArcaneVault.Api.Controllers;

[ApiController, Route("api/staffanalytics")]
public class StaffAnalyticsController(ArcaneVaultDbContext db, ApiTokenService tokens) : ApiControllerBase(db, tokens)
{
    /// <summary>
    /// Loads related rows with EF Core, then applies simple in-memory predicates. This deliberately avoids
    /// complex SQLite expressions that may not translate consistently across provider versions.
    /// </summary>
    private async Task<List<AcquisitionRecord>> FilterAsync(DateTime? from, DateTime? to,
        string? category, string? source, string? product = null)
    {
        var rows = await db.AcquisitionRecords.AsNoTracking()
            .Include(x => x.Item)!.ThenInclude(x => x.CollectionItemCategories).ThenInclude(x => x.Category)
            .Where(x => x.Item != null && !x.Item.IsDeleted)
            .ToListAsync();
        return rows.Where(x => (!from.HasValue || x.PurchaseDate.Date >= from.Value.Date)
            && (!to.HasValue || x.PurchaseDate.Date <= to.Value.Date)
            && (string.IsNullOrWhiteSpace(category) || x.Item!.CollectionItemCategories.Any(c => c.CategoryCode == category))
            && (string.IsNullOrWhiteSpace(source) || x.PurchaseSource == source)
            && (string.IsNullOrWhiteSpace(product) || x.Item!.ItemName == product)).ToList();
    }

    /// <summary>Calculates the eight dashboard KPIs and their equivalent previous-period comparisons.</summary>
    [HttpGet("summary")]
    public async Task<ActionResult<AnalyticsSummary>> Summary(DateTime? from, DateTime? to,
        string? category, string? source, string? product)
    {
        if (RequireStaff() is { } denied) return denied;
        var start = (from ?? DateTime.Today.AddDays(-29)).Date;
        var end = (to ?? DateTime.Today).Date;
        var days = Math.Max(1, (end - start).Days + 1);
        var current = await FilterAsync(start, end, category, source, product);
        var previous = await FilterAsync(start.AddDays(-days), start.AddDays(-1), category, source, product);
        var currentValue = current.Sum(x => x.UnitPrice * x.Quantity);
        var previousValue = previous.Sum(x => x.UnitPrice * x.Quantity);
        var currentQty = current.Sum(x => x.Quantity);
        var previousQty = previous.Sum(x => x.Quantity);
        var users = current.Select(x => x.UserName).Distinct().Count();
        var previousUsers = previous.Select(x => x.UserName).Distinct().Count();
        var avg = currentQty == 0 ? 0 : currentValue / currentQty;
        var previousAvg = previousQty == 0 ? 0 : previousValue / previousQty;
        var currentCategories = DistinctCategoryCount(current);
        var previousCategories = DistinctCategoryCount(previous);
        var currentSourceShare = LeadingSourceShare(current);
        var previousSourceShare = LeadingSourceShare(previous);
        return new AnalyticsSummary(Metric(currentValue, previousValue), Metric(currentQty, previousQty),
            Metric(users, previousUsers), Metric(avg, previousAvg),
            Metric(EstimatedValue(current), EstimatedValue(previous)),
            Metric(current.Count, previous.Count), Metric(currentCategories, previousCategories),
            Metric(currentSourceShare, previousSourceShare));
    }

    /// <summary>Groups matching acquisitions by the selected calendar period for the main chart.</summary>
    [HttpGet("trend")]
    public async Task<ActionResult<IEnumerable<ChartPoint>>> Trend(DateTime? from, DateTime? to,
        string? category, string? source, string? product, string groupBy = "month", bool compare = true)
    {
        if (RequireStaff() is { } denied) return denied;
        var start = (from ?? DateTime.Today.AddDays(-89)).Date;
        var end = (to ?? DateTime.Today).Date;
        if (start > end) (start, end) = (end, start);
        var rows = await FilterAsync(start, end, category, source, product);
        var grouped = rows.GroupBy(x => PeriodStart(x.PurchaseDate, groupBy))
            .OrderBy(x => x.Key)
            .Select(x => new { x.Key, Value = x.Sum(y => y.UnitPrice * y.Quantity), Quantity = x.Sum(y => y.Quantity), Estimated = EstimatedValue(x) }).ToList();
        var previous = new List<(decimal Value, decimal Quantity, decimal Estimated)>();
        if (compare)
        {
            var days = Math.Max(1, (end - start).Days + 1);
            var previousRows = await FilterAsync(start.AddDays(-days), start.AddDays(-1), category, source, product);
            previous = previousRows.GroupBy(x => PeriodStart(x.PurchaseDate, groupBy)).OrderBy(x => x.Key)
                .Select(x => (x.Sum(y => y.UnitPrice * y.Quantity), (decimal)x.Sum(y => y.Quantity), EstimatedValue(x))).ToList();
        }
        return grouped.Select((x, index) => new ChartPoint(PeriodLabel(x.Key, groupBy), x.Value, x.Quantity,
            index < previous.Count ? previous[index].Value : null,
            index < previous.Count ? previous[index].Quantity : null,
            x.Estimated, index < previous.Count ? previous[index].Estimated : null)).ToList();
    }

    [HttpGet("categories")]
    public async Task<ActionResult<IEnumerable<ChartPoint>>> Categories(DateTime? from, DateTime? to, string? category, string? source, string? product)
    {
        if (RequireStaff() is { } denied) return denied;
        var rows = await FilterAsync(from ?? DateTime.Today.AddDays(-89), to ?? DateTime.Today, category, source, product);
        return rows.SelectMany(x => x.Item!.CollectionItemCategories.Select(c => new { Name = c.Category?.CategoryName ?? c.CategoryCode, Record = x }))
            .GroupBy(x => x.Name).Select(x => new ChartPoint(x.Key, x.Sum(y => y.Record.Quantity),
                x.Sum(y => y.Record.UnitPrice * y.Record.Quantity), EstimatedValue: EstimatedValue(x.Select(y => y.Record))))
            .OrderByDescending(x => x.Value).ToList();
    }

    [HttpGet("sources")]
    public async Task<ActionResult<IEnumerable<ChartPoint>>> Sources(DateTime? from, DateTime? to, string? category, string? source, string? product)
    {
        if (RequireStaff() is { } denied) return denied;
        var rows = await FilterAsync(from ?? DateTime.Today.AddDays(-89), to ?? DateTime.Today, category, source, product);
        return rows.GroupBy(x => x.PurchaseSource).Select(x => new ChartPoint(x.Key, x.Sum(y => y.Quantity),
            x.Sum(y => y.UnitPrice * y.Quantity), EstimatedValue: EstimatedValue(x))).OrderByDescending(x => x.Value).ToList();
    }

    [HttpGet("top-items")]
    public async Task<ActionResult<IEnumerable<ChartPoint>>> TopItems(DateTime? from, DateTime? to, string? category, string? source, string? product)
    {
        if (RequireStaff() is { } denied) return denied;
        var rows = await FilterAsync(from ?? DateTime.Today.AddDays(-89), to ?? DateTime.Today, category, source, product);
        return rows.GroupBy(x => x.Item!.ItemName).Select(x => new ChartPoint(x.Key, x.Sum(y => y.Quantity),
            x.Sum(y => y.UnitPrice * y.Quantity), EstimatedValue: EstimatedValue(x))).ToList();
    }

    [HttpGet("products")]
    public async Task<ActionResult<IEnumerable<string>>> Products()
    {
        if (RequireStaff() is { } denied) return denied;
        var names = await db.CollectionItems.AsNoTracking().Where(x => !x.IsDeleted)
            .Select(x => x.ItemName).ToListAsync();
        return names.Distinct(StringComparer.OrdinalIgnoreCase).OrderBy(x => x).ToList();
    }

    /// <summary>
    /// Builds explainable candidates from database rules, ranks them by importance and returns the top three.
    /// No AI service or hard-coded final sentence is used.
    /// </summary>
    [HttpGet("insights")]
    public async Task<ActionResult<IEnumerable<InsightDto>>> Insights(DateTime? from, DateTime? to,
        string? category, string? source, string? product)
    {
        if (RequireStaff() is { } denied) return denied;
        var start = (from ?? DateTime.Today.AddDays(-89)).Date;
        var end = (to ?? DateTime.Today).Date;
        if (start > end) (start, end) = (end, start);
        var rows = await FilterAsync(start, end, category, source, product);
        var total = rows.Sum(x => x.Quantity);
        if (total == 0) return Array.Empty<InsightDto>();
        var candidates = new List<(int Priority, InsightDto Insight)>();
        void Add(int priority, string title, string detail, string tone = "neutral")
            => candidates.Add((priority, new InsightDto(title, detail, tone)));

        var totalValue = rows.Sum(x => x.UnitPrice * x.Quantity);
        var estimatedValue = EstimatedValue(rows);
        var categoryGroups = rows.SelectMany(x => x.Item!.CollectionItemCategories
            .Select(c => new { Name = c.Category?.CategoryName ?? c.CategoryCode, x.Quantity }))
            .GroupBy(x => x.Name).Select(x => new { Name = x.Key, Qty = x.Sum(y => y.Quantity) })
            .OrderByDescending(x => x.Qty).ToList();
        if (categoryGroups.Count > 0)
        {
            var topCategory = categoryGroups[0];
            var categoryShare = topCategory.Qty * 100m / total;
            if (categoryShare >= 60)
                Add(90, "Category concentration", $"{topCategory.Name} represents {categoryShare:0}% of acquired units. Staff should avoid treating this concentrated sample as the whole market.", "warning");
            else
                Add(48, "Balanced category activity", $"The leading category, {topCategory.Name}, represents {categoryShare:0}% of acquired units across {categoryGroups.Count} active categories.", "positive");
        }
        else Add(88, "Category data missing", "The matching acquisition records have no category relationship. Review the affected collection records.", "warning");

        var sourceGroups = rows.GroupBy(x => x.PurchaseSource).Select(x => new { Name = x.Key, Count = x.Count() })
            .OrderByDescending(x => x.Count).First();
        var sourceShare = sourceGroups.Count * 100m / rows.Count;
        if (sourceShare >= 60)
            Add(78, "Acquisition channel dependency", $"{sourceGroups.Name} accounts for {sourceShare:0}% of recorded acquisition events in the selected period.", "warning");
        else
            Add(42, "Leading acquisition channel", $"{sourceGroups.Name} leads with {sourceGroups.Count} events, but activity is distributed across {rows.Select(x => x.PurchaseSource).Distinct().Count()} sources.");

        var itemGroups = rows.GroupBy(x => x.Item!.ItemName).Select(x => new
        {
            Name = x.Key,
            AcquiredValue = x.Sum(y => y.UnitPrice * y.Quantity),
            EstimatedValue = EstimatedValue(x),
            Quantity = x.Sum(y => y.Quantity)
        }).ToList();
        var topEstimated = itemGroups.OrderByDescending(x => x.EstimatedValue).First();
        var estimatedShare = estimatedValue == 0 ? 0 : topEstimated.EstimatedValue * 100m / estimatedValue;
        if (estimatedShare >= 50)
            Add(94, "Estimated value concentration", $"{topEstimated.Name} contributes {estimatedShare:0}% of the selected items' S${estimatedValue:N0} estimated collection value.", "warning");
        else
            Add(65, "Highest estimated-value item", $"{topEstimated.Name} has the highest matching estimated value at S${topEstimated.EstimatedValue:N0}.", "positive");

        if (totalValue > 0 && estimatedValue >= totalValue * 1.25m)
            Add(86, "Estimated value exceeds cost", $"Current estimated value is S${estimatedValue:N0}, compared with S${totalValue:N0} in recorded acquisition value. Both figures are user-entered and unverified.", "positive");
        else if (totalValue > 0 && estimatedValue <= totalValue * .75m)
            Add(86, "Estimated value below cost", $"Current estimated value is S${estimatedValue:N0}, below the S${totalValue:N0} recorded acquisition value. Review the entered estimates before drawing conclusions.", "warning");

        var days = Math.Max(1, (end - start).Days + 1);
        var previous = await FilterAsync(start.AddDays(-days), start.AddDays(-1), category, source, product);
        var previousQty = previous.Sum(x => x.Quantity);
        if (previousQty > 0)
        {
            var change = Math.Round((total - previousQty) * 100m / previousQty, 1);
            if (change >= 20) Add(82, "Acquisition activity increased", $"Acquired quantity increased by {change:0.#}% versus the previous equivalent period.", "positive");
            else if (change <= -20) Add(82, "Acquisition activity decreased", $"Acquired quantity decreased by {Math.Abs(change):0.#}% versus the previous equivalent period.", "warning");
            else Add(38, "Stable acquisition activity", $"Acquired quantity changed by {change:+0.#;-0.#;0}% versus the previous equivalent period.");
        }
        else Add(58, "Limited comparison data", "There are no acquired units in the previous equivalent period, so a percentage comparison would be misleading.");

        var average = totalValue / total;
        Add(30, "Average recorded price", $"The weighted average unit purchase price is S${average:N2} across {total} acquired units.");
        Add(25, "Collector participation", $"{rows.Select(x => x.UserName).Distinct().Count()} collectors recorded {rows.Count} acquisition events in this period.");
        return candidates.OrderByDescending(x => x.Priority).Select(x => x.Insight).Take(3).ToList();
    }

    private static DateTime PeriodStart(DateTime date, string groupBy) => groupBy.ToLowerInvariant() switch
    {
        "day" => date.Date,
        "week" => date.Date.AddDays(-((7 + (int)date.DayOfWeek - (int)DayOfWeek.Monday) % 7)),
        "quarter" => new DateTime(date.Year, ((date.Month - 1) / 3) * 3 + 1, 1),
        "year" => new DateTime(date.Year, 1, 1),
        _ => new DateTime(date.Year, date.Month, 1)
    };
    private static string PeriodLabel(DateTime date, string groupBy) => groupBy.ToLowerInvariant() switch
    {
        "day" => date.ToString("dd MMM"),
        "week" => $"Week of {date:dd MMM}",
        "quarter" => $"Q{((date.Month - 1) / 3) + 1} {date:yyyy}",
        "year" => date.ToString("yyyy"),
        _ => date.ToString("MMM yyyy")
    };
    // A zero previous value has no meaningful percentage change, so HasPrevious is false.
    private static MetricDto Metric(decimal current, decimal previous)
        => new(current, previous == 0 ? 0 : Math.Round((current - previous) / previous * 100, 1), previous != 0);
    // Count each matching item once: current stock × current user-entered estimate.
    private static decimal EstimatedValue(IEnumerable<AcquisitionRecord> rows)
        => rows.Where(x => x.Item is not null).GroupBy(x => x.ItemId)
            .Sum(x => x.First().Item!.EstimatedUnitValue * x.First().Item!.CurrentQuantity);
    private static int DistinctCategoryCount(IEnumerable<AcquisitionRecord> rows)
        => rows.SelectMany(x => x.Item is null ? Enumerable.Empty<CollectionItemCategory>() : x.Item.CollectionItemCategories)
            .Select(x => x.CategoryCode).Distinct(StringComparer.OrdinalIgnoreCase).Count();
    private static decimal LeadingSourceShare(IReadOnlyCollection<AcquisitionRecord> rows)
        => rows.Count == 0 ? 0 : rows.GroupBy(x => x.PurchaseSource).Max(x => x.Count()) * 100m / rows.Count;
}
