using Catalog.Entities;
using Catalog.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace Catalog.Controllers;
[ApiController]
[Route("[controller]")]
public class ItemsController : ControllerBase
{
    private readonly IItemsRepository repository;

    public ItemsController(IItemsRepository repository)
    {
        this.repository = repository;
    }

    [HttpGet]
    public IEnumerable<Item> GetItems()
    {
        var items = repository.GetItems();
        return items;
    }

    [HttpGet("{id}")]
    public ActionResult<Item> GetItem(Guid id)
    {
        var item = repository.GetItem(id);
        if (item is null)
        {
            return NotFound();
        }
        return Ok(item);
    }

}