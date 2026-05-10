using Foundation;
using UIKit;

namespace Collectly.Platforms.iOS.Extensions;

/// <summary>
/// iOS Share Extension handler.
/// In production, this class is compiled into a separate extension target.
/// It processes shared content from the iOS Share Sheet and communicates
/// with the main app via the shared app group container and URL scheme.
///
/// Setup requirements for Xcode/native binding:
/// 1. Create an App Group: group.com.collectly.app
/// 2. Enable App Groups capability on both main app and extension
/// 3. Create a separate extension target with this logic
/// 4. Configure NSExtensionActivationRule in extension's Info.plist
/// </summary>
public static class ShareExtensionHandler
{
    private const string AppGroupId = "group.com.collectly.app";
    private const string SharedDataKey = "CollectlySharedItem";
    private const string AppUrlScheme = "collectly://share";

    /// <summary>
    /// Processes incoming share extension items and stores them in the shared container.
    /// Called from the native ShareViewController in the extension target.
    /// </summary>
    public static async Task<bool> ProcessExtensionItemsAsync(NSExtensionContext context)
    {
        if (context.InputItems is null || context.InputItems.Length == 0)
            return false;

        var inputItem = context.InputItems[0];
        if (inputItem.Attachments is null || inputItem.Attachments.Length == 0)
            return false;

        string? url = null;
        string? text = null;
        string? imageUri = null;

        foreach (var provider in inputItem.Attachments)
        {
            if (provider.HasItemConformingTo("public.url"))
            {
                var item = await provider.LoadItemAsync("public.url", null);
                if (item is NSUrl nsUrl)
                    url = nsUrl.AbsoluteString;
            }
            else if (provider.HasItemConformingTo("public.plain-text"))
            {
                var item = await provider.LoadItemAsync("public.plain-text", null);
                if (item is NSString nsText)
                    text = nsText.ToString();
            }
            else if (provider.HasItemConformingTo("public.image"))
            {
                var item = await provider.LoadItemAsync("public.image", null);
                if (item is NSUrl imgUrl)
                    imageUri = imgUrl.AbsoluteString;
            }
        }

        if (url is null && text is null && imageUri is null)
            return false;

        SaveToSharedContainer(url, text, imageUri);
        return true;
    }

    /// <summary>
    /// Saves shared data to the app group container for the main app to pick up.
    /// </summary>
    private static void SaveToSharedContainer(string? url, string? text, string? imageUri)
    {
        var defaults = new NSUserDefaults(AppGroupId, NSUserDefaultsType.SuiteName);
        var data = new NSDictionary(
            new NSString("url"), NSObject.FromObject(url ?? string.Empty),
            new NSString("text"), NSObject.FromObject(text ?? string.Empty),
            new NSString("image"), NSObject.FromObject(imageUri ?? string.Empty),
            new NSString("timestamp"), NSObject.FromObject(DateTime.UtcNow.ToString("O"))
        );
        defaults.SetValueForKey(data, new NSString(SharedDataKey));
        defaults.Synchronize();
    }

    /// <summary>
    /// Builds the URL to open the main app with shared content parameters.
    /// </summary>
    public static NSUrl BuildAppLaunchUrl(string? url, string? text, string? imageUri)
    {
        var encodedUrl = Uri.EscapeDataString(url ?? string.Empty);
        var encodedText = Uri.EscapeDataString(text ?? string.Empty);
        var encodedImage = Uri.EscapeDataString(imageUri ?? string.Empty);
        return new NSUrl($"{AppUrlScheme}?url={encodedUrl}&text={encodedText}&image={encodedImage}");
    }

    /// <summary>
    /// Checks the shared container for pending items (called by main app on launch).
    /// </summary>
    public static (string? Url, string? Text, string? ImageUri)? GetPendingSharedItem()
    {
        var defaults = new NSUserDefaults(AppGroupId, NSUserDefaultsType.SuiteName);
        var data = defaults.ValueForKey(new NSString(SharedDataKey)) as NSDictionary;
        if (data is null) return null;

        var url = data["url"]?.ToString();
        var text = data["text"]?.ToString();
        var image = data["image"]?.ToString();

        // Clear after reading
        defaults.RemoveObject(SharedDataKey);
        defaults.Synchronize();

        if (string.IsNullOrWhiteSpace(url) && string.IsNullOrWhiteSpace(text) && string.IsNullOrWhiteSpace(image))
            return null;

        return (
            string.IsNullOrWhiteSpace(url) ? null : url,
            string.IsNullOrWhiteSpace(text) ? null : text,
            string.IsNullOrWhiteSpace(image) ? null : image
        );
    }
}
