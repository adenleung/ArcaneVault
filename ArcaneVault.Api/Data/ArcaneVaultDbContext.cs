/*
 * Name: Aden Leung
 * Student Admin No.: 252744K
 * Tutorial Group: IT2814
 */
using ArcaneVault.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace ArcaneVault.Api.Data;

public class ArcaneVaultDbContext(DbContextOptions<ArcaneVaultDbContext> options) : DbContext(options)
{
    public DbSet<ArcaneVaultUser> ArcaneVaultUsers => Set<ArcaneVaultUser>();
    public DbSet<ArcaneVaultUserRole> ArcaneVaultUserRoles => Set<ArcaneVaultUserRole>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<CollectionItem> CollectionItems => Set<CollectionItem>();
    public DbSet<CollectionItemCategory> CollectionItemCategories => Set<CollectionItemCategory>();
    public DbSet<AcquisitionRecord> AcquisitionRecords => Set<AcquisitionRecord>();
    public DbSet<CollectibleCatalog> CollectibleCatalog => Set<CollectibleCatalog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<CollectionItemCategory>().HasKey(x => new { x.ItemId, x.CategoryCode });
        modelBuilder.Entity<ArcaneVaultUser>().HasIndex(x => x.Email).IsUnique();
        modelBuilder.Entity<ArcaneVaultUser>()
            .HasOne(x => x.Role).WithMany(x => x.Users).HasForeignKey(x => x.RoleId);
        modelBuilder.Entity<CollectionItem>()
            .HasOne(x => x.User).WithMany(x => x.CollectionItems).HasForeignKey(x => x.UserName);
        modelBuilder.Entity<CollectionItemCategory>()
            .HasOne(x => x.Item).WithMany(x => x.CollectionItemCategories).HasForeignKey(x => x.ItemId);
        modelBuilder.Entity<CollectionItemCategory>()
            .HasOne(x => x.Category).WithMany(x => x.CollectionItemCategories).HasForeignKey(x => x.CategoryCode)
            .OnDelete(DeleteBehavior.Restrict); // Protect categories used by collection items at database level too.
        modelBuilder.Entity<AcquisitionRecord>()
            .HasOne(x => x.Item).WithMany(x => x.Acquisitions).HasForeignKey(x => x.ItemId)
            .OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<CollectibleCatalog>().HasIndex(x => new { x.ItemName, x.ReferenceNumber });
        modelBuilder.Entity<AcquisitionRecord>()
            .HasOne(x => x.User).WithMany().HasForeignKey(x => x.UserName)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
