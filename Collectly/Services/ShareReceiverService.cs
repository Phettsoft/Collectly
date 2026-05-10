using System.Text.RegularExpressions;
using Collectly.Core.Interfaces.Services;
using Collectly.Models.DTOs;

namespace Collectly.Services;

public partial class ShareReceiverService : IShareReceiverService
{
    private readonly IAppLogger _logger;

    public ShareReceiverService(IAppLogger logger)
    {
        _logger = logger;
    }

    public Task<SharedContent> ProcessSharedContentAsync(string? text, string? url, string? imageUri)
    {
        var content = new SharedContent { ImageUri = imageUri };

        // Extract URL from text if no direct URL provided
        if (!string.IsNullOrWhiteSpace(url))
        {
            content.Url = SanitizeUrl(url);
        }
        else if (!string.IsNullOrWhiteSpace(text))
        {
            var extracted = ExtractUrl(text);
            if (extracted is not null)
            {
                content.Url = SanitizeUrl(extracted);
                content.Text = text.Replace(extracted, "").Trim();
            }
            else
            {
                content.Text = text;
            }
        }

        if (content.HasUrl)
        {
            content.StoreName = DetectStore(content.Url!);
            content.Title = GenerateDefaultTitle(content.Url!, content.StoreName);
        }
        else if (!string.IsNullOrWhiteSpace(content.Text))
        {
            content.Title = content.Text.Length > 100 ? content.Text[..100] : content.Text;
        }

        _logger.Info($"Processed shared content: URL={content.Url ?? "none"}, Store={content.StoreName ?? "unknown"}");
        return Task.FromResult(content);
    }

    private static string SanitizeUrl(string url)
    {
        url = url.Trim();
        if (!url.StartsWith("http://") && !url.StartsWith("https://"))
            url = "https://" + url;
        return url;
    }

    private static string? ExtractUrl(string text)
    {
        var match = UrlRegex().Match(text);
        return match.Success ? match.Value : null;
    }

    private static string? DetectStore(string url)
    {
        var host = new Uri(url).Host.ToLowerInvariant();
        return host switch
        {
            _ when host.Contains("amazon") => "Amazon",
            _ when host.Contains("etsy") => "Etsy",
            _ when host.Contains("walmart") => "Walmart",
            _ when host.Contains("target") => "Target",
            _ when host.Contains("pinterest") || host.Contains("pin.it") => "Pinterest",
            _ when host.Contains("shopify") => "Shopify",
            _ when host.Contains("ebay") => "eBay",
            _ when host.Contains("bestbuy") => "Best Buy",
            _ when host.Contains("homedepot") => "Home Depot",
            _ when host.Contains("lowes") => "Lowe's",
            _ when host.Contains("costco") => "Costco",
            _ when host.Contains("temu") => "Temu",
            _ when host.Contains("shein") => "Shein",
            _ when host.Contains("aliexpress") => "AliExpress",
            _ when host.Contains("nike") => "Nike",
            _ when host.Contains("adidas") => "Adidas",
            _ => null
        };
    }

    private static string GenerateDefaultTitle(string url, string? storeName)
    {
        if (storeName is not null)
            return $"Item from {storeName}";
        try
        {
            return new Uri(url).Host.Replace("www.", "");
        }
        catch
        {
            return "Saved Item";
        }
    }

    [GeneratedRegex(@"https?://[^\s<>""']+", RegexOptions.IgnoreCase)]
    private static partial Regex UrlRegex();
}
