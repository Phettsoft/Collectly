using System.Collections.ObjectModel;
using System.Windows.Input;
using Collectly.Core.Interfaces.Services;
using Collectly.Models.Entities;

namespace Collectly.ViewModels;

public class DashboardViewModel : BaseViewModel
{
    private readonly ICollectionService _collectionService;
    private readonly IAppLogger _logger;
    private string _searchQuery = string.Empty;

    public ObservableCollection<Collection> Collections { get; } = [];
    public ObservableCollection<Collection> FilteredCollections { get; } = [];

    public string SearchQuery
    {
        get => _searchQuery;
        set
        {
            if (SetProperty(ref _searchQuery, value))
                FilterCollections();
        }
    }

    public ICommand LoadCommand { get; }
    public ICommand AddCollectionCommand { get; }
    public ICommand DeleteCollectionCommand { get; }
    public ICommand ArchiveCollectionCommand { get; }
    public ICommand OpenCollectionCommand { get; }
    public ICommand RefreshCommand { get; }

    public DashboardViewModel(ICollectionService collectionService, IAppLogger logger)
    {
        _collectionService = collectionService;
        _logger = logger;
        Title = "Collectly";

        LoadCommand = CreateCommand(LoadCollectionsAsync);
        AddCollectionCommand = CreateCommand(NavigateToAddCollection);
        DeleteCollectionCommand = new Command<Collection>(async c => await DeleteCollectionAsync(c));
        ArchiveCollectionCommand = new Command<Collection>(async c => await ArchiveCollectionAsync(c));
        OpenCollectionCommand = new Command<Collection>(async c => await OpenCollectionAsync(c));
        RefreshCommand = CreateCommand(LoadCollectionsAsync);
    }

    public async Task LoadCollectionsAsync()
    {
        if (IsBusy) return;
        IsBusy = true;

        try
        {
            var collections = await _collectionService.GetActiveCollectionsAsync();
            Collections.Clear();
            FilteredCollections.Clear();
            foreach (var c in collections)
            {
                Collections.Add(c);
                FilteredCollections.Add(c);
            }
        }
        catch (Exception ex)
        {
            _logger.Error("Failed to load collections", ex);
            await Shell.Current.DisplayAlertAsync("Error", "Failed to load collections.", "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void FilterCollections()
    {
        FilteredCollections.Clear();
        var query = SearchQuery.ToLowerInvariant();
        var filtered = string.IsNullOrWhiteSpace(query)
            ? Collections
            : new ObservableCollection<Collection>(
                Collections.Where(c =>
                    c.Name.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                    (c.RecipientName?.Contains(query, StringComparison.OrdinalIgnoreCase) ?? false)));

        foreach (var c in filtered)
            FilteredCollections.Add(c);
    }

    private async Task NavigateToAddCollection() =>
        await Shell.Current.GoToAsync("addcollection");

    private async Task DeleteCollectionAsync(Collection collection)
    {
        var confirm = await Shell.Current.DisplayAlertAsync("Delete",
            $"Delete '{collection.Name}' and all its items?", "Delete", "Cancel");
        if (!confirm) return;

        await _collectionService.DeleteCollectionAsync(collection.Id);
        await LoadCollectionsAsync();
    }

    private async Task ArchiveCollectionAsync(Collection collection)
    {
        await _collectionService.ArchiveCollectionAsync(collection.Id);
        await LoadCollectionsAsync();
    }

    private async Task OpenCollectionAsync(Collection collection) =>
        await Shell.Current.GoToAsync($"collectiondetail?id={collection.Id}");
}
