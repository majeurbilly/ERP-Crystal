using Crystal.Core.Interfaces.Services;
using Crystal.Core.DTOs.Requests;
using Crystal.Core.DTOs.Responses;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Crystal.API.Controllers;

[ApiController]
[Route("api/users")]
[Authorize]
public class UserController : ControllerBase
{
    private readonly IUserService m_userService;

    public UserController(IUserService p_userService)
    {
        m_userService = p_userService;
    }

    [HttpGet]
    [Authorize(Roles = "Admin,Gerant")]
    public async Task<IActionResult> GetAllUsers(CancellationToken p_cancellationToken)
    {
        IEnumerable<UserResponse> users = await m_userService
            .GetAllUsersAsync(p_cancellationToken)
            .ConfigureAwait(false);

        return Ok(users);
    }

    [HttpGet("metrics")]
    [Authorize(Roles = "Admin,Gerant")]
    public async Task<IActionResult> GetHrMetrics(CancellationToken p_cancellationToken)
    {
        HrMetricsResponse metrics = await m_userService
            .GetHrMetricsAsync(p_cancellationToken)
            .ConfigureAwait(false);

        return Ok(metrics);
    }

    [HttpGet("{p_id}")]
    [Authorize(Roles = "Admin,Gerant")]
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
    [Authorize(Roles = "Admin,Gerant")]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserRequest p_request, CancellationToken p_cancellationToken)
    {
        try
        {
            UserResponse newUser = await m_userService
                .CreateUserAsync(p_request, p_cancellationToken)
                .ConfigureAwait(false);

            return CreatedAtAction(nameof(GetUserById), new { p_id = newUser.Id }, newUser);
        }
        catch (InvalidOperationException p_ex)
        {
            return BadRequest(p_ex.Message);
        }
    }

    [HttpPut("{p_id}")]
    [Authorize(Roles = "Admin,Gerant")]
    public async Task<IActionResult> UpdateUser(string p_id, [FromBody] UpdateUserRequest p_request, CancellationToken p_cancellationToken)
    {
        try
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
        catch (InvalidOperationException p_ex)
        {
            return BadRequest(p_ex.Message);
        }
    }

    [HttpPut("me")]
    public async Task<IActionResult> UpdateMyProfile([FromBody] UpdateProfileRequest p_request, CancellationToken p_cancellationToken)
    {
        string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized();
        }

        try
        {
            UserResponse? updatedUser = await m_userService
                .UpdateProfileAsync(userId, p_request, p_cancellationToken)
                .ConfigureAwait(false);

            if (updatedUser is null)
            {
                return NotFound();
            }

            return Ok(updatedUser);
        }
        catch (InvalidOperationException p_ex)
        {
            return BadRequest(p_ex.Message);
        }
    }

    [HttpDelete("{p_id}")]
    [Authorize(Roles = "Admin,Gerant")]
    public async Task<IActionResult> DeleteUser(string p_id, CancellationToken p_cancellationToken)
    {
        try
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
        catch (InvalidOperationException p_ex)
        {
            return BadRequest(p_ex.Message);
        }
    }
}
