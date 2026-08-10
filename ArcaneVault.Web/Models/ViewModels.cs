/*
 * Name: Aden Leung
 * Student Admin No.: 252744K
 * Tutorial Group: IT2814
 */
using System.ComponentModel.DataAnnotations;

namespace ArcaneVault.Web.Models;

public record LoginResponse(string UserName, string Email, string RoleName, string AccessToken);
public record CategoryDto(string CategoryCode, string CategoryName, int ItemCount);
public record ItemDto(int ItemId, string ItemName, string Description, int StartingQuantity,
    int CurrentQuantity, decimal EstimatedUnitValue, DateTime DateAdded, string ImageUrl,
    string UserName, string CategoryCode, string CategoryName);

public class ItemInput : IValidatableObject
{
    [Required, Display(Name = "Item name"), StringLength(100)] public string ItemName { get; set; } = string.Empty;
    [StringLength(700)] public string Description { get; set; } = string.Empty;
    [Range(0, 100000), Display(Name = "Starting quantity")] public int StartingQuantity { get; set; } = 1;
    [Range(0, 100000), Display(Name = "Current quantity")] public int CurrentQuantity { get; set; } = 1;
    [Range(0, 1000000), Display(Name = "Estimated unit value (S$)")] public decimal EstimatedUnitValue { get; set; }
    [Required, Display(Name = "Category")] public string CategoryCode { get; set; } = string.Empty;
    [Display(Name = "Product image")] public string ImageUrl { get; set; } = "/images/products/placeholder.webp";
    [Range(0, 1000000), Display(Name = "Purchase price (S$)")] public decimal PurchasePrice { get; set; }
    [DataType(DataType.Date), Display(Name = "Purchase date")] public DateTime PurchaseDate { get; set; } = DateTime.Today;
    [Required, Display(Name = "Purchase source")] public string PurchaseSource { get; set; } = "Retail store";
    [Required] public string Condition { get; set; } = "New";

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (CurrentQuantity > StartingQuantity)
            yield return new ValidationResult("Current quantity cannot exceed starting quantity.", [nameof(CurrentQuantity)]);
        if (PurchaseDate.Date > DateTime.Today)
            yield return new ValidationResult("Purchase date cannot be in the future.", [nameof(PurchaseDate)]);
    }
}

public record MetricDto(decimal Value, decimal Change, bool HasPrevious = true);
public record AnalyticsSummary(MetricDto RecordedValue, MetricDto ItemsAcquired, MetricDto ActiveUsers,
    MetricDto AveragePrice, MetricDto EstimatedCollectionValue, MetricDto AcquisitionEvents,
    MetricDto CategoriesRepresented, MetricDto LeadingSourceShare);
public record ChartPoint(string Label, decimal Value, decimal? SecondaryValue = null,
    decimal? ComparisonValue = null, decimal? ComparisonSecondaryValue = null,
    decimal? EstimatedValue = null, decimal? ComparisonEstimatedValue = null);
public record InsightDto(string Title, string Detail, string Tone);

public record CatalogMatchDto(int CatalogItemId, string ItemName, string CategoryCode, string Brand,
    string Series, string ReferenceNumber, string ReleaseYear, string Description, string ImageUrl);
public record AiIdentificationDto(string ItemType, string PossibleName, string Brand, string Series,
    string ReferenceNumber, string ReleaseYear, string Description, string[] VisibleText, double Confidence);
public record SmartAddResponse(AiIdentificationDto Identification, CatalogMatchDto[] Matches, string Disclaimer);
public record AssistantResponse(string Answer, DateTime GeneratedAt);
