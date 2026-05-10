using Collectly.Models.Entities;

namespace Collectly.Core.Interfaces.Repositories;

public interface IItemRepository
{
    Task<List<SavedItem>> GetByCollectionIdAsync(int collectionId);
    Task<SavedItem?> GetByIdAsync(int id);
    Task<int> CreateAsync(SavedItem item);
    Task<int> UpdateAsync(SavedItem item);
    Task<int> DeleteAsync(int id);
    Task<List<SavedItem>> SearchAsync(string query);
    Task<bool> ExistsByUrlAsync(string url, int collectionId);
    Task<int> MoveToCollectionAsync(int itemId, int targetCollectionId);
}
