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
[Route("api/leave-requests")]
[Authorize]
public class LeaveRequestsController : ControllerBase
{
    private readonly ILeaveRequestService m_leaveRequestService;

    public LeaveRequestsController(ILeaveRequestService p_leaveRequestService)
    {
        m_leaveRequestService = p_leaveRequestService;
    }

    [HttpGet]
    [RequirePermission(PermissionActions.Read, PermissionSubjects.LeaveRequest)]
    public async Task<ActionResult<IEnumerable<LeaveRequestResponseDto>>> GetAll()
    {
        string? userId = this.GetCurrentUserId();
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized();
        }

        IEnumerable<LeaveRequestResponseDto> leaveRequests = await m_leaveRequestService.GetAllAsync(userId);
        return Ok(leaveRequests);
    }

    [HttpGet("{id:int}")]
    [RequirePermission(PermissionActions.Read, PermissionSubjects.LeaveRequest)]
    public async Task<ActionResult<LeaveRequestResponseDto>> GetById([FromRoute(Name = "id")] int p_id)
    {
        string? userId = this.GetCurrentUserId();
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized();
        }

        LeaveRequestResponseDto? leaveRequest = await m_leaveRequestService.GetByIdAsync(p_id, userId);

        if (leaveRequest is null)
        {
            return NotFound();
        }

        return Ok(leaveRequest);
    }

    [HttpPost]
    [RequirePermission(PermissionActions.Create, PermissionSubjects.LeaveRequest)]
    public async Task<ActionResult<LeaveRequestResponseDto>> Create([FromBody] CreateLeaveRequestDto p_request)
    {
        string? userId = this.GetCurrentUserId();
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized();
        }

        LeaveRequestResponseDto created = await m_leaveRequestService.CreateAsync(p_request, userId);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPatch("{id:int}/status")]
    [RequirePermission(PermissionActions.Manage, PermissionSubjects.LeaveRequest)]
    public async Task<ActionResult<LeaveRequestResponseDto>> UpdateStatus(
        [FromRoute(Name = "id")] int p_id,
        [FromBody] UpdateLeaveRequestStatusDto p_request)
    {
        LeaveRequestResponseDto updated = await m_leaveRequestService.UpdateStatusAsync(p_id, p_request);
        return Ok(updated);
    }

    [HttpDelete("{id:int}")]
    [RequirePermission(PermissionActions.Manage, PermissionSubjects.LeaveRequest)]
    public async Task<IActionResult> Delete([FromRoute(Name = "id")] int p_id)
    {
        await m_leaveRequestService.DeleteAsync(p_id);
        return NoContent();
    }
}
