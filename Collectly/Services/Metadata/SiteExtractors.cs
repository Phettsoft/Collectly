using System.Net;
using System.Text.RegularExpressions;
using Collectly.Models.DTOs;

namespace Collectly.Services.Metadata;

/// <summary>
/// Site-specific extraction strategies for stores that require custom parsing.
/// </summary>
public static partial class SiteExtractors
{
    public static void ExtractAmazon(string html, ProductMetadata result)
    {
        // Product title from #productTitle
        if (string.IsNullOrWhiteSpace(result.Title) || IsStoreName(result.Title))
        {
            var match = AmazonTitleRegex().Match(html);
            if (match.Success)
            {
                var title = WebUtility.HtmlDecode(match.Groups[1].Value).Trim();
                if (!string.IsNullOrWhiteSpace(title))
                    result.Title = title;
            }
        }

        // Price: whole + fraction
        if (!result.Price.HasValue)
        {
            var match = AmazonPriceWholeRegex().Match(html);
            if (match.Success)
            {
                var whole = match.Groups[1].Value.Replace(",", "");
                var fractionMatch = AmazonPriceFractionRegex().Match(html[match.Index..]);
                var fraction = fractionMatch.Success ? fractionMatch.Groups[1].Value : "00";
                if (decimal.TryParse($"{whole}.{fraction}", out var p))
                    result.Price = p;
            }
        }

        // Fallback price from data attribute
        if (!result.Price.HasValue)
        {
            var match = AmazonDataPriceRegex().Match(html);
            if (match.Success && decimal.TryParse(match.Groups[1].Value, out var dp))
                result.Price = dp;
        }

        // Main image from landing image or data-a-dynamic-image
        if (string.IsNullOrWhiteSpace(result.ImageUrl))
        {
            var match = AmazonLandingImageRegex().Match(html);
            if (match.Success)
                result.ImageUrl = match.Groups[1].Value;
        }
        if (string.IsNullOrWhiteSpace(result.ImageUrl))
        {
            var match = AmazonDynamicImageRegex().Match(html);
            if (match.Success)
                result.ImageUrl = match.Groups[1].Value;
        }

        result.StoreName ??= "Amazon";
    }

    public static void ExtractWalmart(string html, ProductMetadata result)
    {
        if (string.IsNullOrWhiteSpace(result.Title) || IsStoreName(result.Title))
        {
            var match = WalmartTitleRegex().Match(html);
            if (match.Success)
                result.Title = WebUtility.HtmlDecode(match.Groups[1].Value).Trim();
        }

        if (!result.Price.HasValue)
        {
            var match = WalmartPriceRegex().Match(html);
            if (match.Success && decimal.TryParse(match.Groups[1].Value, out var p))
                result.Price = p;
        }

        result.StoreName ??= "Walmart";
    }

    public static void ExtractTarget(string html, ProductMetadata result)
    {
        if (string.IsNullOrWhiteSpace(result.Title) || IsStoreName(result.Title))
        {
            var match = TargetTitleRegex().Match(html);
            if (match.Success)
                result.Title = WebUtility.HtmlDecode(match.Groups[1].Value).Trim();
        }

        result.StoreName ??= "Target";
    }

    public static void ExtractEtsy(string html, ProductMetadata result)
    {
        if (string.IsNullOrWhiteSpace(result.Title) || IsStoreName(result.Title))
        {
            var match = EtsyTitleRegex().Match(html);
            if (match.Success)
                result.Title = WebUtility.HtmlDecode(match.Groups[1].Value).Trim();
        }

        result.StoreName ??= "Etsy";
    }

    public static void ExtractEbay(string html, ProductMetadata result)
    {
        if (string.IsNullOrWhiteSpace(result.Title) || IsStoreName(result.Title))
        {
            var match = EbayTitleRegex().Match(html);
            if (match.Success)
                result.Title = WebUtility.HtmlDecode(match.Groups[1].Value).Trim();
        }

        if (!result.Price.HasValue)
        {
            var match = EbayPriceRegex().Match(html);
            if (match.Success && decimal.TryParse(match.Groups[1].Value, out var p))
                result.Price = p;
        }

        result.StoreName ??= "eBay";
    }

    public static void ExtractBestBuy(string html, ProductMetadata result)
    {
        if (string.IsNullOrWhiteSpace(result.Title) || IsStoreName(result.Title))
        {
            var match = BestBuyTitleRegex().Match(html);
            if (match.Success)
                result.Title = WebUtility.HtmlDecode(match.Groups[1].Value).Trim();
        }

        result.StoreName ??= "Best Buy";
    }

    public static void ExtractPinterest(string html, ProductMetadata result)
    {
        // Pinterest pins: get the pin description as title
        if (string.IsNullOrWhiteSpace(result.Title) || IsStoreName(result.Title))
        {
            var match = PinterestDescRegex().Match(html);
            if (match.Success)
            {
                var desc = WebUtility.HtmlDecode(match.Groups[1].Value).Trim();
                if (desc.Length > 150) desc = desc[..150];
                result.Title = desc;
            }
        }

        result.StoreName ??= "Pinterest";
    }

    public static void ExtractShopify(string html, ProductMetadata result)
    {
        // Shopify stores use standard JSON-LD, but fallback to product-single__title
        if (string.IsNullOrWhiteSpace(result.Title) || IsStoreName(result.Title))
        {
            var match = ShopifyTitleRegex().Match(html);
            if (match.Success)
                result.Title = WebUtility.HtmlDecode(match.Groups[1].Value).Trim();
        }
    }

    public static void ExtractTemu(string html, ProductMetadata result)
    {
        result.StoreName ??= "Temu";
    }

    public static void ExtractShein(string html, ProductMetadata result)
    {
        result.StoreName ??= "Shein";
    }

    public static void ExtractAliExpress(string html, ProductMetadata result)
    {
        result.StoreName ??= "AliExpress";
    }

    /// <summary>
    /// Determines if a title is just a store/site name and not a real product title.
    /// </summary>
    public static bool IsStoreName(string? title)
    {
        if (string.IsNullOrWhiteSpace(title)) return true;
        var lower = title.ToLowerInvariant().Trim();
        string[] genericNames =
        [
            "amazon", "amazon.com", "amazon.co.uk", "amazon.ca",
            "walmart", "walmart.com", "target", "target.com",
            "ebay", "ebay.com", "etsy", "etsy.com",
            "pinterest", "pinterest.com", "pin.it",
            "best buy", "bestbuy.com", "home depot", "homedepot.com",
            "lowe's", "lowes.com", "costco", "costco.com",
            "temu", "temu.com", "shein", "shein.com",
            "aliexpress", "aliexpress.com", "nike", "nike.com",
            "adidas", "adidas.com", "shopify", "shop",
            "sign in", "log in", "page not found", "404"
        ];
        return genericNames.Contains(lower) || lower.Length < 3;
    }

    // Amazon
    [GeneratedRegex(@"id=""productTitle""[^>]*>\s*([^<]+)", RegexOptions.IgnoreCase)]
    private static partial Regex AmazonTitleRegex();

    [GeneratedRegex(@"class=""a-price-whole""[^>]*>([\\d,]+)", RegexOptions.IgnoreCase)]
    private static partial Regex AmazonPriceWholeRegex();

    [GeneratedRegex(@"class=""a-price-fraction""[^>]*>(\d+)", RegexOptions.IgnoreCase)]
    private static partial Regex AmazonPriceFractionRegex();

    [GeneratedRegex(@"data-asin-price=""([\d.]+)""", RegexOptions.IgnoreCase)]
    private static partial Regex AmazonDataPriceRegex();

    [GeneratedRegex(@"id=""landingImage""[^>]*src=""([^""]+)""", RegexOptions.IgnoreCase)]
    private static partial Regex AmazonLandingImageRegex();

    [GeneratedRegex(@"data-a-dynamic-image=""\{""([^""]+)""", RegexOptions.IgnoreCase)]
    private static partial Regex AmazonDynamicImageRegex();

    // Walmart
    [GeneratedRegex(@"<h1[^>]*class=""[^""]*prod-ProductTitle[^""]*""[^>]*>([^<]+)", RegexOptions.IgnoreCase)]
    private static partial Regex WalmartTitleRegex();

    [GeneratedRegex(@"itemprop=""price""[^>]*content=""([\d.]+)""", RegexOptions.IgnoreCase)]
    private static partial Regex WalmartPriceRegex();

    // Target
    [GeneratedRegex(@"data-test=""product-title""[^>]*>([^<]+)", RegexOptions.IgnoreCase)]
    private static partial Regex TargetTitleRegex();

    // Etsy
    [GeneratedRegex(@"<h1[^>]*class=""[^""]*wt-text-body-01[^""]*""[^>]*>([^<]+)", RegexOptions.IgnoreCase)]
    private static partial Regex EtsyTitleRegex();

    // eBay
    [GeneratedRegex(@"class=""x-item-title__mainTitle""[^>]*>\s*<span[^>]*>([^<]+)", RegexOptions.IgnoreCase | RegexOptions.Singleline)]
    private static partial Regex EbayTitleRegex();

    [GeneratedRegex(@"class=""x-price-primary""[^>]*>\s*<span[^>]*>.*?US \$([\d,.]+)", RegexOptions.IgnoreCase | RegexOptions.Singleline)]
    private static partial Regex EbayPriceRegex();

    // Best Buy
    [GeneratedRegex(@"class=""sku-title""[^>]*>\s*<h1[^>]*>([^<]+)", RegexOptions.IgnoreCase | RegexOptions.Singleline)]
    private static partial Regex BestBuyTitleRegex();

    // Pinterest
    [GeneratedRegex(@"<meta[^>]*name=""description""[^>]*content=""([^""]+)""", RegexOptions.IgnoreCase)]
    private static partial Regex PinterestDescRegex();

    // Shopify
    [GeneratedRegex(@"class=""product-single__title""[^>]*>([^<]+)", RegexOptions.IgnoreCase)]
    private static partial Regex ShopifyTitleRegex();
}
