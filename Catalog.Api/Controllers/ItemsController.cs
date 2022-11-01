using System.Linq;
using Catalog.Api.Dtos;
using Catalog.Api.Entities;
using Catalog.Api.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace Catalog.Api.Controllers;
[ApiController]
[Route("[controller]")]
public class ItemsController : ControllerBase
{
    private readonly IItemsRepository repository;
    private readonly ILogger<ItemsController> logger;

    public ItemsController(IItemsRepository repository, ILogger<ItemsController> logger)
    {
        this.repository = repository;
        this.logger = logger;
    }

    [HttpGet]
    public async Task<IEnumerable<ItemDto>> GetItemsAsync()
    {
        var items = (await repository.GetItemsAsync())
                    .Select(item => item.AsDto());
        this.logger.LogInformation($"{DateTime.UtcNow.ToString("yyyy-MM-dd hh:mm:ss")} {items.Count()} items returned");
        return items;
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ItemDto>> GetItemAsycn(Guid id)
    {
        var item = await repository.GetItemAsync(id);
        if (item is null)
        {
            return NotFound();
        }
        return item.AsDto();
    }

    [HttpPost]
    public async Task<ActionResult<ItemDto>> CreateItemAsync(CreateItemDto itemDto)
    {
        Item item = new()
        {
            Id = Guid.NewGuid(),
            Name = itemDto.Name,
            Price = itemDto.Price,
            CreatedDate = DateTimeOffset.UtcNow
        };

        await repository.CreateItemAsync(item);

        return CreatedAtAction(nameof(GetItemAsycn), new { id = item.Id }, item.AsDto());
    }

    [HttpPut("{id}")]
    public async Task<ActionResult> UpdateItemAsync(Guid id, UpdateItemDto itemDto)
    {
        Item existingItem = await repository.GetItemAsync(id);

        if (existingItem is null)
        {
            return NotFound();
        }

        Item updatedItem = existingItem with
        {
            Name = itemDto.Name,
            Price = itemDto.Price
        };

        await repository.UpdateItemAsync(updatedItem);

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteItemAsycn(Guid id)
    {
        Item existingItem = await repository.GetItemAsync(id);
        if (existingItem is null)
        {
            return NotFound();
        }
        await repository.DeleteItemAsync(id);

        return NoContent();
    }

}