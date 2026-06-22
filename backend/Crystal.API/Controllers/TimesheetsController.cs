using Crystal.Core.Authorization;
using Crystal.Core.DTOs.Requests;
using Crystal.Core.DTOs.Responses;
using Crystal.Core.Enums;
using Crystal.Core.Interfaces.Services;
using Crystal.API.Extensions;
using Crystal.API.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Crystal.API.Controllers;

[ApiController]
[Route("api/timesheets")]
[Authorize]
public class TimesheetsController : ControllerBase
{
    private readonly ITimesheetService m_timesheetService;
    private readonly IPermissionService m_permissionService;

    public TimesheetsController(
        ITimesheetService p_timesheetService,
        IPermissionService p_permissionService)
    {
        m_timesheetService = p_timesheetService;
        m_permissionService = p_permissionService;
    }

    [HttpGet]
    [RequirePermission(PermissionActions.Read, PermissionSubjects.Timesheet)]
    public async Task<ActionResult<IEnumerable<TimesheetResponseDto>>> GetAll()
    {
        string? userId = this.GetCurrentUserId();
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized();
        }

        IEnumerable<TimesheetResponseDto> timesheets = await m_timesheetService.GetAllAsync(userId);
        return Ok(timesheets);
    }

    [HttpGet("{id:int}")]
    [RequirePermission(PermissionActions.Read, PermissionSubjects.Timesheet)]
    public async Task<ActionResult<TimesheetResponseDto>> GetById([FromRoute(Name = "id")] int p_id)
    {
        string? userId = this.GetCurrentUserId();
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized();
        }

        TimesheetResponseDto? timesheet = await m_timesheetService.GetByIdAsync(p_id, userId);

        if (timesheet is null)
        {
            return NotFound();
        }

        return Ok(timesheet);
    }

    [HttpPost]
    [RequirePermission(PermissionActions.Manage, PermissionSubjects.Timesheet)]
    public async Task<ActionResult<TimesheetResponseDto>> Create([FromBody] CreateTimesheetRequest p_request)
    {
        TimesheetResponseDto created = await m_timesheetService.CreateAsync(p_request);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPost("generate-weekly")]
    [RequirePermission(PermissionActions.Create, PermissionSubjects.Timesheet)]
    public async Task<ActionResult<GenerateWeeklyTimesheetsResponseDto>> GenerateWeekly(
        [FromBody] GenerateWeeklyTimesheetsRequest p_request)
    {
        string? userId = this.GetCurrentUserId();
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized();
        }

        GenerateWeeklyTimesheetsResponseDto generated = await m_timesheetService.GenerateWeeklyAsync(p_request, userId);
        return Ok(generated);
    }

    [HttpPut("{id:int}")]
    [RequirePermission(PermissionActions.Manage, PermissionSubjects.Timesheet)]
    public async Task<ActionResult<TimesheetResponseDto>> Update(
        [FromRoute(Name = "id")] int p_id,
        [FromBody] CreateTimesheetRequest p_request)
    {
        TimesheetResponseDto updated = await m_timesheetService.UpdateAsync(p_id, p_request);
        return Ok(updated);
    }

    [HttpPatch("{id:int}/status")]
    [RequirePermission(PermissionActions.Submit, PermissionSubjects.Timesheet)]
    public async Task<ActionResult<TimesheetResponseDto>> UpdateStatus(
        [FromRoute(Name = "id")] int p_id,
        [FromBody] UpdateTimesheetStatusRequest p_request)
    {
        string? userId = this.GetCurrentUserId();
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized();
        }

        if (p_request.Status == TimesheetStatus.Approved || p_request.Status == TimesheetStatus.Rejected)
        {
            bool canApprove = await m_permissionService.UserHasPermissionAsync(
                userId,
                PermissionActions.Approve,
                PermissionSubjects.Timesheet);

            if (!canApprove)
            {
                return Forbid();
            }
        }

        TimesheetResponseDto updated = await m_timesheetService.UpdateStatusAsync(p_id, p_request, userId);
        return Ok(updated);
    }

    [HttpPatch("{id:int}/paid")]
    [RequirePermission(PermissionActions.Manage, PermissionSubjects.Timesheet)]
    public async Task<ActionResult<TimesheetResponseDto>> UpdatePaid(
        [FromRoute(Name = "id")] int p_id,
        [FromBody] UpdateTimesheetPaidRequest p_request)
    {
        TimesheetResponseDto updated = await m_timesheetService.UpdatePaidAsync(p_id, p_request);
        return Ok(updated);
    }

    [HttpPost("{id:int}/reload-time-entries")]
    [RequirePermission(PermissionActions.Manage, PermissionSubjects.Timesheet)]
    public async Task<ActionResult<TimesheetResponseDto>> ReloadTimeEntries(
        [FromRoute(Name = "id")] int p_id)
    {
        TimesheetResponseDto updated = await m_timesheetService.ReloadTimeEntriesAsync(p_id);
        return Ok(updated);
    }

    [HttpPost("{id:int}/time-entries")]
    [RequirePermission(PermissionActions.Manage, PermissionSubjects.Timesheet)]
    public async Task<ActionResult<TimesheetResponseDto>> AddTimeEntry(
        [FromRoute(Name = "id")] int p_id,
        [FromBody] CreateTimeEntryRequest p_request)
    {
        TimesheetResponseDto updated = await m_timesheetService.AddTimeEntryAsync(p_id, p_request);
        return Ok(updated);
    }

    [HttpPut("{id:int}/time-entries/{timeEntryId:int}")]
    [RequirePermission(PermissionActions.Manage, PermissionSubjects.Timesheet)]
    public async Task<ActionResult<TimesheetResponseDto>> UpdateTimeEntry(
        [FromRoute(Name = "id")] int p_id,
        [FromRoute(Name = "timeEntryId")] int p_timeEntryId,
        [FromBody] UpdateTimeEntryRequest p_request)
    {
        TimesheetResponseDto updated = await m_timesheetService.UpdateTimeEntryAsync(p_id, p_timeEntryId, p_request);
        return Ok(updated);
    }

    [HttpDelete("{id:int}/time-entries/{timeEntryId:int}")]
    [RequirePermission(PermissionActions.Manage, PermissionSubjects.Timesheet)]
    public async Task<ActionResult<TimesheetResponseDto>> RemoveTimeEntry(
        [FromRoute(Name = "id")] int p_id,
        [FromRoute(Name = "timeEntryId")] int p_timeEntryId)
    {
        TimesheetResponseDto updated = await m_timesheetService.RemoveTimeEntryAsync(p_id, p_timeEntryId);
        return Ok(updated);
    }

    [HttpDelete("{id:int}")]
    [RequirePermission(PermissionActions.Manage, PermissionSubjects.Timesheet)]
    public async Task<IActionResult> Delete([FromRoute(Name = "id")] int p_id)
    {
        await m_timesheetService.DeleteAsync(p_id);
        return NoContent();
    }
}
