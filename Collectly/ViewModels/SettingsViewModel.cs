using System.Windows.Input;
using Collectly.Core.Constants;
using Collectly.Core.Enums;
using Collectly.Core.Interfaces.Repositories;
using Collectly.Core.Interfaces.Services;
using Collectly.Models.Entities;

namespace Collectly.ViewModels;

public class SettingsViewModel : BaseViewModel
{
    private readonly ISettingsRepository _settingsRepo;
    private readonly IBackupService _backupService;
    private readonly IAppLogger _logger;
    private ThemeMode _themeMode;
    private string _defaultCurrency = "USD";
    private bool _notificationsEnabled = true;
    private bool _autoBackupEnabled = true;
    private bool _debugLoggingEnabled;
    private string _lastBackupDate = "Never";

    public ThemeMode ThemeMode
    {
        get => _themeMode;
        set { if (SetProperty(ref _themeMode, value)) _ = SaveAsync(); }
    }

    public string DefaultCurrency
    {
        get => _defaultCurrency;
        set { if (SetProperty(ref _defaultCurrency, value)) _ = SaveAsync(); }
    }

    public bool NotificationsEnabled
    {
        get => _notificationsEnabled;
        set { if (SetProperty(ref _notificationsEnabled, value)) _ = SaveAsync(); }
    }

    public bool AutoBackupEnabled
    {
        get => _autoBackupEnabled;
        set { if (SetProperty(ref _autoBackupEnabled, value)) _ = SaveAsync(); }
    }

    public bool DebugLoggingEnabled
    {
        get => _debugLoggingEnabled;
        set { if (SetProperty(ref _debugLoggingEnabled, value)) _ = SaveAsync(); }
    }

    public string LastBackupDate { get => _lastBackupDate; set => SetProperty(ref _lastBackupDate, value); }
    public string AppVersionDisplay => $"v{AppVersion.Version}";
    public string BuildDateDisplay => AppVersion.BuildDate;

    public List<ThemeMode> ThemeModes => Enum.GetValues<ThemeMode>().ToList();
    public List<string> Currencies => ["USD", "EUR", "GBP", "CAD", "AUD", "JPY"];

    public ICommand ExportCommand { get; }
    public ICommand ImportCommand { get; }
    public ICommand BackupNowCommand { get; }
    public ICommand LoadCommand { get; }

    public SettingsViewModel(ISettingsRepository settingsRepo, IBackupService backupService, IAppLogger logger)
    {
        _settingsRepo = settingsRepo;
        _backupService = backupService;
        _logger = logger;
        Title = "Settings";

        ExportCommand = CreateCommand(ExportAsync);
        ImportCommand = CreateCommand(ImportAsync);
        BackupNowCommand = CreateCommand(BackupNowAsync);
        LoadCommand = CreateCommand(LoadAsync);
    }

    public async Task LoadAsync()
    {
        var settings = await _settingsRepo.GetSettingsAsync();
        _themeMode = settings.ThemeMode;
        _defaultCurrency = settings.DefaultCurrency;
        _notificationsEnabled = settings.NotificationsEnabled;
        _autoBackupEnabled = settings.AutoBackupEnabled;
        _debugLoggingEnabled = settings.DebugLoggingEnabled;
        LastBackupDate = settings.LastBackupDate?.ToString("g") ?? "Never";

        OnPropertyChanged(nameof(ThemeMode));
        OnPropertyChanged(nameof(DefaultCurrency));
        OnPropertyChanged(nameof(NotificationsEnabled));
        OnPropertyChanged(nameof(AutoBackupEnabled));
        OnPropertyChanged(nameof(DebugLoggingEnabled));
    }

    private async Task SaveAsync()
    {
        var settings = new AppSettings
        {
            ThemeMode = ThemeMode,
            DefaultCurrency = DefaultCurrency,
            NotificationsEnabled = NotificationsEnabled,
            AutoBackupEnabled = AutoBackupEnabled,
            DebugLoggingEnabled = DebugLoggingEnabled
        };
        await _settingsRepo.SaveSettingsAsync(settings);
        ApplyTheme(ThemeMode);
    }

    private async Task ExportAsync()
    {
        try
        {
            var json = await _backupService.ExportToJsonAsync();
            var filename = $"collectly_export_{DateTime.Now:yyyyMMdd_HHmmss}.json";
            var path = Path.Combine(FileSystem.CacheDirectory, filename);
            await File.WriteAllTextAsync(path, json);
            await Share.Default.RequestAsync(new ShareFileRequest
            {
                Title = "Export Collectly Data",
                File = new ShareFile(path)
            });
        }
        catch (Exception ex)
        {
            _logger.Error("Export failed", ex);
            await Shell.Current.DisplayAlertAsync("Error", "Export failed.", "OK");
        }
    }

    private async Task ImportAsync()
    {
        try
        {
            var result = await FilePicker.Default.PickAsync(new PickOptions
            {
                PickerTitle = "Select Collectly Backup",
                FileTypes = new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
                {
                    { DevicePlatform.Android, new[] { "application/json" } },
                    { DevicePlatform.iOS, new[] { "public.json" } },
                    { DevicePlatform.WinUI, new[] { ".json" } }
                })
            });

            if (result is null) return;
            using var stream = await result.OpenReadAsync();
            using var reader = new StreamReader(stream);
            var json = await reader.ReadToEndAsync();

            var success = await _backupService.ImportFromJsonAsync(json);
            await Shell.Current.DisplayAlertAsync(success ? "Success" : "Error",
                success ? "Import completed." : "Import failed.", "OK");
        }
        catch (Exception ex)
        {
            _logger.Error("Import failed", ex);
            await Shell.Current.DisplayAlertAsync("Error", "Import failed.", "OK");
        }
    }

    private async Task BackupNowAsync()
    {
        try
        {
            await _backupService.CreateAutomaticBackupAsync();
            LastBackupDate = DateTime.Now.ToString("g");
            await Shell.Current.DisplayAlertAsync("Success", "Backup created.", "OK");
        }
        catch (Exception ex)
        {
            _logger.Error("Backup failed", ex);
            await Shell.Current.DisplayAlertAsync("Error", "Backup failed.", "OK");
        }
    }

    private static void ApplyTheme(ThemeMode mode)
    {
        if (Application.Current is null) return;
        Application.Current.UserAppTheme = mode switch
        {
            ThemeMode.Light => AppTheme.Light,
            ThemeMode.Dark => AppTheme.Dark,
            _ => AppTheme.Unspecified
        };
    }
}
