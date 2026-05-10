namespace Collectly.Core.Interfaces.Services;

public interface IMigrationService
{
    Task RunMigrationsAsync();
    Task<int> GetCurrentSchemaVersionAsync();
}
