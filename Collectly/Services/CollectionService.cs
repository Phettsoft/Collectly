using Collectly.Core.Interfaces.Repositories;
using Collectly.Core.Interfaces.Services;
using Collectly.Models.Entities;

namespace Collectly.Services;

public class CollectionService : ICollectionService
{
    private readonly ICollectionRepository _repo;
    private readonly IAppLogger _logger;

    public CollectionService(ICollectionRepository repo, IAppLogger logger)
    {
        _repo = repo;
        _logger = logger;
    }

    public Task<List<Collection>> GetAllCollectionsAsync() => _repo.GetAllAsync();
    public Task<List<Collection>> GetActiveCollectionsAsync() => _repo.GetActiveAsync();
    public Task<Collection?> GetCollectionByIdAsync(int id) => _repo.GetByIdAsync(id);

    public async Task<int> CreateCollectionAsync(Collection collection)
    {
        if (string.IsNullOrWhiteSpace(collection.Name))
            throw new ArgumentException("Collection name is required.");
        return await _repo.CreateAsync(collection);
    }

    public Task<int> UpdateCollectionAsync(Collection collection)
    {
        if (string.IsNullOrWhiteSpace(collection.Name))
            throw new ArgumentException("Collection name is required.");
        return _repo.UpdateAsync(collection);
    }

    public Task<int> DeleteCollectionAsync(int id) => _repo.DeleteAsync(id);

    public async Task<int> ArchiveCollectionAsync(int id)
    {
        var collection = await _repo.GetByIdAsync(id);
        if (collection is null) return 0;
        collection.IsArchived = !collection.IsArchived;
        return await _repo.UpdateAsync(collection);
    }

    public async Task<int> DuplicateCollectionAsync(int id)
    {
        var source = await _repo.GetByIdAsync(id);
        if (source is null) return 0;
        var copy = new Collection
        {
            Name = $"{source.Name} (Copy)",
            RecipientName = source.RecipientName,
            EventType = source.EventType,
            EventDate = source.EventDate,
            Notes = source.Notes,
            ThemeColor = source.ThemeColor,
            Icon = source.Icon,
            IsPrivate = source.IsPrivate
        };
        return await _repo.CreateAsync(copy);
    }

    public Task<List<Collection>> SearchCollectionsAsync(string query) => _repo.SearchAsync(query);
}
