using Collectly.Models.DTOs;

namespace Collectly.Core.Interfaces.Services;

public interface IShareReceiverService
{
    Task<SharedContent> ProcessSharedContentAsync(string? text, string? url, string? imageUri);
}
