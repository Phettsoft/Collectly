using Collectly.Models.Entities;

namespace Collectly.Models.DTOs;

public class BackupData
{
    public string AppVersion { get; set; } = string.Empty;
    public DateTime ExportedAt { get; set; }
    public int SchemaVersion { get; set; }
    public List<Collection> Collections { get; set; } = [];
    public List<SavedItem> Items { get; set; } = [];
    public AppSettings? Settings { get; set; }
}
