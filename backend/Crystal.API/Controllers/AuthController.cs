using Crystal.Core.Authorization;
using Crystal.Core.DTOs.Requests;
using Crystal.Core.DTOs.Responses;
using Microsoft.AspNetCore.Authorization;
using Crystal.Core.Interfaces.Services;
using Crystal.API.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Crystal.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService m_authService;

    public AuthController(IAuthService p_authService)
    {
        m_authService = p_authService;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest p_request, CancellationToken p_cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        LoginResponse? response = await m_authService.LoginAsync(p_request, p_cancellationToken).ConfigureAwait(false);

        if (response is null)
        {
            return Unauthorized(new { message = "Invalid credentials or locked account." });
        }

        return Ok(response);
    }

    [HttpPost("register")]
    [Authorize]
    [RequirePermission(PermissionActions.Create, PermissionSubjects.User)]
    public async Task<IActionResult> Register([FromBody] RegisterRequest p_request, CancellationToken p_cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        RegisterResult result = await m_authService.RegisterAsync(p_request, p_cancellationToken).ConfigureAwait(false);

        if (!result.Succeeded)
        {
            return BadRequest(new { errors = result.Errors });
        }

        return Ok(new { message = "Account created successfully." });
    }
}
