using Collectly.Core.Interfaces.Services;

namespace Collectly.Services;

public class SecureStorageService : ISecureStorageService
{
    private readonly IAppLogger _logger;

    public SecureStorageService(IAppLogger logger)
    {
        _logger = logger;
    }

    public async Task<string?> GetAsync(string key)
    {
        try
        {
            return await SecureStorage.Default.GetAsync(key);
        }
        catch (Exception ex)
        {
            _logger.Error($"SecureStorage read failed for key '{key}'", ex);
            return null;
        }
    }

    public async Task SetAsync(string key, string value)
    {
        try
        {
            await SecureStorage.Default.SetAsync(key, value);
        }
        catch (Exception ex)
        {
            _logger.Error($"SecureStorage write failed for key '{key}'", ex);
        }
    }

    public Task RemoveAsync(string key)
    {
        try
        {
            SecureStorage.Default.Remove(key);
        }
        catch (Exception ex)
        {
            _logger.Error($"SecureStorage remove failed for key '{key}'", ex);
        }
        return Task.CompletedTask;
    }

    public Task ClearAllAsync()
    {
        try
        {
            SecureStorage.Default.RemoveAll();
        }
        catch (Exception ex)
        {
            _logger.Error("SecureStorage clear failed", ex);
        }
        return Task.CompletedTask;
    }
}
