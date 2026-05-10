using System.Windows.Input;
using Collectly.Core.Enums;
using Collectly.Core.Interfaces.Services;
using Collectly.Models.Entities;

namespace Collectly.ViewModels;

[QueryProperty(nameof(CollectionId), "id")]
public class AddEditCollectionViewModel : BaseViewModel
{
    private readonly ICollectionService _collectionService;
    private readonly IAppLogger _logger;
    private int _collectionId;
    private string _name = string.Empty;
    private string _recipientName = string.Empty;
    private EventType _eventType = EventType.None;
    private DateTime? _eventDate;
    private string _notes = string.Empty;
    private string _themeColor = "#6366F1";
    private string _icon = "📦";
    private bool _isPrivate;
    private bool _isEditing;

    public int CollectionId
    {
        get => _collectionId;
        set
        {
            _collectionId = value;
            if (value > 0) _ = LoadExistingAsync();
        }
    }

    public string Name { get => _name; set => SetProperty(ref _name, value); }
    public string RecipientName { get => _recipientName; set => SetProperty(ref _recipientName, value); }
    public EventType EventType { get => _eventType; set => SetProperty(ref _eventType, value); }
    public DateTime? EventDate { get => _eventDate; set => SetProperty(ref _eventDate, value); }
    public string Notes { get => _notes; set => SetProperty(ref _notes, value); }
    public string ThemeColor { get => _themeColor; set => SetProperty(ref _themeColor, value); }
    public string Icon { get => _icon; set => SetProperty(ref _icon, value); }
    public bool IsPrivate { get => _isPrivate; set => SetProperty(ref _isPrivate, value); }
    public bool IsEditing { get => _isEditing; set => SetProperty(ref _isEditing, value); }

    public List<EventType> EventTypes => Enum.GetValues<EventType>().ToList();

    public ICommand SaveCommand { get; }
    public ICommand CancelCommand { get; }

    public AddEditCollectionViewModel(ICollectionService collectionService, IAppLogger logger)
    {
        _collectionService = collectionService;
        _logger = logger;
        Title = "New Collection";
        SaveCommand = CreateCommand(SaveAsync);
        CancelCommand = CreateCommand(async () => await Shell.Current.GoToAsync(".."));
    }

    private async Task LoadExistingAsync()
    {
        var collection = await _collectionService.GetCollectionByIdAsync(_collectionId);
        if (collection is null) return;

        IsEditing = true;
        Title = "Edit Collection";
        Name = collection.Name;
        RecipientName = collection.RecipientName ?? string.Empty;
        EventType = collection.EventType;
        EventDate = collection.EventDate;
        Notes = collection.Notes ?? string.Empty;
        ThemeColor = collection.ThemeColor;
        Icon = collection.Icon;
        IsPrivate = collection.IsPrivate;
    }

    private async Task SaveAsync()
    {
        if (string.IsNullOrWhiteSpace(Name))
        {
            await Shell.Current.DisplayAlertAsync("Validation", "Collection name is required.", "OK");
            return;
        }

        try
        {
            if (IsEditing)
            {
                var existing = await _collectionService.GetCollectionByIdAsync(_collectionId);
                if (existing is null) return;
                existing.Name = Name;
                existing.RecipientName = string.IsNullOrWhiteSpace(RecipientName) ? null : RecipientName;
                existing.EventType = EventType;
                existing.EventDate = EventDate;
                existing.Notes = string.IsNullOrWhiteSpace(Notes) ? null : Notes;
                existing.ThemeColor = ThemeColor;
                existing.Icon = Icon;
                existing.IsPrivate = IsPrivate;
                await _collectionService.UpdateCollectionAsync(existing);
            }
            else
            {
                var collection = new Collection
                {
                    Name = Name,
                    RecipientName = string.IsNullOrWhiteSpace(RecipientName) ? null : RecipientName,
                    EventType = EventType,
                    EventDate = EventDate,
                    Notes = string.IsNullOrWhiteSpace(Notes) ? null : Notes,
                    ThemeColor = ThemeColor,
                    Icon = Icon,
                    IsPrivate = IsPrivate
                };
                await _collectionService.CreateCollectionAsync(collection);
            }

            await Shell.Current.GoToAsync("..");
        }
        catch (Exception ex)
        {
            _logger.Error("Failed to save collection", ex);
            await Shell.Current.DisplayAlertAsync("Error", "Failed to save collection.", "OK");
        }
    }
}
