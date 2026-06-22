using Crystal.Core.Authorization;
using Crystal.Core.DTOs.Requests;
using Crystal.Core.DTOs.Responses;
using Crystal.Core.Interfaces.Services;
using Crystal.API.Authorization;
using Crystal.API.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Crystal.API.Controllers;

[ApiController]
[Route("api/contracts")]
[Authorize]
public class ContractsController : ControllerBase
{
    private readonly IEmploymentContractService m_employmentContractService;

    public ContractsController(IEmploymentContractService p_employmentContractService)
    {
        m_employmentContractService = p_employmentContractService;
    }

    [HttpGet]
    [RequirePermission(PermissionActions.Read, PermissionSubjects.EmploymentContract)]
    public async Task<ActionResult<IEnumerable<EmploymentContractResponseDto>>> GetAll()
    {
        string? userId = this.GetCurrentUserId();
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized();
        }

        IEnumerable<EmploymentContractResponseDto> contracts = await m_employmentContractService.GetAllAsync(userId);
        return Ok(contracts);
    }

    [HttpGet("{id:int}")]
    [RequirePermission(PermissionActions.Read, PermissionSubjects.EmploymentContract)]
    public async Task<ActionResult<EmploymentContractResponseDto>> GetById([FromRoute(Name = "id")] int p_id)
    {
        string? userId = this.GetCurrentUserId();
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized();
        }

        EmploymentContractResponseDto? contract = await m_employmentContractService.GetByIdAsync(p_id, userId);

        if (contract is null)
        {
            return NotFound();
        }

        return Ok(contract);
    }

    [HttpPost]
    [RequirePermission(PermissionActions.Manage, PermissionSubjects.EmploymentContract)]
    public async Task<ActionResult<EmploymentContractResponseDto>> Create([FromBody] CreateEmploymentContractRequest p_request)
    {
        EmploymentContractResponseDto created = await m_employmentContractService.CreateAsync(p_request);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:int}")]
    [RequirePermission(PermissionActions.Manage, PermissionSubjects.EmploymentContract)]
    public async Task<ActionResult<EmploymentContractResponseDto>> Update(
        [FromRoute(Name = "id")] int p_id,
        [FromBody] UpdateEmploymentContractRequest p_request)
    {
        EmploymentContractResponseDto updated = await m_employmentContractService.UpdateAsync(p_id, p_request);
        return Ok(updated);
    }

    [HttpDelete("{id:int}")]
    [RequirePermission(PermissionActions.Manage, PermissionSubjects.EmploymentContract)]
    public async Task<IActionResult> Delete([FromRoute(Name = "id")] int p_id)
    {
        await m_employmentContractService.DeleteAsync(p_id);
        return NoContent();
    }
}
