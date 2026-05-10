namespace Collectly.Models.DTOs;

public class SharedContent
{
    public string? Title { get; set; }
    public string? Url { get; set; }
    public string? Text { get; set; }
    public string? ImageUri { get; set; }
    public string? StoreName { get; set; }
    public bool HasUrl => !string.IsNullOrWhiteSpace(Url);
    public bool HasImage => !string.IsNullOrWhiteSpace(ImageUri);
}
