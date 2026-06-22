using Crystal.Core;
using Crystal.Core.DTOs.Requests;
using Crystal.Core.DTOs.Responses;
using Crystal.Core.Interfaces.Services;
using Crystal.Core.Authorization;
using Crystal.API.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Crystal.API.Controllers;

[ApiController]
[Route("api/roles")]
[Authorize]
public class RolesController : ControllerBase
{
    private readonly IDynamicRoleService m_dynamicRoleService;

    public RolesController(IDynamicRoleService p_dynamicRoleService)
    {
        m_dynamicRoleService = p_dynamicRoleService;
    }

    [HttpGet]
    [RequirePermission(PermissionActions.Read, PermissionSubjects.UserRole)]
    public async Task<ActionResult<IEnumerable<DynamicRoleResponseDto>>> GetAll()
    {
        IEnumerable<DynamicRoleResponseDto> roles = await m_dynamicRoleService.GetAllAsync();
        return Ok(roles);
    }

    [HttpGet("{id}")]
    [RequirePermission(PermissionActions.Read, PermissionSubjects.UserRole)]
    public async Task<ActionResult<DynamicRoleResponseDto>> GetById([FromRoute(Name = "id")] string p_id)
    {
        DynamicRoleResponseDto? role = await m_dynamicRoleService.GetByIdAsync(p_id);

        if (role is null)
        {
            return NotFound();
        }

        return Ok(role);
    }

    [HttpPost]
    [RequirePermission(PermissionActions.Create, PermissionSubjects.UserRole)]
    public async Task<ActionResult<DynamicRoleResponseDto>> Create([FromBody] CreateDynamicRoleRequest p_request)
    {
        DynamicRoleResponseDto created = await m_dynamicRoleService.CreateAsync(p_request);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id}")]
    [RequirePermission(PermissionActions.Update, PermissionSubjects.UserRole)]
    public async Task<ActionResult<DynamicRoleResponseDto>> Update(
        [FromRoute(Name = "id")] string p_id,
        [FromBody] UpdateDynamicRoleRequest p_request)
    {
        DynamicRoleResponseDto updated = await m_dynamicRoleService.UpdateAsync(p_id, p_request);
        return Ok(updated);
    }

    [HttpDelete("{id}")]
    [RequirePermission(PermissionActions.Delete, PermissionSubjects.UserRole)]
    public async Task<IActionResult> Delete([FromRoute(Name = "id")] string p_id)
    {
        await m_dynamicRoleService.DeleteAsync(p_id);
        return NoContent();
    }
}
