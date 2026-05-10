namespace Collectly;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        var page = new ContentPage
        {
            Content = new Label
            {
                Text = "COLLECTLY IS RUNNING!",
                VerticalOptions = LayoutOptions.Center,
                HorizontalOptions = LayoutOptions.Center,
                FontSize = 32,
                TextColor = Colors.White
            },
            BackgroundColor = Colors.DarkBlue
        };
        return new Window(page);
    }
}
