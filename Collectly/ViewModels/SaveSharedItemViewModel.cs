using System.Collections.ObjectModel;
using System.Windows.Input;
using Collectly.Core.Interfaces.Services;
using Collectly.Models.DTOs;
using Collectly.Models.Entities;

namespace Collectly.ViewModels;

[QueryProperty(nameof(SharedUrl), "url")]
[QueryProperty(nameof(SharedText), "text")]
[QueryProperty(nameof(SharedImage), "image")]
public class SaveSharedItemViewModel : BaseViewModel
{
    private readonly IShareReceiverService _shareService;
    private readonly IMetadataExtractorService _metadataService;
    private readonly ICollectionService _collectionService;
    private readonly IItemService _itemService;
    private readonly IAppLogger _logger;

    private string _sharedUrl = string.Empty;
    private string _sharedText = string.Empty;
    private string _sharedImage = string.Empty;
    private string _itemTitle = string.Empty;
    private string _imageUrl = string.Empty;
    private string _price = string.Empty;
    private string _notes = string.Empty;
    private string _storeName = string.Empty;
    private string _newCollectionName = string.Empty;
    private Collection? _selectedCollection;
    private bool _isExtracting;
    private bool _isDuplicate;
    private bool _isCreatingNewList;

    public ObservableCollection<Collection> Collections { get; } = [];

    public string SharedUrl
    {
        get => _sharedUrl;
        set
        {
            var decoded = Uri.UnescapeDataString(value ?? string.Empty);
            SetProperty(ref _sharedUrl, decoded);
            _ = ProcessSharedContentAsync();
        }
    }

    public string SharedText
    {
        get => _sharedText;
        set => SetProperty(ref _sharedText, Uri.UnescapeDataString(value ?? string.Empty));
    }

    public string SharedImage
    {
        get => _sharedImage;
        set => SetProperty(ref _sharedImage, Uri.UnescapeDataString(value ?? string.Empty));
    }

    public string ItemTitle { get => _itemTitle; set => SetProperty(ref _itemTitle, value); }
    public string ImageUrl { get => _imageUrl; set => SetProperty(ref _imageUrl, value); }
    public string Price { get => _price; set => SetProperty(ref _price, value); }
    public string Notes { get => _notes; set => SetProperty(ref _notes, value); }
    public string StoreName { get => _storeName; set => SetProperty(ref _storeName, value); }
    public bool IsExtracting { get => _isExtracting; set => SetProperty(ref _isExtracting, value); }
    public bool IsDuplicate { get => _isDuplicate; set => SetProperty(ref _isDuplicate, value); }
    public bool IsCreatingNewList { get => _isCreatingNewList; set => SetProperty(ref _isCreatingNewList, value); }
    public string NewCollectionName { get => _newCollectionName; set => SetProperty(ref _newCollectionName, value); }

    public Collection? SelectedCollection
    {
        get => _selectedCollection;
        set
        {
            SetProperty(ref _selectedCollection, value);
            _ = CheckDuplicateAsync();
        }
    }

    public ICommand SaveCommand { get; }
    public ICommand CancelCommand { get; }
    public ICommand OpenUrlCommand { get; }
    public ICommand CreateNewListCommand { get; }
    public ICommand CancelNewListCommand { get; }

    public SaveSharedItemViewModel(
        IShareReceiverService shareService,
        IMetadataExtractorService metadataService,
        ICollectionService collectionService,
        IItemService itemService,
        IAppLogger logger)
    {
        _shareService = shareService;
        _metadataService = metadataService;
        _collectionService = collectionService;
        _itemService = itemService;
        _logger = logger;
        Title = "Save Item";

        SaveCommand = CreateCommand(SaveAsync);
        CancelCommand = CreateCommand(async () => await Shell.Current.GoToAsync(".."));
        OpenUrlCommand = CreateCommand(OpenUrlAsync);
        CreateNewListCommand = CreateCommand(CreateNewListAsync);
        CancelNewListCommand = CreateCommand(CancelNewListAsync);

        _ = LoadCollectionsAsync();
    }

    private async Task LoadCollectionsAsync()
    {
        var collections = await _collectionService.GetActiveCollectionsAsync();
        Collections.Clear();
        foreach (var c in collections)
            Collections.Add(c);
        if (Collections.Count > 0)
            SelectedCollection = Collections[0];
    }

    private async Task ProcessSharedContentAsync()
    {
        var content = await _shareService.ProcessSharedContentAsync(SharedText, SharedUrl, SharedImage);
        ItemTitle = content.Title ?? string.Empty;
        StoreName = content.StoreName ?? string.Empty;

        if (content.HasUrl && content.Url != SharedUrl)
            _sharedUrl = content.Url!;

        if (!string.IsNullOrWhiteSpace(SharedImage))
            ImageUrl = SharedImage;

        if (content.HasUrl)
        {
            IsExtracting = true;
            try
            {
                var metadata = await _metadataService.ExtractAsync(content.Url!);
                if (metadata.ExtractionSucceeded)
                {
                    if (!string.IsNullOrWhiteSpace(metadata.Title)) ItemTitle = metadata.Title;
                    if (!string.IsNullOrWhiteSpace(metadata.StoreName) && string.IsNullOrWhiteSpace(StoreName))
                        StoreName = metadata.StoreName;
                    if (!string.IsNullOrWhiteSpace(metadata.ImageUrl)) ImageUrl = metadata.ImageUrl;
                    if (metadata.Price.HasValue) Price = metadata.Price.Value.ToString("F2");
                }
            }
            catch (Exception ex)
            {
                _logger.Warning($"Metadata extraction failed: {ex.Message}");
            }
            finally
            {
                IsExtracting = false;
            }
        }

        OnPropertyChanged(nameof(SharedUrl));
    }

    private async Task OpenUrlAsync()
    {
        if (string.IsNullOrWhiteSpace(SharedUrl)) return;
        try
        {
            await Browser.Default.OpenAsync(SharedUrl, BrowserLaunchMode.SystemPreferred);
        }
        catch (Exception ex)
        {
            _logger.Warning($"Failed to open URL: {ex.Message}");
        }
    }

    private async Task CreateNewListAsync()
    {
        if (IsCreatingNewList)
        {
            // User tapped "Create" — actually create the list
            if (string.IsNullOrWhiteSpace(NewCollectionName))
            {
                await Shell.Current.DisplayAlertAsync("Name Required", "Enter a name for the new list.", "OK");
                return;
            }

            try
            {
                var newCollection = new Collection { Name = NewCollectionName.Trim() };
                var id = await _collectionService.CreateCollectionAsync(newCollection);
                newCollection.Id = id;
                Collections.Add(newCollection);
                SelectedCollection = newCollection;
                IsCreatingNewList = false;
                NewCollectionName = string.Empty;
                _logger.Info($"New collection created from share: {newCollection.Name}");
            }
            catch (Exception ex)
            {
                _logger.Error("Failed to create collection", ex);
                await Shell.Current.DisplayAlertAsync("Error", "Failed to create list.", "OK");
            }
        }
        else
        {
            IsCreatingNewList = true;
        }
    }

    private Task CancelNewListAsync()
    {
        IsCreatingNewList = false;
        NewCollectionName = string.Empty;
        return Task.CompletedTask;
    }

    private async Task CheckDuplicateAsync()
    {
        if (SelectedCollection is null || string.IsNullOrWhiteSpace(SharedUrl)) return;
        IsDuplicate = await _itemService.IsDuplicateAsync(SharedUrl, SelectedCollection.Id);
    }

    private async Task SaveAsync()
    {
        if (SelectedCollection is null)
        {
            await Shell.Current.DisplayAlertAsync("Select Collection", "Please select a collection.", "OK");
            return;
        }

        try
        {
            decimal? price = decimal.TryParse(Price, out var p) ? p : null;
            var item = new SavedItem
            {
                CollectionId = SelectedCollection.Id,
                Title = string.IsNullOrWhiteSpace(ItemTitle) ? "Saved Item" : ItemTitle,
                Url = string.IsNullOrWhiteSpace(SharedUrl) ? null : SharedUrl,
                StoreName = string.IsNullOrWhiteSpace(StoreName) ? null : StoreName,
                ImageUrl = string.IsNullOrWhiteSpace(ImageUrl) ? null : ImageUrl,
                Price = price,
                Notes = string.IsNullOrWhiteSpace(Notes) ? null : Notes
            };

            await _itemService.CreateItemAsync(item);
            _logger.Info($"Shared item saved: {item.Title} -> {SelectedCollection.Name}");
            await Shell.Current.GoToAsync("../..");
        }
        catch (Exception ex)
        {
            _logger.Error("Failed to save shared item", ex);
            await Shell.Current.DisplayAlertAsync("Error", "Failed to save item.", "OK");
        }
    }
}
