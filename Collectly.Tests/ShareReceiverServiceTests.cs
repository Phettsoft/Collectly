using Collectly.Core.Interfaces.Services;
using Collectly.Services;
using NSubstitute;

namespace Collectly.Tests;

public class ShareReceiverServiceTests
{
    private readonly IShareReceiverService _service;

    public ShareReceiverServiceTests()
    {
        var logger = Substitute.For<IAppLogger>();
        _service = new ShareReceiverService(logger);
    }

    [Fact]
    public async Task ProcessSharedContent_WithDirectUrl_ExtractsUrl()
    {
        var result = await _service.ProcessSharedContentAsync(null, "https://www.amazon.com/dp/B09V3KXJPB", null);

        Assert.True(result.HasUrl);
        Assert.Equal("https://www.amazon.com/dp/B09V3KXJPB", result.Url);
        Assert.Equal("Amazon", result.StoreName);
    }

    [Fact]
    public async Task ProcessSharedContent_WithTextContainingUrl_ExtractsUrl()
    {
        var text = "Check this out! https://www.etsy.com/listing/12345 so cool";
        var result = await _service.ProcessSharedContentAsync(text, null, null);

        Assert.True(result.HasUrl);
        Assert.Contains("etsy.com", result.Url);
        Assert.Equal("Etsy", result.StoreName);
    }

    [Fact]
    public async Task ProcessSharedContent_WithPlainText_SetsTitle()
    {
        var result = await _service.ProcessSharedContentAsync("Cool gadget I want", null, null);

        Assert.False(result.HasUrl);
        Assert.Equal("Cool gadget I want", result.Title);
    }

    [Fact]
    public async Task ProcessSharedContent_WithNoContent_ReturnsEmptyResult()
    {
        var result = await _service.ProcessSharedContentAsync(null, null, null);

        Assert.False(result.HasUrl);
        Assert.False(result.HasImage);
    }

    [Theory]
    [InlineData("https://www.walmart.com/ip/123", "Walmart")]
    [InlineData("https://www.target.com/p/item", "Target")]
    [InlineData("https://www.pinterest.com/pin/123", "Pinterest")]
    [InlineData("https://www.bestbuy.com/site/item", "Best Buy")]
    [InlineData("https://www.unknownstore.com/product", null)]
    public async Task ProcessSharedContent_DetectsStoreCorrectly(string url, string? expectedStore)
    {
        var result = await _service.ProcessSharedContentAsync(null, url, null);

        Assert.Equal(expectedStore, result.StoreName);
    }

    [Fact]
    public async Task ProcessSharedContent_WithImage_SetsImageUri()
    {
        var result = await _service.ProcessSharedContentAsync(null, null, "content://media/image.jpg");

        Assert.True(result.HasImage);
        Assert.Equal("content://media/image.jpg", result.ImageUri);
    }
}
