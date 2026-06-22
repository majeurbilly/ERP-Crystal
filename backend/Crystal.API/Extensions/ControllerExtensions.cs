using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;

namespace Crystal.API.Extensions;

public static class ControllerExtensions
{
    public static string? GetCurrentUserId(this ControllerBase p_controller)
    {
        return p_controller.User.FindFirstValue(ClaimTypes.NameIdentifier);
    }
}
