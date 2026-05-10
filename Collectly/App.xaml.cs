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
        InitializeComponent();
        _logger = logger;
        _settingsRepo = settingsRepo;
        _migrationService = migrationService;

        GlobalExceptionHandler.Initialize(logger);
        _ = InitializeAsync();
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        return new Window(new AppShell());
    }

    private async Task InitializeAsync()
    {
        try
        {
            await _migrationService.RunMigrationsAsync();
            await ApplySavedThemeAsync();
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
