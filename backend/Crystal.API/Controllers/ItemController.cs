using Crystal.Core.DTOs.Requests;
using Crystal.Core.DTOs.Responses;
using Crystal.Core.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Crystal.API.Controllers;

[ApiController]
[Route("api/items")]
[Authorize]
public class ItemController : ControllerBase
{
    private readonly IItemService m_itemService;

    public ItemController(IItemService p_itemService) => m_itemService = p_itemService;

    [HttpGet]
    public async Task<IActionResult> GetItems(CancellationToken p_cancellationToken)
    {
        IEnumerable<ItemResponse> items = await m_itemService.GetAllItemsAsync(p_cancellationToken);
        return Ok(items);
    }

    [HttpGet("{p_id:int}")]
    public async Task<IActionResult> GetItemById(int p_id, CancellationToken p_cancellationToken)
    {
        ItemResponse? item = await m_itemService.GetItemByIdAsync(p_id, p_cancellationToken);
        if (item == null)
        {
            return NotFound();
        }

        return Ok(item);
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Gerant")]
    public async Task<IActionResult> CreateItem([FromBody] CreateItemRequest p_request, CancellationToken p_cancellationToken)
    {
        ItemResponse newItem = await m_itemService.CreateItemAsync(p_request, p_cancellationToken);
        return CreatedAtAction(nameof(GetItemById), new { p_id = newItem.Id }, newItem);
    }

    [HttpPut("{p_id:int}")]
    [Authorize(Roles = "Admin,Gerant")]
    public async Task<IActionResult> UpdateItem(int p_id, [FromBody] UpdateItemRequest p_request, CancellationToken p_cancellationToken)
    {
        ItemResponse? updatedItem = await m_itemService.UpdateItemAsync(p_id, p_request, p_cancellationToken);
        if (updatedItem == null)
        {
            return NotFound();
        }

        return Ok(updatedItem);
    }

    [HttpDelete("{p_id:int}")]
    [Authorize(Roles = "Admin,Gerant")]
    public async Task<IActionResult> DeleteItem(int p_id, CancellationToken p_cancellationToken)
    {
        bool isDeleted = await m_itemService.DeleteItemAsync(p_id, p_cancellationToken);
        if (!isDeleted)
        {
            return NotFound();
        }

        return NoContent();
    }
}