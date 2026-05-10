using Collectly.Models.Entities;

namespace Collectly.Core.Interfaces.Services;

public interface IItemService
{
    Task<List<SavedItem>> GetItemsByCollectionAsync(int collectionId);
    Task<SavedItem?> GetItemByIdAsync(int id);
    Task<int> CreateItemAsync(SavedItem item);
    Task<int> UpdateItemAsync(SavedItem item);
    Task<int> DeleteItemAsync(int id);
    Task<int> MoveItemAsync(int itemId, int targetCollectionId);
    Task<int> DuplicateItemAsync(int id, int targetCollectionId);
    Task<bool> IsDuplicateAsync(string url, int collectionId);
    Task<List<SavedItem>> SearchItemsAsync(string query);
}
