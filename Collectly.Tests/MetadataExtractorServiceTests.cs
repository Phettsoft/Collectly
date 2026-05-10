using System.Net;
using Collectly.Core.Interfaces.Services;
using Collectly.Services.Metadata;
using NSubstitute;

namespace Collectly.Tests;

public class MetadataExtractorServiceTests
{
    private readonly IAppLogger _logger;

    public MetadataExtractorServiceTests()
    {
        _logger = Substitute.For<IAppLogger>();
    }

    [Fact]
    public async Task Extract_WithTimeout_ReturnsErrorMessage()
    {
        var handler = new TimeoutHandler();
        var httpClient = new HttpClient(handler);
        var service = new MetadataExtractorService(httpClient, _logger);

        var result = await service.ExtractAsync("https://example.com");

        Assert.False(result.ExtractionSucceeded);
        Assert.NotNull(result.ErrorMessage);
    }

    [Fact]
    public async Task Extract_WithValidHtml_ExtractsTitle()
    {
        var html = """
            <html>
            <head>
                <title>Amazing Product - Store</title>
                <meta property="og:title" content="Amazing Product"/>
                <meta property="og:image" content="https://img.example.com/product.jpg"/>
                <meta property="og:site_name" content="TestStore"/>
            </head>
            <body></body>
            </html>
            """;

        var handler = new FakeHttpHandler(html);
        var httpClient = new HttpClient(handler);
        var service = new MetadataExtractorService(httpClient, _logger);

        var result = await service.ExtractAsync("https://example.com/product");

        Assert.True(result.ExtractionSucceeded);
        Assert.Equal("Amazing Product", result.Title);
        Assert.Equal("TestStore", result.StoreName);
        Assert.Equal("https://img.example.com/product.jpg", result.ImageUrl);
    }

    [Fact]
    public async Task Extract_WithJsonLd_ExtractsPrice()
    {
        var html = """
            <html>
            <head><title>Product</title></head>
            <body>
            <script type="application/ld+json">
            {
                "@type": "Product",
                "name": "Widget Pro",
                "offers": {
                    "price": "49.99",
                    "priceCurrency": "USD"
                }
            }
            </script>
            </body>
            </html>
            """;

        var handler = new FakeHttpHandler(html);
        var httpClient = new HttpClient(handler);
        var service = new MetadataExtractorService(httpClient, _logger);

        var result = await service.ExtractAsync("https://example.com/widget");

        Assert.Equal("Widget Pro", result.Title);
        Assert.Equal(49.99m, result.Price);
        Assert.Equal("USD", result.Currency);
    }

    private class FakeHttpHandler : HttpMessageHandler
    {
        private readonly string _response;
        public FakeHttpHandler(string response) => _response = response;

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, CancellationToken cancellationToken) =>
            Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(_response)
            });
    }

    private class TimeoutHandler : HttpMessageHandler
    {
        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, CancellationToken cancellationToken)
        {
            await Task.Delay(Timeout.Infinite, cancellationToken);
            throw new TaskCanceledException();
        }
    }
}
