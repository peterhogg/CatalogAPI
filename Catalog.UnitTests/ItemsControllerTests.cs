using Catalog.Api.Controllers;
using Catalog.Api.Dtos;
using Catalog.Api.Entities;
using Catalog.Api.Repositories;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;

namespace Catalog.UnitTests;

public class ItemsControllerTest
{

    private readonly Mock<IItemsRepository> repositoryStub = new();
    private readonly Mock<ILogger<ItemsController>> loggerStub = new();
    private readonly Random rand = new();

    [Fact]
    public async Task GetItemAsycn_WithUnexistingItem_ReturnsNotFound()
    {
        // Arrange
        repositoryStub.Setup(repo => repo.GetItemAsync(It.IsAny<Guid>()))
        .ReturnsAsync((Item)null);

        var controller = new ItemsController(repositoryStub.Object, loggerStub.Object);

        // Act
        var result = await controller.GetItemAsycn(Guid.NewGuid());

        // Assert
        result.Result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task GetItemAsycn_WithExistingItem_ReturnsExpectedItem()
    {
        // Arrange
        var expectedItem = CreateRandomItem();
        repositoryStub.Setup(repo => repo.GetItemAsync(It.IsAny<Guid>()))
            .ReturnsAsync(expectedItem);
        var controller = new ItemsController(repositoryStub.Object, loggerStub.Object);

        // Act
        var result = await controller.GetItemAsycn(Guid.NewGuid());
        // Assert
        result.Value.Should().BeEquivalentTo(expectedItem);
    }

    [Fact]
    public async Task GetItemsAsycn_WithExistingItems_ReturnsAllItems()
    {
        // Arrange
        var expectedItems = new[] { CreateRandomItem(), CreateRandomItem(), CreateRandomItem() };
        repositoryStub.Setup(repo => repo.GetItemsAsync())
            .ReturnsAsync(expectedItems);
        var controller = new ItemsController(repositoryStub.Object, loggerStub.Object);

        // Act
        var result = await controller.GetItemsAsync();

        // Assert
        result.Should().BeEquivalentTo(expectedItems);
    }

    [Fact]
    public async Task GetItemsAsycn_WithMatchingItems_ReturnsMatchingItems()
    {
        // Arrange
        var allItems = new[] {
            new Item(){Name="Potion"},
            new Item(){Name="Sword"},
            new Item(){Name="Strength-Potion"},
        };

        var nameToMatch = "Potion";
        repositoryStub.Setup(repo => repo.GetItemsAsync())
            .ReturnsAsync(allItems);
        var controller = new ItemsController(repositoryStub.Object, loggerStub.Object);

        // Act
        IEnumerable<ItemDto> foundItems = await controller.GetItemsAsync(nameToMatch);

        // Assert
        foundItems.Should().OnlyContain(
            item => item.Name == allItems[0].Name || item.Name == allItems[2].Name
        );
    }

    [Fact]
    public async Task CreateItemAsycn_WithItemToCreate_ReturnsCreatedItem()
    {
        // Arrange
        var itemToCreate = new CreateItemDto(Guid.NewGuid().ToString(), Guid.NewGuid().ToString(), rand.Next(1000));

        var controller = new ItemsController(repositoryStub.Object, loggerStub.Object);

        // Act
        var result = await controller.CreateItemAsync(itemToCreate);

        // Assert
        var createdItem = ((result.Result) as CreatedAtActionResult).Value as ItemDto;
        itemToCreate.Should().BeEquivalentTo(
            createdItem,
            options => options.ComparingByMembers<ItemDto>().ExcludingMissingMembers()
        );
        createdItem.Id.Should().NotBeEmpty();
        createdItem.CreatedDate.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public async Task UpdateItemAsycn_WithExistingItem_ReturnsNoContent()
    {
        // Arrange
        Item existingItem = CreateRandomItem();
        repositoryStub.Setup(repo => repo.GetItemAsync(It.IsAny<Guid>()))
            .ReturnsAsync(existingItem);
        var controller = new ItemsController(repositoryStub.Object, loggerStub.Object);

        var itemId = existingItem.Id;
        var itemToUpdate = new UpdateItemDto(Guid.NewGuid().ToString(), Guid.NewGuid().ToString(), Math.Min((existingItem.Price + 3), 1000));

        // Act
        var result = await controller.UpdateItemAsync(itemId, itemToUpdate);

        // Assert
        result.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public async Task DeleteItemAsycn_WithExistingItem_ReturnsNoContent()
    {
        // Arrange
        Item itemToDelete = CreateRandomItem();
        repositoryStub.Setup(repo => repo.GetItemAsync(It.IsAny<Guid>()))
            .ReturnsAsync(itemToDelete);
        var controller = new ItemsController(repositoryStub.Object, loggerStub.Object);

        var itemId = itemToDelete.Id;

        // Act
        var result = await controller.DeleteItemAsycn(itemId);

        // Assert
        result.Should().BeOfType<NoContentResult>();
    }

    private Item CreateRandomItem()
    {
        return new()
        {
            Id = Guid.NewGuid(),
            Name = Guid.NewGuid().ToString(),
            Price = rand.Next(1000),
            CreatedDate = DateTimeOffset.UtcNow
        };
    }
}