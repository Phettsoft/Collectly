using Collectly.Core.Interfaces.Repositories;
using Collectly.Core.Interfaces.Services;
using Collectly.Data.Database;
using Collectly.Models.Entities;

namespace Collectly.Data.Repositories;

public class ItemRepository : IItemRepository
{
    private readonly DatabaseService _db;
    private readonly IAppLogger _logger;

    public ItemRepository(DatabaseService db, IAppLogger logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<List<SavedItem>> GetByCollectionIdAsync(int collectionId)
    {
        var conn = await _db.GetConnectionAsync();
        return await conn.Table<SavedItem>()
            .Where(i => i.CollectionId == collectionId)
            .OrderByDescending(i => i.CreatedAt)
            .ToListAsync();
    }

    public async Task<SavedItem?> GetByIdAsync(int id)
    {
        var conn = await _db.GetConnectionAsync();
        return await conn.Table<SavedItem>().FirstOrDefaultAsync(i => i.Id == id);
    }

    public async Task<int> CreateAsync(SavedItem item)
    {
        var conn = await _db.GetConnectionAsync();
        item.CreatedAt = DateTime.UtcNow;
        item.UpdatedAt = DateTime.UtcNow;
        await conn.InsertAsync(item);
        _logger.Info($"Item created: {item.Title} (ID: {item.Id})");
        return item.Id;
    }

    public async Task<int> UpdateAsync(SavedItem item)
    {
        var conn = await _db.GetConnectionAsync();
        item.UpdatedAt = DateTime.UtcNow;
        return await conn.UpdateAsync(item);
    }

    public async Task<int> DeleteAsync(int id)
    {
        var conn = await _db.GetConnectionAsync();
        _logger.Info($"Item deleted: ID {id}");
        return await conn.Table<SavedItem>().DeleteAsync(i => i.Id == id);
    }

    public async Task<List<SavedItem>> SearchAsync(string query)
    {
        var conn = await _db.GetConnectionAsync();
        var all = await conn.Table<SavedItem>().ToListAsync();
        return all.Where(i =>
            i.Title.Contains(query, StringComparison.OrdinalIgnoreCase) ||
            (i.StoreName?.Contains(query, StringComparison.OrdinalIgnoreCase) ?? false) ||
            (i.Tags?.Contains(query, StringComparison.OrdinalIgnoreCase) ?? false))
            .ToList();
    }

    public async Task<bool> ExistsByUrlAsync(string url, int collectionId)
    {
        var conn = await _db.GetConnectionAsync();
        var normalized = NormalizeUrl(url);
        var items = await conn.Table<SavedItem>()
            .Where(i => i.CollectionId == collectionId)
            .ToListAsync();
        return items.Any(i => NormalizeUrl(i.Url ?? "") == normalized);
    }

    public async Task<int> MoveToCollectionAsync(int itemId, int targetCollectionId)
    {
        var conn = await _db.GetConnectionAsync();
        var item = await conn.Table<SavedItem>().FirstOrDefaultAsync(i => i.Id == itemId);
        if (item is null) return 0;
        item.CollectionId = targetCollectionId;
        item.UpdatedAt = DateTime.UtcNow;
        return await conn.UpdateAsync(item);
    }

    private static string NormalizeUrl(string url)
    {
        if (string.IsNullOrWhiteSpace(url)) return string.Empty;
        url = url.Trim().ToLowerInvariant();
        if (url.EndsWith('/')) url = url[..^1];
        // Remove common tracking parameters
        var idx = url.IndexOf('?');
        if (idx > 0) url = url[..idx];
        return url;
    }
}
