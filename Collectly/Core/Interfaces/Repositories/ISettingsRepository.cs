using Collectly.Models.Entities;

namespace Collectly.Core.Interfaces.Repositories;

public interface ISettingsRepository
{
    Task<AppSettings> GetSettingsAsync();
    Task<int> SaveSettingsAsync(AppSettings settings);
}
