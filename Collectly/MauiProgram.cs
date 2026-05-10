using Collectly.Core.Interfaces.Repositories;
using Collectly.Core.Interfaces.Services;
using Collectly.Data.Database;
using Collectly.Data.Migrations;
using Collectly.Data.Repositories;
using Collectly.Services;
using Collectly.Services.Backup;
using Collectly.Services.Logging;
using Collectly.Services.Metadata;
using Collectly.ViewModels;
using Collectly.Views.Pages;
using Microsoft.Extensions.Logging;

namespace Collectly;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        // Database
        builder.Services.AddSingleton<DatabaseService>();

        // Logging
        builder.Services.AddSingleton<IAppLogger, AppLogger>();

        // Migrations
        builder.Services.AddSingleton<IMigrationService, MigrationService>();

        // Repositories
        builder.Services.AddSingleton<ICollectionRepository, CollectionRepository>();
        builder.Services.AddSingleton<IItemRepository, ItemRepository>();
        builder.Services.AddSingleton<ISettingsRepository, SettingsRepository>();

        // Services
        builder.Services.AddSingleton<ICollectionService, CollectionService>();
        builder.Services.AddSingleton<IItemService, ItemService>();
        builder.Services.AddSingleton<IShareReceiverService, ShareReceiverService>();
        builder.Services.AddSingleton<IBackupService, BackupService>();
        builder.Services.AddSingleton<ISecureStorageService, SecureStorageService>();
        builder.Services.AddSingleton(_ => new HttpClient(new HttpClientHandler
        {
            AllowAutoRedirect = true,
            MaxAutomaticRedirections = 10
        }));
        builder.Services.AddSingleton<IMetadataExtractorService, MetadataExtractorService>();

        // ViewModels
        builder.Services.AddTransient<DashboardViewModel>();
        builder.Services.AddTransient<CollectionDetailViewModel>();
        builder.Services.AddTransient<AddEditCollectionViewModel>();
        builder.Services.AddTransient<AddEditItemViewModel>();
        builder.Services.AddTransient<SaveSharedItemViewModel>();
        builder.Services.AddTransient<SettingsViewModel>();
        builder.Services.AddTransient<AboutViewModel>();

        // Pages
        builder.Services.AddTransient<DashboardPage>();
        builder.Services.AddTransient<CollectionDetailPage>();
        builder.Services.AddTransient<AddEditCollectionPage>();
        builder.Services.AddTransient<AddEditItemPage>();
        builder.Services.AddTransient<SaveSharedItemPage>();
        builder.Services.AddTransient<SettingsPage>();
        builder.Services.AddTransient<AboutPage>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}
