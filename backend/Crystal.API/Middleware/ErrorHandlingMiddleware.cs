using System.Net;
using System.Text.Json;
using System.Collections.Generic;

namespace Crystal.API.Middleware;

public class ErrorHandlingMiddleware
{
    private readonly RequestDelegate m_next;

    public ErrorHandlingMiddleware(RequestDelegate p_next)
    {
        m_next = p_next;
    }

    public async Task Invoke(HttpContext p_context)
    {
        try
        {
            await m_next(p_context);
        }
        catch (Exception p_ex)
        {
            p_context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            p_context.Response.ContentType = "application/json";

            Dictionary<string, string> response = new Dictionary<string, string>
            {
                ["message"] = "Internal error",
                ["detail"] = p_ex.Message
            };

            await p_context.Response.WriteAsync(JsonSerializer.Serialize(response));
        }
    }
}
