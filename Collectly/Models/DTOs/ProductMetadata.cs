namespace Collectly.Models.DTOs;

public class ProductMetadata
{
    public string? Title { get; set; }
    public string? StoreName { get; set; }
    public string? ImageUrl { get; set; }
    public string? Description { get; set; }
    public decimal? Price { get; set; }
    public string? Currency { get; set; }
    public string? Url { get; set; }
    public bool ExtractionSucceeded { get; set; }
    public string? ErrorMessage { get; set; }
}
