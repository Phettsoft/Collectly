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
                Text = "Collectly is running!",
                VerticalOptions = LayoutOptions.Center,
                HorizontalOptions = LayoutOptions.Center,
                FontSize = 24,
                TextColor = Colors.Black
            },
            BackgroundColor = Colors.White
        };
        return new Window(page);
    }
}
