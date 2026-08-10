/*
 * Name: Aden Leung
 * Student Admin No.: 252744K
 * Tutorial Group: IT2814
 */
using ArcaneVault.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace ArcaneVault.Api.Data;

/// <summary>
/// Creates one controlled, realistic demonstration dataset when the database is empty.
/// Keeping this process in one class prevents competing schema/data "repair" routines.
/// </summary>
public static class DbSeeder
{
    public static async Task SeedAsync(ArcaneVaultDbContext db)
    {
        // A role means seeding has already completed, so existing user data is preserved.
        if (await db.ArcaneVaultUserRoles.AnyAsync()) return;

        db.AddRange(
            new ArcaneVaultUserRole { RoleId = 1, RoleName = "Staff" },
            new ArcaneVaultUserRole { RoleId = 2, RoleName = "User" });

        // PasswordSecurity.Hash ensures the demonstration passwords are never stored as plain text.
        db.AddRange(
            new ArcaneVaultUser { UserName = "staff", Email = "staff@arcanevault.test", PasswordHash = PasswordSecurity.Hash("Staff123!"), RoleId = 1 },
            new ArcaneVaultUser { UserName = "aden", Email = "aden@arcanevault.test", PasswordHash = PasswordSecurity.Hash("Aden123!"), RoleId = 2 },
            new ArcaneVaultUser { UserName = "collector", Email = "collector@arcanevault.test", PasswordHash = PasswordSecurity.Hash("Collect123!"), RoleId = 2 });

        db.Categories.AddRange(
            new Category { CategoryCode = "SHOE", CategoryName = "Sneakers" },
            new Category { CategoryCode = "CARD", CategoryName = "Trading Cards" },
            new Category { CategoryCode = "FIG", CategoryName = "Designer Figures" },
            new Category { CategoryCode = "WATCH", CategoryName = "Watches" },
            new Category { CategoryCode = "COMIC", CategoryName = "Comics" },
            new Category { CategoryCode = "GAME", CategoryName = "Gaming" });
        await db.SaveChangesAsync();

        db.CollectibleCatalog.AddRange(
            new CollectibleCatalog { ItemName = "Apex High 'Shadow'", CategoryCode = "SHOE", Brand = "Apex", Series = "High", ReferenceNumber = "SHADOW", ReleaseYear = "2025", Description = "Limited monochrome high-top sneaker with numbered lace tag.", ImageUrl = "/images/products/apex-high.webp" },
            new CollectibleCatalog { ItemName = "Mono Runner 02", CategoryCode = "SHOE", Brand = "Mono", Series = "Runner", ReferenceNumber = "02", ReleaseYear = "2025", Description = "Limited technical runner with a sculpted sole.", ImageUrl = "/images/products/mono-runner.webp" },
            new CollectibleCatalog { ItemName = "Solar Dragon No. 04", CategoryCode = "CARD", Brand = "Solar Archive", Series = "Solar Archive", ReferenceNumber = "04", ReleaseYear = "2026", Description = "Foil fantasy trading card from the Solar Archive series.", ImageUrl = "/images/products/solar-dragon.webp" },
            new CollectibleCatalog { ItemName = "Aurora Falcon No. 27", CategoryCode = "CARD", Brand = "Aurora League", Series = "Championship", ReferenceNumber = "27", ReleaseYear = "2025", Description = "Holographic championship trading card.", ImageUrl = "/images/products/aurora-falcon.webp" },
            new CollectibleCatalog { ItemName = "Lunar Ace No. 11", CategoryCode = "CARD", Brand = "Lunar League", Series = "Championship", ReferenceNumber = "11", ReleaseYear = "2026", Description = "Holographic Lunar League trading card.", ImageUrl = "/images/products/lunar-ace.webp" },
            new CollectibleCatalog { ItemName = "Vault Guardian", CategoryCode = "FIG", Brand = "Arcane", Series = "Vault", ReferenceNumber = "VG-01", ReleaseYear = "2025", Description = "Numbered vinyl guardian figure in graphite finish.", ImageUrl = "/images/products/vault-guardian.webp" },
            new CollectibleCatalog { ItemName = "Obsidian Mecha Prototype", CategoryCode = "FIG", Brand = "Obsidian", Series = "Mecha", ReferenceNumber = "PROTO-01", ReleaseYear = "2024", Description = "Numbered articulated display prototype.", ImageUrl = "/images/products/obsidian-mecha.webp" },
            new CollectibleCatalog { ItemName = "Atlas Field Watch", CategoryCode = "WATCH", Brand = "Atlas", Series = "Field", ReferenceNumber = "AFW-01", ReleaseYear = "2024", Description = "Mechanical field watch with brushed steel case.", ImageUrl = "/images/products/atlas-watch.webp" },
            new CollectibleCatalog { ItemName = "Celestial Rift Issue #1", CategoryCode = "COMIC", Brand = "Celestial Rift", Series = "First Edition", ReferenceNumber = "ISSUE-1", ReleaseYear = "2025", Description = "Sealed first-edition science-fiction comic.", ImageUrl = "/images/products/celestial-rift.webp" },
            new CollectibleCatalog { ItemName = "Neon Circuit Vol. 1", CategoryCode = "COMIC", Brand = "Neon Circuit", Series = "First Print", ReferenceNumber = "VOL-1", ReleaseYear = "2025", Description = "First-print science-fiction graphic novel.", ImageUrl = "/images/products/neon-circuit.webp" },
            new CollectibleCatalog { ItemName = "Chrono Deck Handheld", CategoryCode = "GAME", Brand = "Chrono", Series = "Deck", ReferenceNumber = "CD-01", ReleaseYear = "2025", Description = "Limited handheld console with display dock and case.", ImageUrl = "/images/products/chrono-deck.webp" },
            new CollectibleCatalog { ItemName = "Pixel Core Console", CategoryCode = "GAME", Brand = "Pixel Core", Series = "Anniversary", ReferenceNumber = "PC-ANN", ReleaseYear = "2026", Description = "Anniversary miniature display console with controller.", ImageUrl = "/images/products/pixel-core.webp" });
        await db.SaveChangesAsync();

        // Each specification is the single source for an item, its category link and its acquisition record.
        // Relative dates keep the seed useful for current/prior-period comparisons without future-dated data.
        var specifications = new[]
        {
            new SeedItemSpec("aden", "Apex High 'Shadow'", "Limited monochrome high-top sneaker with original box and numbered lace tag.", 2, 265m, 230m, "/images/products/apex-high.webp", "SHOE", -82, "Retail store", "Pre-owned"),
            new SeedItemSpec("aden", "Solar Dragon No. 04", "Foil fantasy trading card from the Solar Archive series in a protective slab.", 1, 420m, 350m, "/images/products/solar-dragon.webp", "CARD", -66, "Online marketplace", "New"),
            new SeedItemSpec("collector", "Aurora Falcon No. 27", "Holographic championship card preserved in a clear collector slab.", 1, 1350m, 1100m, "/images/products/aurora-falcon.webp", "CARD", -74, "Online marketplace", "New"),
            new SeedItemSpec("aden", "Celestial Rift Issue #1", "Sealed first-edition science-fiction comic with archival backing board.", 1, 1250m, 980m, "/images/products/celestial-rift.webp", "COMIC", -52, "Convention", "New"),
            new SeedItemSpec("aden", "Mono Runner 02", "Limited technical runner with a sculpted sole and complete packaging.", 3, 190m, 172m, "/images/products/mono-runner.webp", "SHOE", -45, "Retail store", "New"),
            new SeedItemSpec("collector", "Obsidian Mecha Prototype", "Numbered articulated display prototype in a graphite collector finish.", 1, 2400m, 1950m, "/images/products/obsidian-mecha.webp", "FIG", -35, "Convention", "New"),
            new SeedItemSpec("aden", "Vault Guardian", "Numbered vinyl guardian figure in a graphite finish.", 1, 145m, 118m, "/images/products/vault-guardian.webp", "FIG", -29, "Convention", "Pre-owned"),
            new SeedItemSpec("collector", "Atlas Field Watch", "Mechanical field watch with brushed steel case and service record.", 1, 780m, 690m, "/images/products/atlas-watch.webp", "WATCH", -20, "Online marketplace", "New"),
            new SeedItemSpec("collector", "Neon Circuit Vol. 1", "First-print science-fiction graphic novel kept in an archival sleeve.", 2, 85m, 62m, "/images/products/neon-circuit.webp", "COMIC", -14, "Retail store", "New"),
            new SeedItemSpec("aden", "Chrono Deck Handheld", "Limited retro-futuristic handheld console with display dock and case.", 1, 950m, 820m, "/images/products/chrono-deck.webp", "GAME", -10, "Online marketplace", "New"),
            new SeedItemSpec("collector", "Pixel Core Console", "Anniversary miniature display console with matching controller.", 1, 210m, 180m, "/images/products/pixel-core.webp", "GAME", -7, "Online marketplace", "New"),
            new SeedItemSpec("collector", "Lunar Ace No. 11", "Holographic championship trading card from the Lunar League set.", 2, 310m, 280m, "/images/products/lunar-ace.webp", "CARD", -3, "Trade", "Pre-owned")
        };

        var items = specifications.Select(specification => new CollectionItem
        {
            UserName = specification.UserName,
            ItemName = specification.ItemName,
            Description = specification.Description,
            StartingQuantity = specification.Quantity,
            CurrentQuantity = specification.Quantity,
            EstimatedUnitValue = specification.EstimatedUnitValue,
            ImageUrl = specification.ImageUrl,
            DateAdded = DateTime.UtcNow.AddDays(specification.DaysAgo)
        }).ToArray();

        db.CollectionItems.AddRange(items);
        await db.SaveChangesAsync(); // Generates ItemId values needed by the related rows below.

        for (var index = 0; index < items.Length; index++)
        {
            var item = items[index];
            var specification = specifications[index];

            db.CollectionItemCategories.Add(new CollectionItemCategory
            {
                ItemId = item.ItemId,
                CategoryCode = specification.CategoryCode
            });
            db.AcquisitionRecords.Add(new AcquisitionRecord
            {
                ItemId = item.ItemId,
                UserName = item.UserName,
                Quantity = item.StartingQuantity,
                UnitPrice = specification.PurchasePrice,
                PurchaseDate = DateTime.Today.AddDays(specification.DaysAgo),
                PurchaseSource = specification.Source,
                Condition = specification.Condition
            });
        }

        await db.SaveChangesAsync();
    }

    /// <summary>Readable seed input used to create all related database rows consistently.</summary>
    private sealed record SeedItemSpec(
        string UserName,
        string ItemName,
        string Description,
        int Quantity,
        decimal EstimatedUnitValue,
        decimal PurchasePrice,
        string ImageUrl,
        string CategoryCode,
        int DaysAgo,
        string Source,
        string Condition);
}
