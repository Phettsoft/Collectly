using Collectly.Core.Interfaces.Repositories;
using Collectly.Core.Interfaces.Services;
using Collectly.Models.Entities;

namespace Collectly.Services;

public class ItemService : IItemService
{
    private readonly IItemRepository _repo;
    private readonly IAppLogger _logger;

    public ItemService(IItemRepository repo, IAppLogger logger)
    {
        _repo = repo;
        _logger = logger;
    }

    public Task<List<SavedItem>> GetItemsByCollectionAsync(int collectionId) =>
        _repo.GetByCollectionIdAsync(collectionId);

    public Task<SavedItem?> GetItemByIdAsync(int id) => _repo.GetByIdAsync(id);

    public async Task<int> CreateItemAsync(SavedItem item)
    {
        if (string.IsNullOrWhiteSpace(item.Title) && string.IsNullOrWhiteSpace(item.Url))
            throw new ArgumentException("Item must have a title or URL.");
        return await _repo.CreateAsync(item);
    }

    public Task<int> UpdateItemAsync(SavedItem item) => _repo.UpdateAsync(item);
    public Task<int> DeleteItemAsync(int id) => _repo.DeleteAsync(id);
    public Task<int> MoveItemAsync(int itemId, int targetCollectionId) =>
        _repo.MoveToCollectionAsync(itemId, targetCollectionId);

    public async Task<int> DuplicateItemAsync(int id, int targetCollectionId)
    {
        var source = await _repo.GetByIdAsync(id);
        if (source is null) return 0;
        var copy = new SavedItem
        {
            CollectionId = targetCollectionId,
            Title = source.Title,
            Url = source.Url,
            StoreName = source.StoreName,
            Description = source.Description,
            ImageUrl = source.ImageUrl,
            Price = source.Price,
            SalePrice = source.SalePrice,
            Currency = source.Currency,
            Quantity = source.Quantity,
            Priority = source.Priority,
            Tags = source.Tags,
            Notes = source.Notes
        };
        return await _repo.CreateAsync(copy);
    }

    public Task<bool> IsDuplicateAsync(string url, int collectionId) =>
        _repo.ExistsByUrlAsync(url, collectionId);

    public Task<List<SavedItem>> SearchItemsAsync(string query) => _repo.SearchAsync(query);
}
