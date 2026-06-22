using Crystal.Core.Authorization;
using Crystal.Core.DTOs.Requests;
using Crystal.Core.DTOs.Responses;
using Crystal.Core.Interfaces.Services;
using Crystal.API.Extensions;
using Crystal.API.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Crystal.API.Controllers;

[ApiController]
[Route("api/time-entries")]
[Authorize]
public class TimeEntriesController : ControllerBase
{
    private readonly ITimeEntryService m_timeEntryService;
    private readonly IPunchEligibilityService m_punchEligibilityService;

    public TimeEntriesController(
        ITimeEntryService p_timeEntryService,
        IPunchEligibilityService p_punchEligibilityService)
    {
        m_timeEntryService = p_timeEntryService;
        m_punchEligibilityService = p_punchEligibilityService;
    }

    [HttpGet]
    [RequirePermission(PermissionActions.Read, PermissionSubjects.TimeEntry)]
    public async Task<ActionResult<IEnumerable<TimeEntryResponseDto>>> GetAll()
    {
        string? userId = this.GetCurrentUserId();
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized();
        }

        IEnumerable<TimeEntryResponseDto> timeEntries = await m_timeEntryService.GetAllAsync(userId);
        return Ok(timeEntries);
    }

    [HttpGet("me/active")]
    [RequirePermission(PermissionActions.Read, PermissionSubjects.TimeEntry)]
    public async Task<ActionResult<TimeEntryResponseDto?>> GetActive()
    {
        string? userId = this.GetCurrentUserId();
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized();
        }

        TimeEntryResponseDto? activeEntry = await m_timeEntryService.GetActiveAsync(userId);
        if (activeEntry is null)
        {
            return NoContent();
        }

        return Ok(activeEntry);
    }

    [HttpGet("me/punch-eligibility")]
    [RequirePermission(PermissionActions.Read, PermissionSubjects.TimeEntry)]
    public async Task<ActionResult<PunchEligibilityDto>> GetPunchEligibility()
    {
        string? userId = this.GetCurrentUserId();
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized();
        }

        PunchEligibilityDto eligibility = await m_punchEligibilityService.EvaluateAsync(userId);
        return Ok(eligibility);
    }

    [HttpPost("me/punch-in")]
    [RequirePermission(PermissionActions.Create, PermissionSubjects.TimeEntry)]
    public async Task<ActionResult<TimeEntryResponseDto>> PunchIn()
    {
        string? userId = this.GetCurrentUserId();
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized();
        }

        TimeEntryResponseDto created = await m_timeEntryService.PunchInAsync(userId);
        return Ok(created);
    }

    [HttpPost("me/punch-out")]
    [RequirePermission(PermissionActions.Create, PermissionSubjects.TimeEntry)]
    public async Task<ActionResult<TimeEntryResponseDto>> PunchOut()
    {
        string? userId = this.GetCurrentUserId();
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized();
        }

        TimeEntryResponseDto updated = await m_timeEntryService.PunchOutAsync(userId);
        return Ok(updated);
    }

    [HttpGet("{id:int}")]
    [RequirePermission(PermissionActions.Read, PermissionSubjects.TimeEntry)]
    public async Task<ActionResult<TimeEntryResponseDto>> GetById([FromRoute(Name = "id")] int p_id)
    {
        string? userId = this.GetCurrentUserId();
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized();
        }

        TimeEntryResponseDto? timeEntry = await m_timeEntryService.GetByIdAsync(p_id, userId);

        if (timeEntry is null)
        {
            return NotFound();
        }

        return Ok(timeEntry);
    }

    [HttpPost]
    [RequirePermission(PermissionActions.Create, PermissionSubjects.TimeEntry)]
    public async Task<ActionResult<TimeEntryResponseDto>> Create([FromBody] CreateTimeEntryRequest p_request)
    {
        string? userId = this.GetCurrentUserId();
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized();
        }

        TimeEntryResponseDto created = await m_timeEntryService.CreateAsync(p_request, userId);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:int}")]
    [RequirePermission(PermissionActions.Manage, PermissionSubjects.TimeEntry)]
    public async Task<ActionResult<TimeEntryResponseDto>> Update(
        [FromRoute(Name = "id")] int p_id,
        [FromBody] UpdateTimeEntryRequest p_request)
    {
        TimeEntryResponseDto updated = await m_timeEntryService.UpdateAsync(p_id, p_request);
        return Ok(updated);
    }

    [HttpDelete("{id:int}")]
    [RequirePermission(PermissionActions.Manage, PermissionSubjects.TimeEntry)]
    public async Task<IActionResult> Delete([FromRoute(Name = "id")] int p_id)
    {
        await m_timeEntryService.DeleteAsync(p_id);
        return NoContent();
    }
}
