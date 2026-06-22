using Crystal.Core.Authorization;
using Crystal.Core.DTOs.Requests;
using Crystal.Core.DTOs.Responses;
using Crystal.Core.Interfaces.Services;
using Crystal.API.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
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
    [RequirePermission(PermissionActions.Read, PermissionSubjects.Item)]
    public async Task<IActionResult> GetInventory(
        [FromQuery] string? p_search = null,
        [FromQuery] int? p_publisherId = null,
        [FromQuery] int[]? p_categoryIds = null,
        [FromQuery] int? p_authorId = null,
        [FromQuery] bool? p_isBook = null)
    {
        IEnumerable<ItemResponseDto> items = await m_itemService.GetInventoryAsync(
            p_search,
            p_publisherId,
            p_categoryIds,
            p_authorId,
            p_isBook);

        return Ok(items);
    }

    [HttpGet("{p_id:int}")]
    [RequirePermission(PermissionActions.Read, PermissionSubjects.Item)]
    public async Task<IActionResult> GetItemById(int p_id)
    {
        ItemResponseDto? item = await m_itemService.GetByIdAsync(p_id);
        return item is null ? NotFound() : Ok(item);
    }

    [HttpPost]
    [RequirePermission(PermissionActions.Create, PermissionSubjects.Item)]
    public async Task<IActionResult> Create([FromBody] CreateItemRequest p_request)
    {
        ItemResponseDto item = await m_itemService.CreateAsync(p_request);
        return StatusCode(StatusCodes.Status201Created, item);
    }

    [HttpPost("books")]
    [RequirePermission(PermissionActions.Create, PermissionSubjects.Item)]
    public async Task<IActionResult> CreateBook([FromBody] CreateBookRequest p_request)
    {
        ItemResponseDto item = await m_itemService.CreateBookAsync(p_request);
        return StatusCode(StatusCodes.Status201Created, item);
    }

    [HttpPut("{p_id:int}")]
    [RequirePermission(PermissionActions.Update, PermissionSubjects.Item)]
    public async Task<IActionResult> Update(int p_id, [FromBody] UpdateItemRequest p_request)
    {
        ItemResponseDto? item = await m_itemService.UpdateAsync(p_id, p_request);

        if (item is null)
        {
            return NotFound();
        }

        return Ok(item);
    }

    [HttpDelete("{p_id:int}")]
    [RequirePermission(PermissionActions.Delete, PermissionSubjects.Item)]
    public async Task<IActionResult> Delete(int p_id)
    {
        bool deleted = await m_itemService.DeleteAsync(p_id);

        if (!deleted)
        {
            return NotFound();
        }

        return NoContent();
    }

    [HttpPost("{p_id:int}/image")]
    [Consumes("multipart/form-data")]
    [ApiExplorerSettings(IgnoreApi = true)]
    [RequirePermission(PermissionActions.Update, PermissionSubjects.Item)]
    public async Task<IActionResult> UploadImage(int p_id, [FromForm] IFormFile? p_file)
    {
        if (p_file is null || p_file.Length == 0)
        {
            return BadRequest(new { message = "Le fichier image est requis." });
        }

        await using Stream stream = p_file.OpenReadStream();
        ItemResponseDto? item = await m_itemService.UploadImageAsync(p_id, stream, p_file.FileName);

        return item is null ? NotFound() : Ok(item);
    }
}
