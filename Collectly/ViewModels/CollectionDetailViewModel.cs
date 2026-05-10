using System.Collections.ObjectModel;
using System.Windows.Input;
using Collectly.Core.Interfaces.Services;
using Collectly.Models.Entities;

namespace Collectly.ViewModels;

[QueryProperty(nameof(CollectionId), "id")]
public class CollectionDetailViewModel : BaseViewModel
{
    private readonly ICollectionService _collectionService;
    private readonly IItemService _itemService;
    private readonly IAppLogger _logger;
    private int _collectionId;
    private Collection? _collection;

    public ObservableCollection<SavedItem> Items { get; } = [];

    public int CollectionId
    {
        get => _collectionId;
        set
        {
            _collectionId = value;
            _ = LoadAsync();
        }
    }

    public Collection? Collection
    {
        get => _collection;
        set => SetProperty(ref _collection, value);
    }

    public ICommand LoadCommand { get; }
    public ICommand AddItemCommand { get; }
    public ICommand DeleteItemCommand { get; }
    public ICommand TogglePurchasedCommand { get; }
    public ICommand OpenItemCommand { get; }
    public ICommand EditCollectionCommand { get; }
    public ICommand ShareCollectionCommand { get; }
    public ICommand ShareItemCommand { get; }

    public CollectionDetailViewModel(
        ICollectionService collectionService,
        IItemService itemService,
        IAppLogger logger)
    {
        _collectionService = collectionService;
        _itemService = itemService;
        _logger = logger;

        LoadCommand = CreateCommand(LoadAsync);
        AddItemCommand = CreateCommand(NavigateToAddItem);
        DeleteItemCommand = new Command<SavedItem>(async i => await DeleteItemAsync(i));
        TogglePurchasedCommand = new Command<SavedItem>(async i => await TogglePurchasedAsync(i));
        OpenItemCommand = new Command<SavedItem>(async i => await OpenItemAsync(i));
        EditCollectionCommand = CreateCommand(NavigateToEditCollection);
        ShareCollectionCommand = CreateCommand(ShareCollectionAsync);
        ShareItemCommand = new Command<SavedItem>(async i => await ShareItemAsync(i));
    }

    public async Task LoadAsync()
    {
        if (IsBusy) return;
        IsBusy = true;

        try
        {
            Collection = await _collectionService.GetCollectionByIdAsync(_collectionId);
            Title = Collection?.Name ?? "Collection";

            var items = await _itemService.GetItemsByCollectionAsync(_collectionId);
            Items.Clear();
            foreach (var item in items)
                Items.Add(item);
        }
        catch (Exception ex)
        {
            _logger.Error("Failed to load collection detail", ex);
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task NavigateToAddItem() =>
        await Shell.Current.GoToAsync($"additem?collectionId={_collectionId}");

    private async Task NavigateToEditCollection() =>
        await Shell.Current.GoToAsync($"editcollection?id={_collectionId}");

    private async Task DeleteItemAsync(SavedItem item)
    {
        var confirm = await Shell.Current.DisplayAlertAsync("Delete",
            $"Delete '{item.Title}'?", "Delete", "Cancel");
        if (!confirm) return;
        await _itemService.DeleteItemAsync(item.Id);
        await LoadAsync();
    }

    private async Task TogglePurchasedAsync(SavedItem item)
    {
        item.IsPurchased = !item.IsPurchased;
        await _itemService.UpdateItemAsync(item);
        await LoadAsync();
    }

    private async Task OpenItemAsync(SavedItem item) =>
        await Shell.Current.GoToAsync($"edititem?id={item.Id}");

    private async Task ShareCollectionAsync()
    {
        if (Collection is null || Items.Count == 0) return;

        var lines = new List<string>
        {
            $"📋 {Collection.Name}",
            $"{Items.Count} item(s)",
            ""
        };

        foreach (var item in Items)
        {
            var line = $"• {item.Title}";
            if (item.Price.HasValue) line += $" - ${item.Price:F2}";
            if (!string.IsNullOrWhiteSpace(item.StoreName)) line += $" ({item.StoreName})";
            lines.Add(line);
            if (!string.IsNullOrWhiteSpace(item.Url))
                lines.Add($"  {item.Url}");
        }

        var text = string.Join("\n", lines);
        await Share.Default.RequestAsync(new ShareTextRequest
        {
            Title = $"Share: {Collection.Name}",
            Text = text
        });
    }

    private async Task ShareItemAsync(SavedItem item)
    {
        var lines = new List<string> { item.Title };
        if (item.Price.HasValue) lines.Add($"${item.Price:F2}");
        if (!string.IsNullOrWhiteSpace(item.StoreName)) lines.Add(item.StoreName);
        if (!string.IsNullOrWhiteSpace(item.Url)) lines.Add(item.Url);

        var text = string.Join("\n", lines);
        await Share.Default.RequestAsync(new ShareTextRequest
        {
            Title = $"Share: {item.Title}",
            Text = text,
            Uri = item.Url
        });
    }
}
