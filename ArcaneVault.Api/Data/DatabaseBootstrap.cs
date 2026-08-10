/*
 * Name: Aden Leung
 * Student Admin No.: 252744K
 * Tutorial Group: IT2814
 */
using System.Data;
using Microsoft.EntityFrameworkCore;

namespace ArcaneVault.Api.Data;

public static class DatabaseBootstrap
{
    // Increment only after the final model schema changes. One version controls the whole database.
    public const int SchemaVersion = 5;

    public static async Task PrepareAsync(ArcaneVaultDbContext db, ILogger logger)
    {
        var connection = db.Database.GetDbConnection();
        var dataSource = Path.GetFullPath(connection.DataSource);
        var exists = File.Exists(dataSource);
        var current = exists ? await ReadVersionAsync(connection) : 0;
        var valid = exists && await HasCurrentSchemaAsync(connection);

        // Preserve an incompatible database before rebuilding instead of attempting fragile per-column repairs.
        if (exists && (current != SchemaVersion || !valid))
        {
            await connection.CloseAsync();
            var backup = $"{dataSource}.backup-{DateTime.Now:yyyyMMdd-HHmmss}";
            File.Copy(dataSource, backup, false);
            logger.LogWarning("Older Arcane Vault database backed up to {Backup}; rebuilding schema version {Version}.", backup, SchemaVersion);
            await db.Database.EnsureDeletedAsync();
        }

        await db.Database.EnsureCreatedAsync();
        await SetVersionAsync(connection, SchemaVersion);
    }

    private static async Task<int> ReadVersionAsync(System.Data.Common.DbConnection connection)
    {
        var shouldClose = connection.State != ConnectionState.Open;
        if (shouldClose) await connection.OpenAsync();
        try
        {
            await using var command = connection.CreateCommand();
            command.CommandText = "PRAGMA user_version";
            return Convert.ToInt32(await command.ExecuteScalarAsync());
        }
        finally { if (shouldClose) await connection.CloseAsync(); }
    }

    private static async Task<bool> HasCurrentSchemaAsync(System.Data.Common.DbConnection connection)
    {
        var shouldClose = connection.State != ConnectionState.Open;
        if (shouldClose) await connection.OpenAsync();
        try
        {
            var required = new Dictionary<string, string[]>
            {
                ["ArcaneVaultUsers"] = ["UserName", "Email", "PasswordHash", "IsDeleted", "RoleId"],
                ["ArcaneVaultUserRoles"] = ["RoleId", "RoleName"],
                ["Categories"] = ["CategoryCode", "CategoryName"],
                ["CollectionItems"] = ["ItemId", "ItemName", "Description", "IsDeleted", "StartingQuantity", "CurrentQuantity", "EstimatedUnitValue", "DateAdded", "ImageUrl", "UserName"],
                ["CollectionItemCategories"] = ["ItemId", "CategoryCode"],
                ["AcquisitionRecords"] = ["AcquisitionId", "ItemId", "UserName", "Quantity", "UnitPrice", "PurchaseDate", "PurchaseSource", "Condition", "CreatedAt"],
                ["CollectibleCatalog"] = ["CatalogItemId", "ItemName", "CategoryCode", "Brand", "Series", "ReferenceNumber", "ReleaseYear", "Description", "ImageUrl"]
            };
            foreach (var table in required)
            {
                await using var command = connection.CreateCommand();
                command.CommandText = $"PRAGMA table_info({table.Key})";
                await using var reader = await command.ExecuteReaderAsync();
                var columns = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                while (await reader.ReadAsync()) columns.Add(reader.GetString(1));
                if (!table.Value.All(columns.Contains)) return false;
            }
            return true;
        }
        finally { if (shouldClose) await connection.CloseAsync(); }
    }

    private static async Task SetVersionAsync(System.Data.Common.DbConnection connection, int version)
    {
        var shouldClose = connection.State != ConnectionState.Open;
        if (shouldClose) await connection.OpenAsync();
        try
        {
            await using var command = connection.CreateCommand();
            command.CommandText = $"PRAGMA user_version = {version}";
            await command.ExecuteNonQueryAsync();
        }
        finally { if (shouldClose) await connection.CloseAsync(); }
    }
}
