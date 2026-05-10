using System.Text.Json;
using Collectly.Core.Constants;
using Collectly.Core.Interfaces.Repositories;
using Collectly.Core.Interfaces.Services;
using Collectly.Models.DTOs;
using Collectly.Models.Entities;

namespace Collectly.Services.Backup;

public class BackupService : IBackupService
{
    private readonly ICollectionRepository _collectionRepo;
    private readonly IItemRepository _itemRepo;
    private readonly ISettingsRepository _settingsRepo;
    private readonly IAppLogger _logger;

    public BackupService(
        ICollectionRepository collectionRepo,
        IItemRepository itemRepo,
        ISettingsRepository settingsRepo,
        IAppLogger logger)
    {
        _collectionRepo = collectionRepo;
        _itemRepo = itemRepo;
        _settingsRepo = settingsRepo;
        _logger = logger;
    }

    public async Task<string> ExportToJsonAsync()
    {
        var collections = await _collectionRepo.GetAllAsync();
        var items = new List<SavedItem>();
        foreach (var c in collections)
        {
            var collectionItems = await _itemRepo.GetByCollectionIdAsync(c.Id);
            items.AddRange(collectionItems);
        }

        var backup = new BackupData
        {
            AppVersion = AppVersion.Version,
            ExportedAt = DateTime.UtcNow,
            SchemaVersion = AppVersion.DatabaseSchemaVersion,
            Collections = collections,
            Items = items,
            Settings = await _settingsRepo.GetSettingsAsync()
        };

        var json = JsonSerializer.Serialize(backup, new JsonSerializerOptions { WriteIndented = true });
        _logger.Info($"Export completed: {collections.Count} collections, {items.Count} items");
        return json;
    }

    public async Task<bool> ImportFromJsonAsync(string json)
    {
        try
        {
            var backup = JsonSerializer.Deserialize<BackupData>(json);
            if (!ValidateBackup(backup))
                return false;

            var idMap = new Dictionary<int, int>();

            foreach (var c in backup!.Collections)
            {
                var oldId = c.Id;
                c.Id = 0;
                c.Name = SanitizeString(c.Name, AppConstants.MaxTitleLength) ?? c.Name;
                c.Notes = SanitizeString(c.Notes, AppConstants.MaxNotesLength);
                var newId = await _collectionRepo.CreateAsync(c);
                idMap[oldId] = newId;
            }

            foreach (var item in backup.Items)
            {
                item.Id = 0;
                if (idMap.TryGetValue(item.CollectionId, out var newCollId))
                    item.CollectionId = newCollId;
                item.Title = SanitizeString(item.Title, AppConstants.MaxTitleLength) ?? "Imported Item";
                item.Url = SanitizeString(item.Url, AppConstants.MaxUrlLength);
                item.Notes = SanitizeString(item.Notes, AppConstants.MaxNotesLength);
                await _itemRepo.CreateAsync(item);
            }

            _logger.Info($"Import completed: {backup.Collections.Count} collections, {backup.Items.Count} items");
            return true;
        }
        catch (JsonException ex)
        {
            _logger.Error("Import failed: malformed JSON", ex);
            return false;
        }
        catch (Exception ex)
        {
            _logger.Error("Import failed", ex);
            return false;
        }
    }

    private bool ValidateBackup(BackupData? backup)
    {
        if (backup is null)
        {
            _logger.Error("Import failed: invalid JSON structure");
            return false;
        }

        if (string.IsNullOrWhiteSpace(backup.AppVersion))
        {
            _logger.Error("Import failed: missing app version in backup");
            return false;
        }

        if (backup.SchemaVersion > AppVersion.DatabaseSchemaVersion)
        {
            _logger.Error($"Import failed: backup schema v{backup.SchemaVersion} is newer than app schema v{AppVersion.DatabaseSchemaVersion}");
            return false;
        }

        if (backup.Collections is null || backup.Items is null)
        {
            _logger.Error("Import failed: null collections or items array");
            return false;
        }

        return true;
    }

    private static string? SanitizeString(string? value, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;
        return value.Length > maxLength ? value[..maxLength] : value;
    }

    public async Task<string> CreateAutomaticBackupAsync()
    {
        if (!Directory.Exists(AppConstants.BackupDirectory))
            Directory.CreateDirectory(AppConstants.BackupDirectory);

        var json = await ExportToJsonAsync();
        var filename = $"backup_{DateTime.Now:yyyyMMdd_HHmmss}{AppConstants.BackupFileExtension}";
        var path = Path.Combine(AppConstants.BackupDirectory, filename);
        await File.WriteAllTextAsync(path, json);
        _logger.Info($"Automatic backup created: {filename}");
        return path;
    }

    public Task<List<string>> GetBackupFilesAsync()
    {
        if (!Directory.Exists(AppConstants.BackupDirectory))
            return Task.FromResult(new List<string>());

        var files = Directory.GetFiles(AppConstants.BackupDirectory, $"*{AppConstants.BackupFileExtension}")
            .OrderByDescending(f => f)
            .ToList();
        return Task.FromResult(files);
    }
}
