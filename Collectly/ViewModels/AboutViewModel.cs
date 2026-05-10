using Collectly.Core.Constants;

namespace Collectly.ViewModels;

public class AboutViewModel : BaseViewModel
{
    public string AppName => AppVersion.AppName;
    public string Version => $"v{AppVersion.Version}";
    public string BuildDate => AppVersion.BuildDate;
    public string Description => AppVersion.Description;
    public int SchemaVersion => AppVersion.DatabaseSchemaVersion;

    public AboutViewModel()
    {
        Title = "About";
    }
}
