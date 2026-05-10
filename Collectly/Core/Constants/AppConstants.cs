namespace Collectly.Core.Constants;

public static class AppConstants
{
    public const string DatabaseFilename = "collectly.db3";
    public const int MaxTitleLength = 200;
    public const int MaxNotesLength = 2000;
    public const int MaxUrlLength = 2048;
    public const int MetadataTimeoutSeconds = 15;
    public const int MaxRetryAttempts = 3;
    public const string DefaultCurrency = "USD";
    public const string BackupFileExtension = ".collectly.json";
    public const string ShareIntentAction = "com.collectly.SHARE";

    public static string DatabasePath =>
        Path.Combine(FileSystem.AppDataDirectory, DatabaseFilename);

    public static string BackupDirectory =>
        Path.Combine(FileSystem.AppDataDirectory, "Backups");

    public static string LogDirectory =>
        Path.Combine(FileSystem.AppDataDirectory, "Logs");
}
