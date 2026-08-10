/* Name: Aden Leung | Student Admin No.: 252744K | Tutorial Group: IT2814 */
using ArcaneVault.Api.Data;
using ArcaneVault.Api.Models;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace ArcaneVault.Tests;

public class DatabaseTests
{
    [Fact]
    public async Task Email_IsUniqueCaseNormalisedByAccountWorkflow()
    {
        await using var fixture = await DatabaseFixture.CreateAsync();
        fixture.Db.ArcaneVaultUsers.Add(new ArcaneVaultUser { UserName="first", Email="same@example.com", PasswordHash="hash", RoleId=2 });
        await fixture.Db.SaveChangesAsync();
        fixture.Db.ArcaneVaultUsers.Add(new ArcaneVaultUser { UserName="second", Email="same@example.com", PasswordHash="hash", RoleId=2 });
        await Assert.ThrowsAsync<DbUpdateException>(() => fixture.Db.SaveChangesAsync());
    }

    [Fact]
    public async Task UserName_IsUniqueBecauseItIsTheAccountPrimaryKey()
    {
        await using var fixture = await DatabaseFixture.CreateAsync();
        fixture.Db.ChangeTracker.Clear();
        fixture.Db.ArcaneVaultUsers.Add(new ArcaneVaultUser { UserName="user", Email="other@example.com", PasswordHash="hash", RoleId=2 });
        await Assert.ThrowsAsync<DbUpdateException>(() => fixture.Db.SaveChangesAsync());
    }

    [Fact]
    public async Task CategoryRelationship_UsesRestrictDeletion()
    {
        await using var fixture = await DatabaseFixture.CreateAsync();
        var relationship = fixture.Db.Model.FindEntityType(typeof(CollectionItemCategory))!.GetForeignKeys()
            .Single(x => x.PrincipalEntityType.ClrType == typeof(Category));
        Assert.Equal(DeleteBehavior.Restrict, relationship.DeleteBehavior);
    }

    [Fact]
    public async Task SoftDeletedItems_AreExcludedFromActiveCollection()
    {
        await using var fixture = await DatabaseFixture.CreateAsync();
        fixture.Db.CollectionItems.AddRange(
            new CollectionItem { ItemName="Visible sneaker", Description="Blue", UserName="user", IsDeleted=false },
            new CollectionItem { ItemName="Removed card", Description="Red", UserName="user", IsDeleted=true });
        await fixture.Db.SaveChangesAsync();
        var active = await fixture.Db.CollectionItems.Where(x => !x.IsDeleted).ToListAsync();
        Assert.Single(active);
        Assert.Equal("Visible sneaker", active[0].ItemName);
    }

    [Fact]
    public async Task SearchableFields_AreAvailableAfterSafeMaterialisation()
    {
        await using var fixture = await DatabaseFixture.CreateAsync();
        var category = new Category { CategoryCode="SNK", CategoryName="Sneakers" };
        var item = new CollectionItem { ItemName="Air Runner", Description="Limited blue pair", UserName="user", CurrentQuantity=2, StartingQuantity=3, EstimatedUnitValue=450 };
        fixture.Db.AddRange(category, item); await fixture.Db.SaveChangesAsync();
        fixture.Db.CollectionItemCategories.Add(new CollectionItemCategory { ItemId=item.ItemId, CategoryCode=category.CategoryCode });
        await fixture.Db.SaveChangesAsync();
        var rows = await fixture.Db.CollectionItems.Include(x=>x.CollectionItemCategories).ThenInclude(x=>x.Category).ToListAsync();
        Assert.Contains(rows, x=>x.ItemName.Contains("Runner") && x.Description.Contains("blue") && x.CurrentQuantity==2 && x.EstimatedUnitValue==450 && x.CollectionItemCategories.Any(c=>c.Category!.CategoryName=="Sneakers"));
    }

    private sealed class DatabaseFixture : IAsyncDisposable
    {
        public SqliteConnection Connection { get; }
        public ArcaneVaultDbContext Db { get; }
        private DatabaseFixture(SqliteConnection connection, ArcaneVaultDbContext db) { Connection=connection; Db=db; }
        public static async Task<DatabaseFixture> CreateAsync()
        {
            var connection = new SqliteConnection("Data Source=:memory:"); await connection.OpenAsync();
            var db = new ArcaneVaultDbContext(new DbContextOptionsBuilder<ArcaneVaultDbContext>().UseSqlite(connection).Options);
            await db.Database.EnsureCreatedAsync();
            db.ArcaneVaultUserRoles.AddRange(new ArcaneVaultUserRole { RoleId=1, RoleName="Staff" }, new ArcaneVaultUserRole { RoleId=2, RoleName="User" });
            db.ArcaneVaultUsers.Add(new ArcaneVaultUser { UserName="user", Email="user@example.com", PasswordHash="hash", RoleId=2 });
            await db.SaveChangesAsync(); return new DatabaseFixture(connection, db);
        }
        public async ValueTask DisposeAsync() { await Db.DisposeAsync(); await Connection.DisposeAsync(); }
    }
}
