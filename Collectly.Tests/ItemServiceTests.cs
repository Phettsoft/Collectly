using Collectly.Core.Interfaces.Repositories;
using Collectly.Core.Interfaces.Services;
using Collectly.Models.Entities;
using Collectly.Services;
using NSubstitute;

namespace Collectly.Tests;

public class ItemServiceTests
{
    private readonly IItemRepository _repo;
    private readonly IItemService _service;

    public ItemServiceTests()
    {
        _repo = Substitute.For<IItemRepository>();
        var logger = Substitute.For<IAppLogger>();
        _service = new ItemService(_repo, logger);
    }

    [Fact]
    public async Task CreateItem_WithTitleOnly_Succeeds()
    {
        _repo.CreateAsync(Arg.Any<SavedItem>()).Returns(1);

        var item = new SavedItem { Title = "Cool Gadget", CollectionId = 1 };
        var id = await _service.CreateItemAsync(item);

        Assert.Equal(1, id);
    }

    [Fact]
    public async Task CreateItem_WithUrlOnly_Succeeds()
    {
        _repo.CreateAsync(Arg.Any<SavedItem>()).Returns(1);

        var item = new SavedItem { Url = "https://amazon.com/dp/123", CollectionId = 1 };
        var id = await _service.CreateItemAsync(item);

        Assert.Equal(1, id);
    }

    [Fact]
    public async Task CreateItem_WithNoTitleOrUrl_ThrowsArgumentException()
    {
        var item = new SavedItem { CollectionId = 1 };

        await Assert.ThrowsAsync<ArgumentException>(() =>
            _service.CreateItemAsync(item));
    }

    [Fact]
    public async Task DuplicateItem_CopiesAllFields()
    {
        var source = new SavedItem
        {
            Id = 1,
            CollectionId = 1,
            Title = "Widget",
            Url = "https://example.com",
            StoreName = "Example",
            Price = 29.99m
        };
        _repo.GetByIdAsync(1).Returns(source);
        _repo.CreateAsync(Arg.Any<SavedItem>()).Returns(2);

        var newId = await _service.DuplicateItemAsync(1, 2);

        await _repo.Received(1).CreateAsync(Arg.Is<SavedItem>(i =>
            i.Title == "Widget" &&
            i.CollectionId == 2 &&
            i.Price == 29.99m));
    }

    [Fact]
    public async Task IsDuplicate_DelegatesToRepository()
    {
        _repo.ExistsByUrlAsync("https://amazon.com/dp/123", 1).Returns(true);

        var result = await _service.IsDuplicateAsync("https://amazon.com/dp/123", 1);

        Assert.True(result);
    }

    [Fact]
    public async Task MoveItem_DelegatesToRepository()
    {
        _repo.MoveToCollectionAsync(1, 2).Returns(1);

        var result = await _service.MoveItemAsync(1, 2);

        Assert.Equal(1, result);
    }
}
