using System.Windows.Input;
using Collectly.Core.Enums;
using Collectly.Core.Interfaces.Services;
using Collectly.Models.Entities;

namespace Collectly.ViewModels;

[QueryProperty(nameof(ItemId), "id")]
[QueryProperty(nameof(CollectionId), "collectionId")]
public class AddEditItemViewModel : BaseViewModel
{
    private readonly IItemService _itemService;
    private readonly IMetadataExtractorService _metadataService;
    private readonly IAppLogger _logger;
    private int _itemId;
    private int _collectionId;
    private string _itemTitle = string.Empty;
    private string _url = string.Empty;
    private string _storeName = string.Empty;
    private string _description = string.Empty;
    private string _imageUrl = string.Empty;
    private string _price = string.Empty;
    private string _notes = string.Empty;
    private ItemPriority _priority = ItemPriority.None;
    private string _tags = string.Empty;
    private bool _isEditing;
    private bool _isExtracting;

    public int ItemId
    {
        get => _itemId;
        set { _itemId = value; if (value > 0) _ = LoadExistingAsync(); }
    }

    public int CollectionId { get => _collectionId; set => SetProperty(ref _collectionId, value); }
    public string ItemTitle { get => _itemTitle; set => SetProperty(ref _itemTitle, value); }
    public string Url { get => _url; set => SetProperty(ref _url, value); }
    public string StoreName { get => _storeName; set => SetProperty(ref _storeName, value); }
    public string Description { get => _description; set => SetProperty(ref _description, value); }
    public string ImageUrl { get => _imageUrl; set => SetProperty(ref _imageUrl, value); }
    public string Price { get => _price; set => SetProperty(ref _price, value); }
    public string Notes { get => _notes; set => SetProperty(ref _notes, value); }
    public ItemPriority Priority { get => _priority; set => SetProperty(ref _priority, value); }
    public string Tags { get => _tags; set => SetProperty(ref _tags, value); }
    public bool IsEditing { get => _isEditing; set => SetProperty(ref _isEditing, value); }
    public bool IsExtracting { get => _isExtracting; set => SetProperty(ref _isExtracting, value); }

    public List<ItemPriority> Priorities => Enum.GetValues<ItemPriority>().ToList();

    public ICommand SaveCommand { get; }
    public ICommand CancelCommand { get; }
    public ICommand ExtractMetadataCommand { get; }
    public ICommand OpenUrlCommand { get; }

    public AddEditItemViewModel(
        IItemService itemService,
        IMetadataExtractorService metadataService,
        IAppLogger logger)
    {
        _itemService = itemService;
        _metadataService = metadataService;
        _logger = logger;
        Title = "New Item";

        SaveCommand = CreateCommand(SaveAsync);
        CancelCommand = CreateCommand(async () => await Shell.Current.GoToAsync(".."));
        ExtractMetadataCommand = CreateCommand(ExtractMetadataAsync);
        OpenUrlCommand = CreateCommand(OpenUrlAsync);
    }

    private async Task OpenUrlAsync()
    {
        if (string.IsNullOrWhiteSpace(Url)) return;
        try
        {
            await Browser.Default.OpenAsync(Url, BrowserLaunchMode.SystemPreferred);
        }
        catch (Exception ex)
        {
            _logger.Warning($"Failed to open URL: {ex.Message}");
        }
    }

    private async Task LoadExistingAsync()
    {
        var item = await _itemService.GetItemByIdAsync(_itemId);
        if (item is null) return;

        IsEditing = true;
        Title = "Edit Item";
        CollectionId = item.CollectionId;
        ItemTitle = item.Title;
        Url = item.Url ?? string.Empty;
        StoreName = item.StoreName ?? string.Empty;
        Description = item.Description ?? string.Empty;
        ImageUrl = item.ImageUrl ?? string.Empty;
        Price = item.Price?.ToString("F2") ?? string.Empty;
        Notes = item.Notes ?? string.Empty;
        Priority = item.Priority;
        Tags = item.Tags ?? string.Empty;
    }

    private async Task ExtractMetadataAsync()
    {
        if (string.IsNullOrWhiteSpace(Url)) return;
        IsExtracting = true;

        try
        {
            var metadata = await _metadataService.ExtractAsync(Url);
            if (metadata.ExtractionSucceeded)
            {
                if (!string.IsNullOrWhiteSpace(metadata.Title)) ItemTitle = metadata.Title;
                if (!string.IsNullOrWhiteSpace(metadata.StoreName)) StoreName = metadata.StoreName;
                if (!string.IsNullOrWhiteSpace(metadata.ImageUrl)) ImageUrl = metadata.ImageUrl;
                if (!string.IsNullOrWhiteSpace(metadata.Description)) Description = metadata.Description;
                if (metadata.Price.HasValue) Price = metadata.Price.Value.ToString("F2");
            }
        }
        catch (Exception ex)
        {
            _logger.Error("Metadata extraction failed", ex);
        }
        finally
        {
            IsExtracting = false;
        }
    }

    private async Task SaveAsync()
    {
        if (string.IsNullOrWhiteSpace(ItemTitle) && string.IsNullOrWhiteSpace(Url))
        {
            await Shell.Current.DisplayAlertAsync("Validation", "Title or URL is required.", "OK");
            return;
        }

        try
        {
            decimal? price = decimal.TryParse(Price, out var p) ? p : null;

            if (IsEditing)
            {
                var item = await _itemService.GetItemByIdAsync(_itemId);
                if (item is null) return;
                item.Title = ItemTitle;
                item.Url = string.IsNullOrWhiteSpace(Url) ? null : Url;
                item.StoreName = string.IsNullOrWhiteSpace(StoreName) ? null : StoreName;
                item.Description = string.IsNullOrWhiteSpace(Description) ? null : Description;
                item.ImageUrl = string.IsNullOrWhiteSpace(ImageUrl) ? null : ImageUrl;
                item.Price = price;
                item.Notes = string.IsNullOrWhiteSpace(Notes) ? null : Notes;
                item.Priority = Priority;
                item.Tags = string.IsNullOrWhiteSpace(Tags) ? null : Tags;
                await _itemService.UpdateItemAsync(item);
            }
            else
            {
                var item = new SavedItem
                {
                    CollectionId = CollectionId,
                    Title = ItemTitle,
                    Url = string.IsNullOrWhiteSpace(Url) ? null : Url,
                    StoreName = string.IsNullOrWhiteSpace(StoreName) ? null : StoreName,
                    Description = string.IsNullOrWhiteSpace(Description) ? null : Description,
                    ImageUrl = string.IsNullOrWhiteSpace(ImageUrl) ? null : ImageUrl,
                    Price = price,
                    Notes = string.IsNullOrWhiteSpace(Notes) ? null : Notes,
                    Priority = Priority,
                    Tags = string.IsNullOrWhiteSpace(Tags) ? null : Tags
                };
                await _itemService.CreateItemAsync(item);
            }

            await Shell.Current.GoToAsync("..");
        }
        catch (Exception ex)
        {
            _logger.Error("Failed to save item", ex);
            await Shell.Current.DisplayAlertAsync("Error", "Failed to save item.", "OK");
        }
    }
}
