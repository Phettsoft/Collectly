using Collectly.Core.Interfaces.Repositories;
using Collectly.Core.Interfaces.Services;
using Collectly.Models.Entities;
using Collectly.Services;
using NSubstitute;

namespace Collectly.Tests;

public class CollectionServiceTests
{
    private readonly ICollectionRepository _repo;
    private readonly ICollectionService _service;

    public CollectionServiceTests()
    {
        _repo = Substitute.For<ICollectionRepository>();
        var logger = Substitute.For<IAppLogger>();
        _service = new CollectionService(_repo, logger);
    }

    [Fact]
    public async Task CreateCollection_WithValidName_Succeeds()
    {
        _repo.CreateAsync(Arg.Any<Collection>()).Returns(1);

        var collection = new Collection { Name = "Christmas List" };
        var id = await _service.CreateCollectionAsync(collection);

        Assert.Equal(1, id);
        await _repo.Received(1).CreateAsync(Arg.Any<Collection>());
    }

    [Fact]
    public async Task CreateCollection_WithEmptyName_ThrowsArgumentException()
    {
        var collection = new Collection { Name = "" };

        await Assert.ThrowsAsync<ArgumentException>(() =>
            _service.CreateCollectionAsync(collection));
    }

    [Fact]
    public async Task ArchiveCollection_TogglesArchivedFlag()
    {
        var collection = new Collection { Id = 1, Name = "Test", IsArchived = false };
        _repo.GetByIdAsync(1).Returns(collection);
        _repo.UpdateAsync(Arg.Any<Collection>()).Returns(1);

        await _service.ArchiveCollectionAsync(1);

        await _repo.Received(1).UpdateAsync(Arg.Is<Collection>(c => c.IsArchived == true));
    }

    [Fact]
    public async Task DuplicateCollection_CreatesNewWithCopySuffix()
    {
        var source = new Collection { Id = 1, Name = "Birthday Ideas", Icon = "🎂" };
        _repo.GetByIdAsync(1).Returns(source);
        _repo.CreateAsync(Arg.Any<Collection>()).Returns(2);

        await _service.DuplicateCollectionAsync(1);

        await _repo.Received(1).CreateAsync(Arg.Is<Collection>(c =>
            c.Name == "Birthday Ideas (Copy)" && c.Icon == "🎂"));
    }

    [Fact]
    public async Task GetActiveCollections_CallsRepository()
    {
        _repo.GetActiveAsync().Returns(new List<Collection>
        {
            new() { Id = 1, Name = "List 1" },
            new() { Id = 2, Name = "List 2" }
        });

        var result = await _service.GetActiveCollectionsAsync();

        Assert.Equal(2, result.Count);
    }
}
