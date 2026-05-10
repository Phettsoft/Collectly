using SQLite;
using Collectly.Core.Constants;
using Collectly.Core.Interfaces.Services;
using Collectly.Data.Database;

namespace Collectly.Data.Migrations;

public class MigrationService : IMigrationService
{
    private readonly DatabaseService _db;
    private readonly IAppLogger _logger;

    public MigrationService(DatabaseService db, IAppLogger logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task RunMigrationsAsync()
    {
        var conn = await _db.GetConnectionAsync();
        await EnsureSchemaVersionTableAsync(conn);

        var currentVersion = await GetCurrentSchemaVersionAsync(conn);
        var targetVersion = AppVersion.DatabaseSchemaVersion;

        if (currentVersion >= targetVersion)
        {
            _logger.Info($"Database schema is up to date (v{currentVersion}).");
            return;
        }

        _logger.Info($"Migrating database from v{currentVersion} to v{targetVersion}...");

        for (int version = currentVersion + 1; version <= targetVersion; version++)
        {
            await ApplyMigrationAsync(conn, version);
            await SetSchemaVersionAsync(conn, version);
            _logger.Info($"Applied migration v{version}.");
        }

        _logger.Info("All migrations applied successfully.");
    }

    public async Task<int> GetCurrentSchemaVersionAsync()
    {
        var conn = await _db.GetConnectionAsync();
        await EnsureSchemaVersionTableAsync(conn);
        return await GetCurrentSchemaVersionAsync(conn);
    }

    private static async Task EnsureSchemaVersionTableAsync(SQLiteAsyncConnection conn)
    {
        await conn.ExecuteAsync("""
            CREATE TABLE IF NOT EXISTS SchemaVersion (
                Id INTEGER PRIMARY KEY CHECK (Id = 1),
                Version INTEGER NOT NULL DEFAULT 1,
                AppliedAt TEXT NOT NULL
            )
            """);

        var count = await conn.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM SchemaVersion");
        if (count == 0)
        {
            await conn.ExecuteAsync(
                "INSERT INTO SchemaVersion (Id, Version, AppliedAt) VALUES (1, 1, ?)",
                DateTime.UtcNow.ToString("O"));
        }
    }

    private static async Task<int> GetCurrentSchemaVersionAsync(SQLiteAsyncConnection conn)
    {
        return await conn.ExecuteScalarAsync<int>("SELECT Version FROM SchemaVersion WHERE Id = 1");
    }

    private static async Task SetSchemaVersionAsync(SQLiteAsyncConnection conn, int version)
    {
        await conn.ExecuteAsync(
            "UPDATE SchemaVersion SET Version = ?, AppliedAt = ? WHERE Id = 1",
            version, DateTime.UtcNow.ToString("O"));
    }

    private async Task ApplyMigrationAsync(SQLiteAsyncConnection conn, int version)
    {
        switch (version)
        {
            case 2:
                await MigrateV2Async(conn);
                break;
            // Add future migrations here:
            // case 3: await MigrateV3Async(conn); break;
            default:
                _logger.Warning($"No migration handler for version {version}.");
                break;
        }
    }

    /// <summary>
    /// Example migration v2: adds Tags table for normalized tag storage.
    /// This will be activated when DatabaseSchemaVersion is bumped to 2.
    /// </summary>
    private static async Task MigrateV2Async(SQLiteAsyncConnection conn)
    {
        await conn.ExecuteAsync("""
            CREATE TABLE IF NOT EXISTS Tags (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Name TEXT NOT NULL UNIQUE COLLATE NOCASE,
                CreatedAt TEXT NOT NULL
            )
            """);

        await conn.ExecuteAsync("""
            CREATE TABLE IF NOT EXISTS ItemTags (
                ItemId INTEGER NOT NULL,
                TagId INTEGER NOT NULL,
                PRIMARY KEY (ItemId, TagId),
                FOREIGN KEY (ItemId) REFERENCES Items(Id) ON DELETE CASCADE,
                FOREIGN KEY (TagId) REFERENCES Tags(Id) ON DELETE CASCADE
            )
            """);
    }
}
