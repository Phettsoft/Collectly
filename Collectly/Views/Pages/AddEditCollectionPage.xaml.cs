using Collectly.ViewModels;

namespace Collectly.Views.Pages;

public partial class AddEditCollectionPage : ContentPage
{
    public AddEditCollectionPage(AddEditCollectionViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
