namespace Collectly.Core.Interfaces.Services;

public interface IBackupService
{
    Task<string> ExportToJsonAsync();
    Task<bool> ImportFromJsonAsync(string json);
    Task<string> CreateAutomaticBackupAsync();
    Task<List<string>> GetBackupFilesAsync();
}
