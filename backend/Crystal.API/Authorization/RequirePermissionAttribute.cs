using Crystal.Core.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Security.Claims;

namespace Crystal.API.Authorization;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
public sealed class RequirePermissionAttribute : Attribute, IAsyncAuthorizationFilter
{
    public string Action { get; }
    public string Subject { get; }

    public RequirePermissionAttribute(string p_action, string p_subject)
    {
        Action = p_action;
        Subject = p_subject;
    }

    public async Task OnAuthorizationAsync(AuthorizationFilterContext p_context)
    {
        if (p_context.HttpContext.User.Identity?.IsAuthenticated != true)
        {
            p_context.Result = new UnauthorizedResult();
            return;
        }

        string? userId = p_context.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId))
        {
            p_context.Result = new UnauthorizedResult();
            return;
        }

        IPermissionService permissionService = p_context.HttpContext.RequestServices
            .GetRequiredService<IPermissionService>();

        bool isAllowed = await permissionService.UserHasPermissionAsync(userId, Action, Subject);
        if (!isAllowed)
        {
            p_context.Result = new ForbidResult();
        }
    }
}
