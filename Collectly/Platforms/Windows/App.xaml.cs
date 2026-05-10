using Microsoft.UI.Xaml;

namespace Collectly.WinUI;

public partial class App : MauiWinUIApplication
{
    public App()
    {
        this.InitializeComponent();
    }

    protected override MauiApp CreateMauiApp() => Collectly.MauiProgram.CreateMauiApp();
}
