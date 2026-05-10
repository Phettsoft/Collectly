using Collectly.ViewModels;

namespace Collectly.Views.Pages;

public partial class CollectionDetailPage : ContentPage
{
    public CollectionDetailPage(CollectionDetailViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
