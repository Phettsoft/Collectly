using Collectly.Views.Pages;

namespace Collectly;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();

        Routing.RegisterRoute("collectiondetail", typeof(CollectionDetailPage));
        Routing.RegisterRoute("addcollection", typeof(AddEditCollectionPage));
        Routing.RegisterRoute("editcollection", typeof(AddEditCollectionPage));
        Routing.RegisterRoute("additem", typeof(AddEditItemPage));
        Routing.RegisterRoute("edititem", typeof(AddEditItemPage));
        Routing.RegisterRoute("saveshared", typeof(SaveSharedItemPage));
    }
}
