using SQLite;
using Collectly.Core.Enums;

namespace Collectly.Models.Entities;

[Table("Settings")]
public class AppSettings
{
    [PrimaryKey]
    public int Id { get; set; } = 1;

    public ThemeMode ThemeMode { get; set; } = ThemeMode.System;

    [MaxLength(10)]
    public string DefaultCurrency { get; set; } = "USD";

    public bool NotificationsEnabled { get; set; } = true;
    public bool AutoBackupEnabled { get; set; } = true;
    public bool DebugLoggingEnabled { get; set; }

    public DateTime? LastBackupDate { get; set; }
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
