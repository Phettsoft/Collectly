using Collectly.ViewModels;

namespace Collectly.Views.Pages;

public partial class AboutPage : ContentPage
{
    public AboutPage(AboutViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
