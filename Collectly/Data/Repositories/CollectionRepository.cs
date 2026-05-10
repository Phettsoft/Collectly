using Collectly.Core.Interfaces.Repositories;
using Collectly.Core.Interfaces.Services;
using Collectly.Data.Database;
using Collectly.Models.Entities;

namespace Collectly.Data.Repositories;

public class CollectionRepository : ICollectionRepository
{
    private readonly DatabaseService _db;
    private readonly IAppLogger _logger;

    public CollectionRepository(DatabaseService db, IAppLogger logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<List<Collection>> GetAllAsync()
    {
        var conn = await _db.GetConnectionAsync();
        var collections = await conn.Table<Collection>().ToListAsync();
        await PopulateItemCounts(conn, collections);
        return collections;
    }

    public async Task<List<Collection>> GetActiveAsync()
    {
        var conn = await _db.GetConnectionAsync();
        var collections = await conn.Table<Collection>()
            .Where(c => !c.IsArchived)
            .OrderByDescending(c => c.IsPinned)
            .ToListAsync();
        await PopulateItemCounts(conn, collections);
        return collections;
    }

    public async Task<List<Collection>> GetArchivedAsync()
    {
        var conn = await _db.GetConnectionAsync();
        var collections = await conn.Table<Collection>()
            .Where(c => c.IsArchived)
            .ToListAsync();
        await PopulateItemCounts(conn, collections);
        return collections;
    }

    public async Task<Collection?> GetByIdAsync(int id)
    {
        var conn = await _db.GetConnectionAsync();
        return await conn.Table<Collection>().FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<int> CreateAsync(Collection collection)
    {
        var conn = await _db.GetConnectionAsync();
        collection.CreatedAt = DateTime.UtcNow;
        collection.UpdatedAt = DateTime.UtcNow;
        await conn.InsertAsync(collection);
        _logger.Info($"Collection created: {collection.Name} (ID: {collection.Id})");
        return collection.Id;
    }

    public async Task<int> UpdateAsync(Collection collection)
    {
        var conn = await _db.GetConnectionAsync();
        collection.UpdatedAt = DateTime.UtcNow;
        _logger.Info($"Collection updated: {collection.Name} (ID: {collection.Id})");
        return await conn.UpdateAsync(collection);
    }

    public async Task<int> DeleteAsync(int id)
    {
        var conn = await _db.GetConnectionAsync();
        await conn.Table<SavedItem>().DeleteAsync(i => i.CollectionId == id);
        _logger.Info($"Collection deleted: ID {id}");
        return await conn.Table<Collection>().DeleteAsync(c => c.Id == id);
    }

    public async Task<List<Collection>> SearchAsync(string query)
    {
        var conn = await _db.GetConnectionAsync();
        var lower = query.ToLowerInvariant();
        var all = await conn.Table<Collection>().ToListAsync();
        return all.Where(c =>
            c.Name.Contains(lower, StringComparison.OrdinalIgnoreCase) ||
            (c.RecipientName?.Contains(lower, StringComparison.OrdinalIgnoreCase) ?? false))
            .ToList();
    }

    private static async Task PopulateItemCounts(SQLite.SQLiteAsyncConnection conn, List<Collection> collections)
    {
        foreach (var c in collections)
        {
            c.ItemCount = await conn.Table<SavedItem>().CountAsync(i => i.CollectionId == c.Id);
        }
    }
}
