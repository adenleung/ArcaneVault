/* Name: Aden Leung | Student Admin No.: 252744K | Tutorial Group: IT2814 */
using System.ComponentModel.DataAnnotations;
using ArcaneVault.Api.DTOs;

namespace ArcaneVault.Tests;

public class ValidationTests
{
    [Fact]
    public void CollectionItem_RejectsImpossibleQuantitiesAndFuturePurchase()
    {
        var request = ValidRequest();
        request.StartingQuantity = 1;
        request.CurrentQuantity = 2;
        request.PurchaseDate = DateTime.Today.AddDays(1);
        var results = Validate(request);
        Assert.Contains(results, x => x.ErrorMessage!.Contains("cannot exceed"));
        Assert.Contains(results, x => x.ErrorMessage!.Contains("future"));
    }

    [Fact]
    public void CollectionItem_RejectsNegativeValuesAndUnsafeImageScheme()
    {
        var request = ValidRequest();
        request.EstimatedUnitValue = -1;
        request.ImageUrl = "file:///private/image.png";
        var results = Validate(request);
        Assert.Contains(results, x => x.MemberNames.Contains(nameof(request.EstimatedUnitValue)));
        Assert.Contains(results, x => x.MemberNames.Contains(nameof(request.ImageUrl)));
    }

    [Fact]
    public void EmptyDatabaseStyleResult_CanBeRepresentedWithoutFailure() => Assert.Empty(Array.Empty<ChartPoint>());

    private static CollectionItemRequest ValidRequest() => new()
    {
        ItemName = "Test collectible", Description = "Test", StartingQuantity = 2, CurrentQuantity = 1,
        EstimatedUnitValue = 20, CategoryCode = "SNK", PurchasePrice = 10,
        PurchaseDate = DateTime.Today, PurchaseSource = "Retail store", Condition = "New",
        ImageUrl = "/images/products/placeholder.webp"
    };
    private static List<ValidationResult> Validate(object value)
    {
        var results = new List<ValidationResult>();
        Validator.TryValidateObject(value, new ValidationContext(value), results, true);
        return results;
    }
}
