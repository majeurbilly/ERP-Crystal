using Crystal.API.Extensions;
using Crystal.API.Authorization;
using Crystal.Core.Authorization;
using Crystal.Core.DTOs.Requests;
using Crystal.Core.DTOs.Responses;
using Crystal.Core.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Crystal.API.Controllers;

[ApiController]
[Route("api/inventory")]
[Authorize]
public class InventoryController : ControllerBase
{
    private readonly IInventoryService m_inventoryService;
    private readonly IPermissionService m_permissionService;

    public InventoryController(
        IInventoryService p_inventoryService,
        IPermissionService p_permissionService)
    {
        m_inventoryService = p_inventoryService;
        m_permissionService = p_permissionService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<LocationInventoryLineResponseDto>>> GetInventory(
        [FromQuery] int? p_locationId = null,
        [FromQuery] int? p_itemId = null)
    {
        IEnumerable<LocationInventoryLineResponseDto> lines =
            await m_inventoryService.GetInventoryLinesAsync(p_locationId, p_itemId);

        return Ok(lines);
    }

    [HttpPut("quantity")]
    public async Task<IActionResult> UpdateQuantity([FromBody] UpdateInventoryQuantityRequest p_request)
    {
        IActionResult? authorizationResult = await AuthorizeInventoryUpdateAsync(p_request.LocationId);
        if (authorizationResult is not null)
        {
            return authorizationResult;
        }

        await m_inventoryService.UpdateQuantityAsync(p_request);
        return NoContent();
    }

    [HttpPost("quantity/add")]
    public async Task<IActionResult> AddQuantity([FromBody] UpdateInventoryQuantityRequest p_request)
    {
        IActionResult? authorizationResult = await AuthorizeInventoryUpdateAsync(p_request.LocationId);
        if (authorizationResult is not null)
        {
            return authorizationResult;
        }

        await m_inventoryService.AddQuantityAsync(p_request);
        return NoContent();
    }

    [HttpGet("locations/{p_locationId:int}/items/{p_itemId:int}")]
    public async Task<ActionResult<InventoryStockResponseDto>> GetStock(
        int p_locationId,
        int p_itemId)
    {
        InventoryStockResponseDto stock = await m_inventoryService.GetStockAsync(p_locationId, p_itemId);
        return Ok(stock);
    }

    [HttpPut("locations/{p_locationId:int}/items/{p_itemId:int}")]
    public async Task<IActionResult> SetStock(
        int p_locationId,
        int p_itemId,
        [FromBody] UpdateStockRequest p_request)
    {
        IActionResult? authorizationResult = await AuthorizeInventoryUpdateAsync(p_locationId);
        if (authorizationResult is not null)
        {
            return authorizationResult;
        }

        await m_inventoryService.SetStockAsync(p_locationId, p_itemId, p_request);
        return NoContent();
    }

    [HttpPost("import")]
    [Consumes("multipart/form-data")]
    [ApiExplorerSettings(IgnoreApi = true)]
    [RequirePermission(PermissionActions.Update, PermissionSubjects.InventoryQuantity)]
    public async Task<IActionResult> ImportExcel([FromForm] IFormFile p_file)
    {
        await using Stream stream = p_file.OpenReadStream();
        await m_inventoryService.ImportFromExcelAsync(stream, p_file.FileName);
        return NoContent();
    }

    private async Task<IActionResult?> AuthorizeInventoryUpdateAsync(int p_locationId)
    {
        string? userId = this.GetCurrentUserId();
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized();
        }

        bool hasPermission = await m_permissionService.UserHasPermissionForLocationAsync(
            userId,
            PermissionActions.Update,
            PermissionSubjects.InventoryQuantity,
            p_locationId);

        if (!hasPermission)
        {
            return StatusCode(
                StatusCodes.Status403Forbidden,
                "Vous n'avez pas la permission de modifier l'inventaire pour cette succursale.");
        }

        return null;
    }
}
