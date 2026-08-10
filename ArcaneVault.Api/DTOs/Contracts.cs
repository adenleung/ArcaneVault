/*
 * Name: Aden Leung
 * Student Admin No.: 252744K
 * Tutorial Group: IT2814
 */
using System.ComponentModel.DataAnnotations;

namespace ArcaneVault.Api.DTOs;

public record RegisterRequest(
    [Required, StringLength(40, MinimumLength = 3)] string UserName,
    [Required, EmailAddress] string Email,
    [Required, MinLength(8)] string Password);
public record LoginRequest([Required] string Email, [Required] string Password);
public record LoginResponse(string UserName, string Email, string RoleName, string AccessToken);

public record CategoryRequest(
    [Required, RegularExpression("^[A-Za-z0-9_-]{2,12}$")] string CategoryCode,
    [Required, StringLength(60)] string CategoryName);

public class CollectionItemRequest : IValidatableObject
{
    [Required, StringLength(100)] public string ItemName { get; set; } = string.Empty;
    [StringLength(700)] public string Description { get; set; } = string.Empty;
    [Range(0, 100000)] public int StartingQuantity { get; set; }
    [Range(0, 100000)] public int CurrentQuantity { get; set; }
    [Range(0, 1000000)] public decimal EstimatedUnitValue { get; set; }
    [Required] public string CategoryCode { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = "/images/products/placeholder.webp";
    [Range(0, 1000000)] public decimal PurchasePrice { get; set; }
    public DateTime PurchaseDate { get; set; } = DateTime.Today;
    [Required] public string PurchaseSource { get; set; } = "Retail store";
    [Required] public string Condition { get; set; } = "New";

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (CurrentQuantity > StartingQuantity)
            yield return new ValidationResult("Current quantity cannot exceed starting quantity.", [nameof(CurrentQuantity)]);
        if (PurchaseDate.Date > DateTime.Today)
            yield return new ValidationResult("Purchase date cannot be in the future.", [nameof(PurchaseDate)]);
        if (!string.IsNullOrWhiteSpace(ImageUrl) && !ImageUrl.StartsWith('/')
            && !(Uri.TryCreate(ImageUrl, UriKind.Absolute, out var uri) && uri.Scheme is "http" or "https"))
            yield return new ValidationResult("Use a local image path or a valid HTTP/HTTPS image URL.", [nameof(ImageUrl)]);
    }
}

public record CategoryDto(string CategoryCode, string CategoryName, int ItemCount);
public record ItemDto(int ItemId, string ItemName, string Description, int StartingQuantity,
    int CurrentQuantity, decimal EstimatedUnitValue, DateTime DateAdded, string ImageUrl,
    string UserName, string CategoryCode, string CategoryName);

public record MetricDto(decimal Value, decimal Change, bool HasPrevious = true);
public record AnalyticsSummary(MetricDto RecordedValue, MetricDto ItemsAcquired,
    MetricDto ActiveUsers, MetricDto AveragePrice, MetricDto EstimatedCollectionValue,
    MetricDto AcquisitionEvents, MetricDto CategoriesRepresented, MetricDto LeadingSourceShare);
public record ChartPoint(string Label, decimal Value, decimal? SecondaryValue = null,
    decimal? ComparisonValue = null, decimal? ComparisonSecondaryValue = null,
    decimal? EstimatedValue = null, decimal? ComparisonEstimatedValue = null);
public record InsightDto(string Title, string Detail, string Tone);
