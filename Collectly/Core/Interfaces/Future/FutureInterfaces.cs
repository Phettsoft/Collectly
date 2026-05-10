namespace Collectly.Core.Interfaces.Future;

/// <summary>
/// Future interface for cloud synchronization.
/// Implementation will connect to backend API for multi-device sync.
/// </summary>
public interface ICloudSyncService
{
    Task<bool> SyncAsync(CancellationToken cancellationToken = default);
    Task<bool> IsAuthenticatedAsync();
    Task<bool> LoginAsync(string email, string password);
    Task LogoutAsync();
}

/// <summary>
/// Future interface for AI-powered recommendations.
/// Implementation will provide smart suggestions based on user behavior.
/// </summary>
public interface IAiRecommendationService
{
    Task<List<string>> GetGiftSuggestionsAsync(string recipientName, string eventType);
    Task<List<string>> GetSimilarProductsAsync(string productUrl);
    Task<string> GetSmartCategoryAsync(string productTitle, string? storeName);
}

/// <summary>
/// Future interface for price tracking.
/// Implementation will monitor product prices and notify on drops.
/// </summary>
public interface IPriceTrackingService
{
    Task<decimal?> GetCurrentPriceAsync(string productUrl);
    Task<bool> StartTrackingAsync(int itemId);
    Task<bool> StopTrackingAsync(int itemId);
}

/// <summary>
/// Future interface for push notifications.
/// </summary>
public interface INotificationService
{
    Task<bool> RegisterDeviceAsync();
    Task SendLocalNotificationAsync(string title, string message);
    Task ScheduleReminderAsync(int collectionId, DateTime reminderDate);
}

/// <summary>
/// Future interface for family/shared collections.
/// </summary>
public interface ISharingService
{
    Task<string> GenerateShareLinkAsync(int collectionId);
    Task<bool> AcceptShareInviteAsync(string inviteCode);
    Task<bool> RevokeShareAsync(int collectionId, string userId);
}
