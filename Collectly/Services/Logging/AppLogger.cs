using Collectly.Core.Interfaces.Services;
using Collectly.Data.Database;
using Collectly.Models.Entities;

namespace Collectly.Services.Logging;

public class AppLogger : IAppLogger
{
    private readonly DatabaseService _db;

    public AppLogger(DatabaseService db)
    {
        _db = db;
    }

    public void Info(string message) => Log("Info", message);
    public void Warning(string message) => Log("Warning", message);
    public void Error(string message, Exception? exception = null) =>
        Log("Error", message, exception?.ToString());
    public void Debug(string message) => Log("Debug", message);

    private async void Log(string level, string message, string? details = null)
    {
        try
        {
            System.Diagnostics.Debug.WriteLine($"[{level}] {message}");
            var conn = await _db.GetConnectionAsync();
            await conn.InsertAsync(new ActivityLog
            {
                Level = level,
                Message = message.Length > 500 ? message[..500] : message,
                Details = details?.Length > 2000 ? details[..2000] : details,
                Source = "App",
                CreatedAt = DateTime.UtcNow
            });
        }
        catch
        {
            System.Diagnostics.Debug.WriteLine($"[LogError] Failed to persist: {message}");
        }
    }
}
