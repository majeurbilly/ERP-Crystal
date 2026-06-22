using Crystal.Core.Authorization;
using Crystal.Core.DTOs.Responses;
using Crystal.Core.Interfaces.Services;
using Crystal.API.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Crystal.API.Controllers;

[ApiController]
[Route("api/permission-entities")]
[Authorize]
public class PermissionEntitiesController : ControllerBase
{
    private readonly IDynamicRoleService m_dynamicRoleService;

    public PermissionEntitiesController(IDynamicRoleService p_dynamicRoleService)
    {
        m_dynamicRoleService = p_dynamicRoleService;
    }

    [HttpGet]
    [RequirePermission(PermissionActions.Read, PermissionSubjects.UserRole)]
    public ActionResult<IEnumerable<PermissionEntityResponseDto>> GetAll()
    {
        IEnumerable<PermissionEntityResponseDto> entities = m_dynamicRoleService.GetPermissionEntities();
        return Ok(entities);
    }
}
