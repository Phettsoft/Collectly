using Collectly.Core.Interfaces.Services;

namespace Collectly.Core.Helpers;

public static class GlobalExceptionHandler
{
    private static IAppLogger? _logger;

    public static void Initialize(IAppLogger logger)
    {
        _logger = logger;

        AppDomain.CurrentDomain.UnhandledException += (_, args) =>
        {
            var ex = args.ExceptionObject as Exception;
            _logger?.Error($"Unhandled domain exception: {ex?.Message}", ex);
        };

        TaskScheduler.UnobservedTaskException += (_, args) =>
        {
            _logger?.Error($"Unobserved task exception: {args.Exception.Message}", args.Exception);
            args.SetObserved();
        };
    }
}
