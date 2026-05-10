using SQLite;

namespace Collectly.Models.Entities;

[Table("ActivityLogs")]
public class ActivityLog
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    [MaxLength(50)]
    public string Level { get; set; } = "Info";

    [MaxLength(500)]
    public string Message { get; set; } = string.Empty;

    [MaxLength(2000)]
    public string? Details { get; set; }

    [MaxLength(100)]
    public string? Source { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
