using System.Net;
using System.Text.Json;
using System.Text.RegularExpressions;
using Collectly.Core.Constants;
using Collectly.Core.Interfaces.Services;
using Collectly.Models.DTOs;

namespace Collectly.Services.Metadata;

public partial class MetadataExtractorService : IMetadataExtractorService
{
    private readonly HttpClient _httpClient;
    private readonly IAppLogger _logger;

    public MetadataExtractorService(HttpClient httpClient, IAppLogger logger)
    {
        _httpClient = httpClient;
        _httpClient.Timeout = TimeSpan.FromSeconds(AppConstants.MetadataTimeoutSeconds);
        _logger = logger;
    }

    public async Task<ProductMetadata> ExtractAsync(string url, CancellationToken cancellationToken = default)
    {
        var result = new ProductMetadata { Url = url };

        try
        {
            // Fetch page (follows redirects automatically, resolving short URLs)
            var (finalUrl, html) = await FetchWithRedirectsAsync(url, cancellationToken);
            result.Url = finalUrl;

            if (html is null)
            {
                result.ErrorMessage = "Failed to fetch page";
                return result;
            }

            // Detect store from final resolved URL
            result.StoreName = DetectStoreFromUrl(finalUrl);

            // Priority-ordered extraction:
            // 1. JSON-LD (most structured, most reliable)
            ExtractAllJsonLd(html, result);

            // 2. Open Graph meta tags
            ExtractOpenGraph(html, result);

            // 3. Twitter Card meta tags
            ExtractTwitterCard(html, result);

            // 4. Standard meta tags
            ExtractStandardMeta(html, result);

            // 5. Microdata (itemprop attributes)
            ExtractMicrodata(html, result);

            // 6. Site-specific extractors (fallback for stubborn sites)
            ApplySiteSpecificExtractor(finalUrl, html, result);

            // 7. HTML <title> as last resort
            if (string.IsNullOrWhiteSpace(result.Title) || SiteExtractors.IsStoreName(result.Title))
                result.Title = ExtractHtmlTitle(html);

            // 8. Generic price pattern fallback
            if (!result.Price.HasValue)
                ExtractGenericPrice(html, result);

            // 9. Generic image fallback (first large image)
            if (string.IsNullOrWhiteSpace(result.ImageUrl))
                ExtractFallbackImage(html, result);

            // Clean up title
            result.Title = CleanTitle(result.Title, result.StoreName);

            result.ExtractionSucceeded = !string.IsNullOrWhiteSpace(result.Title)
                && !SiteExtractors.IsStoreName(result.Title);

            _logger.Info($"Metadata extracted: Title={result.Title}, Store={result.StoreName}, Price={result.Price}");
        }
        catch (TaskCanceledException)
        {
            result.ErrorMessage = "Request timed out";
            _logger.Warning($"Metadata extraction timed out: {url}");
        }
        catch (Exception ex)
        {
            result.ErrorMessage = ex.Message;
            _logger.Error($"Metadata extraction failed: {url}", ex);
        }

        return result;
    }

    private async Task<(string FinalUrl, string? Html)> FetchWithRedirectsAsync(string url, CancellationToken ct)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, url);
        request.Headers.Add("User-Agent", DesktopUserAgent);
        request.Headers.Add("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8");
        request.Headers.Add("Accept-Language", "en-US,en;q=0.9");
        request.Headers.Add("Cache-Control", "no-cache");
        request.Headers.Add("Sec-Fetch-Dest", "document");
        request.Headers.Add("Sec-Fetch-Mode", "navigate");
        request.Headers.Add("Sec-Fetch-Site", "none");

        var response = await _httpClient.SendAsync(request, ct);
        var finalUrl = response.RequestMessage?.RequestUri?.ToString() ?? url;

        if (!response.IsSuccessStatusCode)
            return (finalUrl, null);

        var html = await response.Content.ReadAsStringAsync(ct);
        return (finalUrl, html);
    }

    private static void ExtractAllJsonLd(string html, ProductMetadata result)
    {
        var matches = JsonLdRegex().Matches(html);
        foreach (Match match in matches)
        {
            try
            {
                var json = match.Groups[1].Value.Trim();
                using var doc = JsonDocument.Parse(json);
                ParseJsonLdRoot(doc.RootElement, result);
            }
            catch { /* best-effort */ }
        }
    }

    private static void ParseJsonLdRoot(JsonElement root, ProductMetadata result)
    {
        if (root.ValueKind == JsonValueKind.Array)
        {
            foreach (var item in root.EnumerateArray())
                ParseJsonLdRoot(item, result);
            return;
        }

        if (root.TryGetProperty("@graph", out var graph))
        {
            ParseJsonLdRoot(graph, result);
            return;
        }

        if (!root.TryGetProperty("@type", out var typeEl)) return;

        var typeStr = typeEl.ValueKind == JsonValueKind.Array
            ? typeEl[0].GetString()
            : typeEl.GetString();

        if (typeStr is null) return;

        if (typeStr.Equals("Product", StringComparison.OrdinalIgnoreCase))
            ExtractJsonLdProduct(root, result);
        else if (typeStr.Contains("Organization", StringComparison.OrdinalIgnoreCase))
            ExtractJsonLdOrganization(root, result);
    }

    private static void ExtractJsonLdProduct(JsonElement root, ProductMetadata result)
    {
        if (root.TryGetProperty("name", out var name))
        {
            var n = name.GetString();
            if (!string.IsNullOrWhiteSpace(n) && !SiteExtractors.IsStoreName(n))
                result.Title ??= n;
        }

        if (root.TryGetProperty("description", out var desc))
            result.Description ??= desc.GetString();

        if (root.TryGetProperty("image", out var image))
            result.ImageUrl ??= ExtractJsonImage(image);

        if (root.TryGetProperty("brand", out var brand))
        {
            if (brand.ValueKind == JsonValueKind.Object && brand.TryGetProperty("name", out var bn))
                result.StoreName ??= bn.GetString();
            else if (brand.ValueKind == JsonValueKind.String)
                result.StoreName ??= brand.GetString();
        }

        if (root.TryGetProperty("offers", out var offers))
            ExtractJsonLdOffers(offers, result);
    }

    private static void ExtractJsonLdOffers(JsonElement offers, ProductMetadata result)
    {
        var offer = offers.ValueKind == JsonValueKind.Array ? offers[0] : offers;

        if (offer.TryGetProperty("price", out var price))
        {
            string? priceStr = price.ValueKind == JsonValueKind.Number
                ? price.GetDecimal().ToString("F2")
                : price.GetString();
            if (decimal.TryParse(priceStr?.Replace(",", ""), out var p) && p > 0)
                result.Price ??= p;
        }

        if (offer.TryGetProperty("priceCurrency", out var currency))
            result.Currency ??= currency.GetString();

        // Some sites put price in lowPrice/highPrice
        if (!result.Price.HasValue && offer.TryGetProperty("lowPrice", out var low))
        {
            string? lowStr = low.ValueKind == JsonValueKind.Number
                ? low.GetDecimal().ToString("F2")
                : low.GetString();
            if (decimal.TryParse(lowStr?.Replace(",", ""), out var lp) && lp > 0)
                result.Price = lp;
        }
    }

    private static void ExtractJsonLdOrganization(JsonElement root, ProductMetadata result)
    {
        if (root.TryGetProperty("name", out var name))
            result.StoreName ??= name.GetString();
    }

    private static string? ExtractJsonImage(JsonElement image)
    {
        return image.ValueKind switch
        {
            JsonValueKind.String => image.GetString(),
            JsonValueKind.Array when image.GetArrayLength() > 0 =>
                image[0].ValueKind == JsonValueKind.String
                    ? image[0].GetString()
                    : image[0].TryGetProperty("url", out var u) ? u.GetString() : null,
            JsonValueKind.Object => image.TryGetProperty("url", out var u) ? u.GetString() : null,
            _ => null
        };
    }

    private static void ExtractOpenGraph(string html, ProductMetadata result)
    {
        var ogTitle = GetMetaContent(html, "og:title");
        if (!string.IsNullOrWhiteSpace(ogTitle) && !SiteExtractors.IsStoreName(ogTitle))
            result.Title ??= ogTitle;

        result.Description ??= GetMetaContent(html, "og:description");
        result.ImageUrl ??= GetMetaContent(html, "og:image");

        var siteName = GetMetaContent(html, "og:site_name");
        if (!string.IsNullOrWhiteSpace(siteName))
            result.StoreName ??= siteName;

        // og:price:amount is used by some stores
        if (!result.Price.HasValue)
        {
            var priceStr = GetMetaContent(html, "og:price:amount")
                ?? GetMetaContent(html, "product:price:amount");
            if (decimal.TryParse(priceStr?.Replace(",", ""), out var p) && p > 0)
                result.Price = p;
        }

        result.Currency ??= GetMetaContent(html, "og:price:currency")
            ?? GetMetaContent(html, "product:price:currency");
    }

    private static void ExtractTwitterCard(string html, ProductMetadata result)
    {
        var tTitle = GetMetaContent(html, "twitter:title");
        if (!string.IsNullOrWhiteSpace(tTitle) && !SiteExtractors.IsStoreName(tTitle))
            result.Title ??= tTitle;

        result.Description ??= GetMetaContent(html, "twitter:description");
        result.ImageUrl ??= GetMetaContent(html, "twitter:image");
        result.ImageUrl ??= GetMetaContent(html, "twitter:image:src");
    }

    private static void ExtractStandardMeta(string html, ProductMetadata result)
    {
        result.Description ??= GetMetaContent(html, "description");
        result.Title ??= GetMetaContent(html, "title");
    }

    private static void ExtractMicrodata(string html, ProductMetadata result)
    {
        // itemprop="name" for product title
        if (string.IsNullOrWhiteSpace(result.Title) || SiteExtractors.IsStoreName(result.Title))
        {
            var match = MicrodataNameRegex().Match(html);
            if (match.Success)
            {
                var val = WebUtility.HtmlDecode(match.Groups[1].Value).Trim();
                if (!SiteExtractors.IsStoreName(val))
                    result.Title = val;
            }
        }

        // itemprop="price"
        if (!result.Price.HasValue)
        {
            var match = MicrodataPriceContentRegex().Match(html);
            if (match.Success && decimal.TryParse(match.Groups[1].Value.Replace(",", ""), out var p) && p > 0)
                result.Price = p;
        }

        // itemprop="image"
        if (string.IsNullOrWhiteSpace(result.ImageUrl))
        {
            var match = MicrodataImageRegex().Match(html);
            if (match.Success)
                result.ImageUrl = match.Groups[1].Value;
        }
    }

    private static void ApplySiteSpecificExtractor(string url, string html, ProductMetadata result)
    {
        var host = GetHost(url);
        if (host is null) return;

        if (host.Contains("amazon"))
            SiteExtractors.ExtractAmazon(html, result);
        else if (host.Contains("walmart"))
            SiteExtractors.ExtractWalmart(html, result);
        else if (host.Contains("target"))
            SiteExtractors.ExtractTarget(html, result);
        else if (host.Contains("etsy"))
            SiteExtractors.ExtractEtsy(html, result);
        else if (host.Contains("ebay"))
            SiteExtractors.ExtractEbay(html, result);
        else if (host.Contains("bestbuy"))
            SiteExtractors.ExtractBestBuy(html, result);
        else if (host.Contains("pinterest") || host.Contains("pin.it"))
            SiteExtractors.ExtractPinterest(html, result);
        else if (host.Contains("temu"))
            SiteExtractors.ExtractTemu(html, result);
        else if (host.Contains("shein"))
            SiteExtractors.ExtractShein(html, result);
        else if (host.Contains("aliexpress"))
            SiteExtractors.ExtractAliExpress(html, result);
        else if (html.Contains("Shopify", StringComparison.OrdinalIgnoreCase))
            SiteExtractors.ExtractShopify(html, result);
    }

    private static void ExtractGenericPrice(string html, ProductMetadata result)
    {
        // Match common price patterns: $19.99, $1,299.00, USD 19.99
        var match = GenericPriceRegex().Match(html);
        if (match.Success)
        {
            var priceStr = match.Groups[1].Value.Replace(",", "");
            if (decimal.TryParse(priceStr, out var p) && p > 0 && p < 100000)
                result.Price = p;
        }
    }

    private static void ExtractFallbackImage(string html, ProductMetadata result)
    {
        // Look for large images (likely product images)
        var matches = LargeImageRegex().Matches(html);
        foreach (Match match in matches)
        {
            var src = match.Groups[1].Value;
            if (string.IsNullOrWhiteSpace(src)) continue;
            // Skip icons, logos, tracking pixels
            if (src.Contains("logo", StringComparison.OrdinalIgnoreCase)) continue;
            if (src.Contains("icon", StringComparison.OrdinalIgnoreCase)) continue;
            if (src.Contains("pixel", StringComparison.OrdinalIgnoreCase)) continue;
            if (src.Contains("1x1", StringComparison.OrdinalIgnoreCase)) continue;
            if (src.Contains("sprite", StringComparison.OrdinalIgnoreCase)) continue;
            if (src.Length < 20) continue;
            result.ImageUrl = src;
            break;
        }
    }

    private static string? ExtractHtmlTitle(string html)
    {
        var match = TitleRegex().Match(html);
        if (!match.Success) return null;
        var title = WebUtility.HtmlDecode(match.Groups[1].Value).Trim();
        return SiteExtractors.IsStoreName(title) ? null : title;
    }

    private static string? CleanTitle(string? title, string? storeName)
    {
        if (string.IsNullOrWhiteSpace(title)) return null;

        // Remove store suffixes: " : Amazon.com", " | Walmart", " - Target", etc.
        string[] separators = [" : ", " | ", " - ", " — ", " – ", " · "];
        foreach (var sep in separators)
        {
            var idx = title.LastIndexOf(sep, StringComparison.OrdinalIgnoreCase);
            if (idx <= 0) continue;
            var suffix = title[(idx + sep.Length)..].Trim();
            if (SiteExtractors.IsStoreName(suffix) ||
                (storeName != null && suffix.Contains(storeName, StringComparison.OrdinalIgnoreCase)))
            {
                title = title[..idx].Trim();
                break;
            }
        }

        // Also check prefix: "Amazon.com: Product Name"
        foreach (var sep in separators)
        {
            var idx = title.IndexOf(sep, StringComparison.OrdinalIgnoreCase);
            if (idx <= 0 || idx > 30) continue;
            var prefix = title[..idx].Trim();
            if (SiteExtractors.IsStoreName(prefix) ||
                (storeName != null && prefix.Contains(storeName, StringComparison.OrdinalIgnoreCase)))
            {
                title = title[(idx + sep.Length)..].Trim();
                break;
            }
        }

        // Remove "Buy ", "Shop " prefixes
        if (title.StartsWith("Buy ", StringComparison.OrdinalIgnoreCase))
            title = title[4..];
        if (title.StartsWith("Shop ", StringComparison.OrdinalIgnoreCase))
            title = title[5..];

        return string.IsNullOrWhiteSpace(title) || SiteExtractors.IsStoreName(title) ? null : title.Trim();
    }

    private static string? DetectStoreFromUrl(string url)
    {
        var host = GetHost(url);
        if (host is null) return null;

        return host switch
        {
            _ when host.Contains("amazon") => "Amazon",
            _ when host.Contains("etsy") => "Etsy",
            _ when host.Contains("walmart") => "Walmart",
            _ when host.Contains("target") => "Target",
            _ when host.Contains("pinterest") || host.Contains("pin.it") => "Pinterest",
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
            _ when host.Contains("macys") => "Macy's",
            _ when host.Contains("nordstrom") => "Nordstrom",
            _ when host.Contains("kohls") => "Kohl's",
            _ when host.Contains("zappos") => "Zappos",
            _ when host.Contains("wayfair") => "Wayfair",
            _ when host.Contains("overstock") => "Overstock",
            _ when host.Contains("newegg") => "Newegg",
            _ when host.Contains("gamestop") => "GameStop",
            _ when host.Contains("apple.com") => "Apple",
            _ when host.Contains("samsung") => "Samsung",
            _ when host.Contains("ikea") => "IKEA",
            _ when host.Contains("zara") => "Zara",
            _ when host.Contains("hm.com") || host.Contains("h&m") => "H&M",
            _ when host.Contains("uniqlo") => "Uniqlo",
            _ when host.Contains("gap.com") || host.Contains("gap.") => "Gap",
            _ when host.Contains("oldnavy") => "Old Navy",
            _ => null
        };
    }

    private static string? GetHost(string url)
    {
        try { return new Uri(url).Host.ToLowerInvariant(); }
        catch { return null; }
    }

    private static string? GetMetaContent(string html, string property)
    {
        // property="x" content="y"
        var pattern = $@"<meta[^>]*(?:property|name)=[""']{Regex.Escape(property)}[""'][^>]*content=[""']([^""']*)[""']";
        var match = Regex.Match(html, pattern, RegexOptions.IgnoreCase | RegexOptions.Singleline);
        if (match.Success) return WebUtility.HtmlDecode(match.Groups[1].Value);

        // content="y" property="x"
        pattern = $@"<meta[^>]*content=[""']([^""']*)[""'][^>]*(?:property|name)=[""']{Regex.Escape(property)}[""']";
        match = Regex.Match(html, pattern, RegexOptions.IgnoreCase | RegexOptions.Singleline);
        return match.Success ? WebUtility.HtmlDecode(match.Groups[1].Value) : null;
    }

    private const string DesktopUserAgent =
        "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/125.0.0.0 Safari/537.36";

    [GeneratedRegex(@"<script[^>]*type=[""']application/ld\+json[""'][^>]*>(.*?)</script>", RegexOptions.Singleline | RegexOptions.IgnoreCase)]
    private static partial Regex JsonLdRegex();

    [GeneratedRegex(@"<title[^>]*>(.*?)</title>", RegexOptions.Singleline | RegexOptions.IgnoreCase)]
    private static partial Regex TitleRegex();

    [GeneratedRegex(@"[\$£€]\s*([\d,]+\.?\d*)", RegexOptions.IgnoreCase)]
    private static partial Regex GenericPriceRegex();

    [GeneratedRegex(@"itemprop=""name""[^>]*>([^<]+)<", RegexOptions.IgnoreCase)]
    private static partial Regex MicrodataNameRegex();

    [GeneratedRegex(@"itemprop=""price""[^>]*content=""([\d,.]+)""", RegexOptions.IgnoreCase)]
    private static partial Regex MicrodataPriceContentRegex();

    [GeneratedRegex(@"itemprop=""image""[^>]*(?:content|src)=""([^""]+)""", RegexOptions.IgnoreCase)]
    private static partial Regex MicrodataImageRegex();

    [GeneratedRegex(@"<img[^>]*src=""(https?://[^""]{50,})""", RegexOptions.IgnoreCase)]
    private static partial Regex LargeImageRegex();
}
