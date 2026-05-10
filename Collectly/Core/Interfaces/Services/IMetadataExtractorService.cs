using Collectly.Models.DTOs;

namespace Collectly.Core.Interfaces.Services;

public interface IMetadataExtractorService
{
    Task<ProductMetadata> ExtractAsync(string url, CancellationToken cancellationToken = default);
}
