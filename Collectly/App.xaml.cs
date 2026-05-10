using Collectly.Core.Helpers;
using Collectly.Core.Interfaces.Repositories;
using Collectly.Core.Interfaces.Services;

namespace Collectly;

public partial class App : Application
{
    private readonly IAppLogger _logger;
    private readonly ISettingsRepository _settingsRepo;
    private readonly IMigrationService _migrationService;

    public App(IAppLogger logger, ISettingsRepository settingsRepo, IMigrationService migrationService)
    {
        try
        {
            System.Diagnostics.Debug.WriteLine("[App] Constructor starting...");
            InitializeComponent();
            _logger = logger;
            _settingsRepo = settingsRepo;
            _migrationService = migrationService;

            GlobalExceptionHandler.Initialize(logger);
            System.Diagnostics.Debug.WriteLine("[App] Constructor complete.");
            _ = InitializeAsync();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[App] FATAL: {ex}");
            Console.WriteLine($"[App] FATAL: {ex}");
            throw;
        }
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        try
        {
            System.Diagnostics.Debug.WriteLine("[App] CreateWindow starting...");
            var shell = new AppShell();
            System.Diagnostics.Debug.WriteLine("[App] AppShell created.");
            return new Window(shell);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[App] CreateWindow FATAL: {ex}");
            Console.WriteLine($"[App] CreateWindow FATAL: {ex}");
            throw;
        }
    }

    private async Task InitializeAsync()
    {
        try
        {
            await Task.Delay(100); // Allow UI to render first
            await _migrationService.RunMigrationsAsync();
            await MainThread.InvokeOnMainThreadAsync(ApplySavedThemeAsync);
        }
        catch (Exception ex)
        {
            _logger.Error("App initialization failed", ex);
        }
    }

    private async Task ApplySavedThemeAsync()
    {
        try
        {
            var settings = await _settingsRepo.GetSettingsAsync();
            UserAppTheme = settings.ThemeMode switch
            {
                Core.Enums.ThemeMode.Light => AppTheme.Light,
                Core.Enums.ThemeMode.Dark => AppTheme.Dark,
                _ => AppTheme.Unspecified
            };
        }
        catch (Exception ex)
        {
            _logger.Error("Failed to apply saved theme", ex);
        }
    }
}
