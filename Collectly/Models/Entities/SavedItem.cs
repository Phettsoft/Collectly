using SQLite;
using Collectly.Core.Enums;

namespace Collectly.Models.Entities;

[Table("Items")]
public class SavedItem
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    [Indexed]
    public int CollectionId { get; set; }

    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(2048)]
    public string? Url { get; set; }

    [MaxLength(100)]
    public string? StoreName { get; set; }

    [MaxLength(2000)]
    public string? Description { get; set; }

    [MaxLength(2048)]
    public string? ImageUrl { get; set; }

    [MaxLength(500)]
    public string? LocalImagePath { get; set; }

    public decimal? Price { get; set; }
    public decimal? SalePrice { get; set; }

    [MaxLength(10)]
    public string Currency { get; set; } = "USD";

    public int Quantity { get; set; } = 1;
    public ItemPriority Priority { get; set; } = ItemPriority.None;

    [MaxLength(500)]
    public string? Tags { get; set; }

    [MaxLength(2000)]
    public string? Notes { get; set; }

    public bool IsPurchased { get; set; }
    public bool IsWrapped { get; set; }
    public bool IsDelivered { get; set; }
    public bool IsFavorite { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
