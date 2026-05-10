using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;

namespace Collectly.Platforms.Android;

[Activity(
    Theme = "@style/Maui.SplashTheme",
    MainLauncher = true,
    LaunchMode = LaunchMode.SingleTask,
    ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode |
                           ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
[IntentFilter(
    [Intent.ActionSend],
    Categories = [Intent.CategoryDefault],
    DataMimeType = "text/plain")]
[IntentFilter(
    [Intent.ActionSend],
    Categories = [Intent.CategoryDefault],
    DataMimeType = "image/*")]
public class MainActivity : MauiAppCompatActivity
{
    protected override void OnCreate(Bundle? savedInstanceState)
    {
        base.OnCreate(savedInstanceState);
        HandleShareIntent(Intent);
    }

    protected override void OnNewIntent(Intent? intent)
    {
        base.OnNewIntent(intent);
        if (intent is not null)
            HandleShareIntent(intent);
    }

    private static void HandleShareIntent(Intent? intent)
    {
        if (intent?.Action != Intent.ActionSend) return;

        string? sharedText = null;
        string? sharedUrl = null;
        string? sharedImage = null;

        if (intent.Type?.StartsWith("text/") == true)
        {
            var extraText = intent.GetStringExtra(Intent.ExtraText) ?? string.Empty;

            // Extract URL from the shared text
            var urlMatch = System.Text.RegularExpressions.Regex.Match(
                extraText, @"https?://[^\s<>""']+", System.Text.RegularExpressions.RegexOptions.IgnoreCase);

            if (urlMatch.Success)
            {
                sharedUrl = urlMatch.Value;
                sharedText = extraText.Replace(urlMatch.Value, "").Trim();
            }
            else
            {
                sharedText = extraText;
            }
        }
        else if (intent.Type?.StartsWith("image/") == true)
        {
            global::Android.Net.Uri? uri;
            if (OperatingSystem.IsAndroidVersionAtLeast(33))
                uri = intent.GetParcelableExtra(Intent.ExtraStream, Java.Lang.Class.FromType(typeof(global::Android.Net.Uri))) as global::Android.Net.Uri;
            else
#pragma warning disable CA1422
                uri = intent.GetParcelableExtra(Intent.ExtraStream) as global::Android.Net.Uri;
#pragma warning restore CA1422
            sharedImage = uri?.ToString();
        }

        if (sharedText is null && sharedUrl is null && sharedImage is null) return;

        MainThread.BeginInvokeOnMainThread(async () =>
        {
            await Task.Delay(500); // Allow shell to initialize
            var encodedUrl = Uri.EscapeDataString(sharedUrl ?? string.Empty);
            var encodedText = Uri.EscapeDataString(sharedText ?? string.Empty);
            var encodedImage = Uri.EscapeDataString(sharedImage ?? string.Empty);
            await Shell.Current.GoToAsync(
                $"saveshared?url={encodedUrl}&text={encodedText}&image={encodedImage}");
        });
    }
}
