using SQLite;
using Collectly.Core.Constants;
using Collectly.Models.Entities;

namespace Collectly.Data.Database;

public class DatabaseService
{
    private SQLiteAsyncConnection? _database;
    private readonly SemaphoreSlim _initLock = new(1, 1);
    private bool _initialized;

    public async Task<SQLiteAsyncConnection> GetConnectionAsync()
    {
        if (_database is not null && _initialized)
            return _database;

        await _initLock.WaitAsync();
        try
        {
            if (_database is not null && _initialized)
                return _database;

            _database = new SQLiteAsyncConnection(AppConstants.DatabasePath,
                SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.Create | SQLiteOpenFlags.SharedCache);

            await InitializeTablesAsync();
            _initialized = true;
            System.Diagnostics.Debug.WriteLine("[DatabaseService] Database initialized successfully.");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[DatabaseService] Failed to initialize database: {ex.Message}");
            throw;
        }
        finally
        {
            _initLock.Release();
        }

        return _database;
    }

    private async Task InitializeTablesAsync()
    {
        if (_database is null) return;

        await _database.CreateTableAsync<Collection>();
        await _database.CreateTableAsync<SavedItem>();
        await _database.CreateTableAsync<AppSettings>();
        await _database.CreateTableAsync<ActivityLog>();

        var settings = await _database.Table<AppSettings>().FirstOrDefaultAsync();
        if (settings is null)
        {
            await _database.InsertAsync(new AppSettings());
        }
    }
}
