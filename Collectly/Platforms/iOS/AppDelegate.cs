using Foundation;
using UIKit;
using Collectly.Platforms.iOS.Extensions;

namespace Collectly.Platforms.iOS;

[Register("AppDelegate")]
public class AppDelegate : MauiUIApplicationDelegate
{
    protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();

    public override bool OpenUrl(UIApplication application, NSUrl url, NSDictionary options)
    {
        if (url.Scheme == "collectly" && url.Host == "share")
        {
            var query = url.Query ?? string.Empty;
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                await Task.Delay(300);
                await Shell.Current.GoToAsync($"saveshared?{query}");
            });
            return true;
        }
        return base.OpenUrl(application, url, options);
    }

    public override bool FinishedLaunching(UIApplication application, NSDictionary? launchOptions)
    {
        var result = base.FinishedLaunching(application, launchOptions);
        CheckPendingSharedItems();
        return result;
    }

    public override void OnActivated(UIApplication application)
    {
        base.OnActivated(application);
        CheckPendingSharedItems();
    }

    private static void CheckPendingSharedItems()
    {
        var pending = ShareExtensionHandler.GetPendingSharedItem();
        if (pending is null) return;

        var (url, text, imageUri) = pending.Value;
        MainThread.BeginInvokeOnMainThread(async () =>
        {
            await Task.Delay(500);
            var encodedUrl = Uri.EscapeDataString(url ?? string.Empty);
            var encodedText = Uri.EscapeDataString(text ?? string.Empty);
            var encodedImage = Uri.EscapeDataString(imageUri ?? string.Empty);
            await Shell.Current.GoToAsync(
                $"saveshared?url={encodedUrl}&text={encodedText}&image={encodedImage}");
        });
    }
}
