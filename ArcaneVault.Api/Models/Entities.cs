/*
 * Name: Aden Leung
 * Student Admin No.: 252744K
 * Tutorial Group: IT2814
 */
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ArcaneVault.Api.Models;

public class ArcaneVaultUserRole
{
    [Key] public int RoleId { get; set; }
    [Required, StringLength(30)] public string RoleName { get; set; } = "User";
    public ICollection<ArcaneVaultUser> Users { get; set; } = [];
}

public class ArcaneVaultUser
{
    [Key, StringLength(40)] public string UserName { get; set; } = string.Empty;
    [Required, EmailAddress, StringLength(120)] public string Email { get; set; } = string.Empty;
    [Required] public string PasswordHash { get; set; } = string.Empty;
    public bool IsDeleted { get; set; }
    public int RoleId { get; set; }
    public ArcaneVaultUserRole? Role { get; set; }
    public ICollection<CollectionItem> CollectionItems { get; set; } = [];
}

public class Category
{
    [Key, StringLength(12)] public string CategoryCode { get; set; } = string.Empty;
    [Required, StringLength(60)] public string CategoryName { get; set; } = string.Empty;
    public ICollection<CollectionItemCategory> CollectionItemCategories { get; set; } = [];
}

public class CollectionItem
{
    [Key] public int ItemId { get; set; }
    [Required, StringLength(100)] public string ItemName { get; set; } = string.Empty;
    [StringLength(700)] public string Description { get; set; } = string.Empty;
    public bool IsDeleted { get; set; }
    [Range(0, 100000)] public int StartingQuantity { get; set; }
    [Range(0, 100000)] public int CurrentQuantity { get; set; }
    [Range(0, 1000000)] public decimal EstimatedUnitValue { get; set; }
    public DateTime DateAdded { get; set; } = DateTime.UtcNow;
    [StringLength(255)] public string ImageUrl { get; set; } = "/images/products/placeholder.webp";
    [Required, StringLength(40)] public string UserName { get; set; } = string.Empty;
    public ArcaneVaultUser? User { get; set; }
    public ICollection<CollectionItemCategory> CollectionItemCategories { get; set; } = [];
    public ICollection<AcquisitionRecord> Acquisitions { get; set; } = [];
}

public class CollectionItemCategory
{
    public int ItemId { get; set; }
    public CollectionItem? Item { get; set; }
    [StringLength(12)] public string CategoryCode { get; set; } = string.Empty;
    public Category? Category { get; set; }
}

public class AcquisitionRecord
{
    [Key] public int AcquisitionId { get; set; }
    public int ItemId { get; set; }
    public CollectionItem? Item { get; set; }
    [Required, StringLength(40)] public string UserName { get; set; } = string.Empty;
    public ArcaneVaultUser? User { get; set; }
    [Range(1, 100000)] public int Quantity { get; set; }
    [Column(TypeName = "decimal(12,2)"), Range(0, 1000000)] public decimal UnitPrice { get; set; }
    public DateTime PurchaseDate { get; set; }
    [Required, StringLength(50)] public string PurchaseSource { get; set; } = "Retail store";
    [Required, StringLength(30)] public string Condition { get; set; } = "New";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class CollectibleCatalog
{
    [Key] public int CatalogItemId { get; set; }
    [Required, StringLength(120)] public string ItemName { get; set; } = string.Empty;
    [Required, StringLength(12)] public string CategoryCode { get; set; } = string.Empty;
    [StringLength(60)] public string Brand { get; set; } = string.Empty;
    [StringLength(80)] public string Series { get; set; } = string.Empty;
    [StringLength(40)] public string ReferenceNumber { get; set; } = string.Empty;
    [StringLength(20)] public string ReleaseYear { get; set; } = string.Empty;
    [StringLength(700)] public string Description { get; set; } = string.Empty;
    [StringLength(255)] public string ImageUrl { get; set; } = "/images/products/placeholder.webp";
}
