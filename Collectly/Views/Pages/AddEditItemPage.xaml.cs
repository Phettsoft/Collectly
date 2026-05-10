using Collectly.ViewModels;

namespace Collectly.Views.Pages;

public partial class AddEditItemPage : ContentPage
{
    public AddEditItemPage(AddEditItemViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
