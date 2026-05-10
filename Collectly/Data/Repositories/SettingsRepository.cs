using Collectly.Core.Interfaces.Repositories;
using Collectly.Data.Database;
using Collectly.Models.Entities;

namespace Collectly.Data.Repositories;

public class SettingsRepository : ISettingsRepository
{
    private readonly DatabaseService _db;

    public SettingsRepository(DatabaseService db)
    {
        _db = db;
    }

    public async Task<AppSettings> GetSettingsAsync()
    {
        var conn = await _db.GetConnectionAsync();
        var settings = await conn.Table<AppSettings>().FirstOrDefaultAsync();
        return settings ?? new AppSettings();
    }

    public async Task<int> SaveSettingsAsync(AppSettings settings)
    {
        var conn = await _db.GetConnectionAsync();
        settings.UpdatedAt = DateTime.UtcNow;
        return await conn.InsertOrReplaceAsync(settings);
    }
}
