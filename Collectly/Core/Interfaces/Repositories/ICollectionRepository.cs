using Collectly.Models.Entities;

namespace Collectly.Core.Interfaces.Repositories;

public interface ICollectionRepository
{
    Task<List<Collection>> GetAllAsync();
    Task<List<Collection>> GetActiveAsync();
    Task<List<Collection>> GetArchivedAsync();
    Task<Collection?> GetByIdAsync(int id);
    Task<int> CreateAsync(Collection collection);
    Task<int> UpdateAsync(Collection collection);
    Task<int> DeleteAsync(int id);
    Task<List<Collection>> SearchAsync(string query);
}
