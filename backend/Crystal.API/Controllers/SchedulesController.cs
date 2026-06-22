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
[Route("api/schedules")]
[Authorize]
public class SchedulesController : ControllerBase
{
    private readonly IScheduledShiftService m_scheduledShiftService;

    public SchedulesController(IScheduledShiftService p_scheduledShiftService)
    {
        m_scheduledShiftService = p_scheduledShiftService;
    }

    [HttpGet]
    [RequirePermission(PermissionActions.Read, PermissionSubjects.ScheduledShift)]
    public async Task<ActionResult<IEnumerable<ScheduledShiftResponseDto>>> GetAll()
    {
        string? userId = this.GetCurrentUserId();
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized();
        }

        IEnumerable<ScheduledShiftResponseDto> scheduledShifts = await m_scheduledShiftService.GetAllAsync(userId);
        return Ok(scheduledShifts);
    }

    [HttpGet("team")]
    [RequirePermission(PermissionActions.Read, PermissionSubjects.ScheduledShift)]
    public async Task<ActionResult<IEnumerable<ScheduledShiftResponseDto>>> GetTeamSchedule()
    {
        string? userId = this.GetCurrentUserId();
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized();
        }

        IEnumerable<ScheduledShiftResponseDto> scheduledShifts =
            await m_scheduledShiftService.GetTeamScheduleAsync(userId);
        return Ok(scheduledShifts);
    }

    [HttpGet("{id:int}")]
    [RequirePermission(PermissionActions.Read, PermissionSubjects.ScheduledShift)]
    public async Task<ActionResult<ScheduledShiftResponseDto>> GetById([FromRoute(Name = "id")] int p_id)
    {
        string? userId = this.GetCurrentUserId();
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized();
        }

        ScheduledShiftResponseDto? scheduledShift = await m_scheduledShiftService.GetByIdAsync(p_id, userId);

        if (scheduledShift is null)
        {
            return NotFound();
        }

        return Ok(scheduledShift);
    }

    [HttpPost]
    [RequirePermission(PermissionActions.Manage, PermissionSubjects.ScheduledShift)]
    public async Task<ActionResult<ScheduledShiftResponseDto>> Create([FromBody] CreateScheduledShiftRequest p_request)
    {
        ScheduledShiftResponseDto created = await m_scheduledShiftService.CreateAsync(p_request);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:int}")]
    [RequirePermission(PermissionActions.Manage, PermissionSubjects.ScheduledShift)]
    public async Task<ActionResult<ScheduledShiftResponseDto>> Update(
        [FromRoute(Name = "id")] int p_id,
        [FromBody] UpdateScheduledShiftRequest p_request)
    {
        ScheduledShiftResponseDto updated = await m_scheduledShiftService.UpdateAsync(p_id, p_request);
        return Ok(updated);
    }

    [HttpDelete("{id:int}")]
    [RequirePermission(PermissionActions.Manage, PermissionSubjects.ScheduledShift)]
    public async Task<IActionResult> Delete([FromRoute(Name = "id")] int p_id)
    {
        await m_scheduledShiftService.DeleteAsync(p_id);
        return NoContent();
    }
}
