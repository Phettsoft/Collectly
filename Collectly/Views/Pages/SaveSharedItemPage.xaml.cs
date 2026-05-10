using Collectly.ViewModels;

namespace Collectly.Views.Pages;

public partial class SaveSharedItemPage : ContentPage
{
    public SaveSharedItemPage(SaveSharedItemViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
