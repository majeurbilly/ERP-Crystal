using Crystal.Core.Authorization;
using Crystal.Core.DTOs.Requests;
using Crystal.Core.DTOs.Responses;
using Crystal.Core.Interfaces.Services;
using Crystal.API.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Crystal.API.Controllers;

[ApiController]
[Route("api/locations")]
[Authorize]
public class LocationsController : ControllerBase
{
    private readonly ILocationService m_locationService;

    public LocationsController(ILocationService p_locationService)
    {
        m_locationService = p_locationService;
    }

    [HttpGet]
    [RequirePermission(PermissionActions.Read, PermissionSubjects.Location)]
    public async Task<ActionResult<IEnumerable<LocationResponseDto>>> GetAll()
    {
        IEnumerable<LocationResponseDto> locations = await m_locationService.GetAllAsync();
        return Ok(locations);
    }

    [HttpGet("dropdown")]
    [RequirePermission(PermissionActions.Read, PermissionSubjects.Location)]
    public async Task<ActionResult<List<LocationOptionResponseDto>>> GetDropdown()
    {
        List<LocationOptionResponseDto> options = await m_locationService.GetDropdownOptionsAsync();
        return Ok(options);
    }

    [HttpGet("{id:int}")]
    [RequirePermission(PermissionActions.Read, PermissionSubjects.Location)]
    public async Task<ActionResult<LocationResponseDto>> GetById([FromRoute(Name = "id")] int p_id)
    {
        LocationResponseDto? location = await m_locationService.GetByIdAsync(p_id);

        if (location is null)
        {
            return NotFound();
        }

        return Ok(location);
    }

    [HttpPost]
    [RequirePermission(PermissionActions.Manage, PermissionSubjects.Location)]
    public async Task<ActionResult<LocationResponseDto>> Create([FromBody] CreateLocationRequestDto p_request)
    {
        LocationResponseDto created = await m_locationService.CreateAsync(p_request);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:int}")]
    [RequirePermission(PermissionActions.Manage, PermissionSubjects.Location)]
    public async Task<ActionResult<LocationResponseDto>> Update(
        [FromRoute(Name = "id")] int p_id,
        [FromBody] UpdateLocationRequestDto p_request)
    {
        LocationResponseDto updated = await m_locationService.UpdateAsync(p_id, p_request);
        return Ok(updated);
    }

    [HttpDelete("{id:int}")]
    [RequirePermission(PermissionActions.Manage, PermissionSubjects.Location)]
    public async Task<IActionResult> Delete([FromRoute(Name = "id")] int p_id)
    {
        await m_locationService.DeleteAsync(p_id);
        return NoContent();
    }
}
