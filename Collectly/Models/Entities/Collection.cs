using SQLite;
using Collectly.Core.Enums;

namespace Collectly.Models.Entities;

[Table("Collections")]
public class Collection
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? RecipientName { get; set; }

    public EventType EventType { get; set; } = EventType.None;

    public DateTime? EventDate { get; set; }

    [MaxLength(2000)]
    public string? Notes { get; set; }

    [MaxLength(20)]
    public string ThemeColor { get; set; } = "#6366F1";

    [MaxLength(50)]
    public string Icon { get; set; } = "📦";

    public bool IsPinned { get; set; }
    public bool IsPrivate { get; set; }
    public bool IsShared { get; set; }
    public bool IsArchived { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    [Ignore]
    public int ItemCount { get; set; }
}
