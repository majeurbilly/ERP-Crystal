using Crystal.Core.Authorization;
using Crystal.Core.DTOs.Requests;
using Crystal.Core.DTOs.Responses;
using Crystal.Core.Interfaces.Services;
using Crystal.API.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Crystal.API.Controllers;

[ApiController]
[Route("api/job-positions")]
[Authorize]
public class JobPositionsController : ControllerBase
{
    private readonly IJobPositionService m_jobPositionService;

    public JobPositionsController(IJobPositionService p_jobPositionService)
    {
        m_jobPositionService = p_jobPositionService;
    }

    [HttpGet]
    [RequirePermission(PermissionActions.Read, PermissionSubjects.JobPosition)]
    public async Task<ActionResult<IEnumerable<JobPositionResponseDto>>> GetAll()
    {
        IEnumerable<JobPositionResponseDto> jobPositions = await m_jobPositionService.GetAllAsync();
        return Ok(jobPositions);
    }

    [HttpGet("{id:int}")]
    [RequirePermission(PermissionActions.Read, PermissionSubjects.JobPosition)]
    public async Task<ActionResult<JobPositionResponseDto>> GetById([FromRoute(Name = "id")] int p_id)
    {
        JobPositionResponseDto? jobPosition = await m_jobPositionService.GetByIdAsync(p_id);

        if (jobPosition is null)
        {
            return NotFound();
        }

        return Ok(jobPosition);
    }

    [HttpPost]
    [RequirePermission(PermissionActions.Manage, PermissionSubjects.JobPosition)]
    public async Task<ActionResult<JobPositionResponseDto>> Create([FromBody] CreateJobPositionRequest p_request)
    {
        JobPositionResponseDto created = await m_jobPositionService.CreateAsync(p_request);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:int}")]
    [RequirePermission(PermissionActions.Manage, PermissionSubjects.JobPosition)]
    public async Task<ActionResult<JobPositionResponseDto>> Update(
        [FromRoute(Name = "id")] int p_id,
        [FromBody] UpdateJobPositionRequest p_request)
    {
        JobPositionResponseDto updated = await m_jobPositionService.UpdateAsync(p_id, p_request);
        return Ok(updated);
    }

    [HttpDelete("{id:int}")]
    [RequirePermission(PermissionActions.Manage, PermissionSubjects.JobPosition)]
    public async Task<IActionResult> Delete([FromRoute(Name = "id")] int p_id)
    {
        await m_jobPositionService.DeleteAsync(p_id);
        return NoContent();
    }
}
