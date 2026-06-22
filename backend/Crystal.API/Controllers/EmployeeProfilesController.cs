using Crystal.API.Extensions;
using Crystal.Core.Authorization;
using Crystal.Core.DTOs.Requests;
using Crystal.Core.DTOs.Responses;
using Crystal.Core.Interfaces.Services;
using Crystal.API.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Crystal.API.Controllers;

[ApiController]
[Route("api/employee-profiles")]
[Authorize]
public class EmployeeProfilesController : ControllerBase
{
    private readonly IEmployeeProfileService m_employeeProfileService;

    public EmployeeProfilesController(IEmployeeProfileService p_employeeProfileService)
    {
        m_employeeProfileService = p_employeeProfileService;
    }

    [HttpGet]
    [RequirePermission(PermissionActions.Read, PermissionSubjects.EmployeeProfile)]
    public async Task<ActionResult<IEnumerable<EmployeeProfileResponseDto>>> GetAll()
    {
        string? applicationUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrWhiteSpace(applicationUserId))
        {
            return Unauthorized();
        }

        IEnumerable<EmployeeProfileResponseDto> employeeProfiles =
            await m_employeeProfileService.GetAllAsync(applicationUserId);
        return Ok(employeeProfiles);
    }

    [HttpGet("me")]
    [RequirePermission(PermissionActions.Read, PermissionSubjects.EmployeeProfile)]
    public async Task<ActionResult<EmployeeProfileResponseDto>> GetMyProfile()
    {
        string? applicationUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrWhiteSpace(applicationUserId))
        {
            return Unauthorized();
        }

        EmployeeProfileResponseDto employeeProfile = await m_employeeProfileService.GetMyProfileAsync(applicationUserId);
        return Ok(employeeProfile);
    }

    [HttpGet("{id:int}")]
    [RequirePermission(PermissionActions.Read, PermissionSubjects.EmployeeProfile)]
    public async Task<ActionResult<EmployeeProfileResponseDto>> GetById([FromRoute(Name = "id")] int p_id)
    {
        string? applicationUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrWhiteSpace(applicationUserId))
        {
            return Unauthorized();
        }

        EmployeeProfileResponseDto? employeeProfile =
            await m_employeeProfileService.GetByIdAsync(p_id, applicationUserId);

        if (employeeProfile is null)
        {
            return NotFound();
        }

        return Ok(employeeProfile);
    }

    [HttpPost]
    [RequirePermission(PermissionActions.Create, PermissionSubjects.EmployeeProfile)]
    public async Task<ActionResult<EmployeeProfileResponseDto>> Create([FromBody] CreateEmployeeProfileRequest p_request)
    {
        EmployeeProfileResponseDto created = await m_employeeProfileService.CreateAsync(p_request);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:int}")]
    [RequirePermission(PermissionActions.Update, PermissionSubjects.EmployeeProfile)]
    public async Task<ActionResult<EmployeeProfileResponseDto>> Update(
        [FromRoute(Name = "id")] int p_id,
        [FromBody] UpdateEmployeeProfileRequest p_request)
    {
        EmployeeProfileResponseDto updated = await m_employeeProfileService.UpdateAsync(p_id, p_request);
        return Ok(updated);
    }

    [HttpDelete("{id:int}")]
    [RequirePermission(PermissionActions.Delete, PermissionSubjects.EmployeeProfile)]
    public async Task<IActionResult> Delete([FromRoute(Name = "id")] int p_id)
    {
        await m_employeeProfileService.DeleteAsync(p_id);
        return NoContent();
    }
}
