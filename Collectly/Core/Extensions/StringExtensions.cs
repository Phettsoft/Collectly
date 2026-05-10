namespace Collectly.Core.Extensions;

public static class StringExtensions
{
    public static string Truncate(this string value, int maxLength) =>
        string.IsNullOrEmpty(value) ? value :
        value.Length <= maxLength ? value : value[..maxLength] + "…";

    public static bool IsValidUrl(this string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return false;
        return Uri.TryCreate(value, UriKind.Absolute, out var uri) &&
               (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
    }
}
