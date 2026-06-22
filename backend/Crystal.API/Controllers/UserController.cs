using Crystal.Core.Interfaces.Services;
using Crystal.Core.DTOs.Requests;
using Crystal.Core.DTOs.Responses;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Crystal.API.Authorization;
using Crystal.Core.Authorization;

namespace Crystal.API.Controllers;

[ApiController]
[Route("api/users")]
[Authorize]
public class UserController : ControllerBase
{
    private readonly IUserService m_userService;
    private readonly IPermissionService m_permissionService;

    public UserController(IUserService p_userService, IPermissionService p_permissionService)
    {
        m_userService = p_userService;
        m_permissionService = p_permissionService;
    }

    [HttpGet]
    [RequirePermission(PermissionActions.Read, PermissionSubjects.User)]
    public async Task<IActionResult> GetAllUsers(CancellationToken p_cancellationToken)
    {
        IEnumerable<UserResponse> users = await m_userService
            .GetAllUsersAsync(p_cancellationToken)
            .ConfigureAwait(false);

        return Ok(users);
    }

    [HttpGet("{p_id}")]
    [RequirePermission(PermissionActions.Read, PermissionSubjects.User)]
    public async Task<IActionResult> GetUserById(string p_id, CancellationToken p_cancellationToken)
    {
        UserResponse? user = await m_userService
            .GetUserByIdAsync(p_id, p_cancellationToken)
            .ConfigureAwait(false);

        if (user is null)
        {
            return NotFound();
        }

        return Ok(user);
    }

    [HttpGet("me/permissions")]
    public async Task<IActionResult> GetMyPermissions(CancellationToken p_cancellationToken)
    {
        string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized();
        }

        UserPermissionsResponseDto permissions = await m_permissionService
            .GetUserPermissionsAsync(userId)
            .ConfigureAwait(false);

        return Ok(permissions);
    }

    [HttpGet("me")]
    public async Task<IActionResult> GetMyProfile(CancellationToken p_cancellationToken)
    {
        string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized();
        }

        UserResponse? user = await m_userService
            .GetUserByIdAsync(userId, p_cancellationToken)
            .ConfigureAwait(false);

        if (user is null)
        {
            return NotFound();
        }

        return Ok(user);
    }

    [HttpPost]
    [RequirePermission(PermissionActions.Create, PermissionSubjects.User)]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserRequest p_request, CancellationToken p_cancellationToken)
    {
        UserResponse newUser = await m_userService
            .CreateUserAsync(p_request, p_cancellationToken)
            .ConfigureAwait(false);

        return CreatedAtAction(nameof(GetUserById), new { p_id = newUser.Id }, newUser);
    }

    [HttpPut("{p_id}")]
    [RequirePermission(PermissionActions.Update, PermissionSubjects.User)]
    public async Task<IActionResult> UpdateUser(string p_id, [FromBody] UpdateUserRequest p_request, CancellationToken p_cancellationToken)
    {
        UserResponse? updatedUser = await m_userService
            .UpdateUserAsync(p_id, p_request, p_cancellationToken)
            .ConfigureAwait(false);

        if (updatedUser is null)
        {
            return NotFound();
        }

        return Ok(updatedUser);
    }

    [HttpPut("me")]
    public async Task<IActionResult> UpdateMyProfile([FromBody] UpdateProfileRequest p_request, CancellationToken p_cancellationToken)
    {
        string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized();
        }

        UserResponse? updatedUser = await m_userService
            .UpdateProfileAsync(userId, p_request, p_cancellationToken)
            .ConfigureAwait(false);

        if (updatedUser is null)
        {
            return NotFound();
        }

        return Ok(updatedUser);
    }

    [HttpDelete("{p_id}")]
    [RequirePermission(PermissionActions.Delete, PermissionSubjects.User)]
    public async Task<IActionResult> DeleteUser(string p_id, CancellationToken p_cancellationToken)
    {
        bool isDeleted = await m_userService
            .DeleteUserAsync(p_id, p_cancellationToken)
            .ConfigureAwait(false);

        if (!isDeleted)
        {
            return NotFound();
        }

        return NoContent();
    }
}
