using Collectly.Models.Entities;

namespace Collectly.Core.Interfaces.Services;

public interface ICollectionService
{
    Task<List<Collection>> GetAllCollectionsAsync();
    Task<List<Collection>> GetActiveCollectionsAsync();
    Task<Collection?> GetCollectionByIdAsync(int id);
    Task<int> CreateCollectionAsync(Collection collection);
    Task<int> UpdateCollectionAsync(Collection collection);
    Task<int> DeleteCollectionAsync(int id);
    Task<int> ArchiveCollectionAsync(int id);
    Task<int> DuplicateCollectionAsync(int id);
    Task<List<Collection>> SearchCollectionsAsync(string query);
}
