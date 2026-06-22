using System.Net;
using System.Text.Json;
using Crystal.Core.Constants;
using Microsoft.Extensions.Hosting;

namespace Crystal.API.Middleware;

public class ErrorHandlingMiddleware
{
    private readonly RequestDelegate m_next;
    private readonly IHostEnvironment m_hostEnvironment;

    public ErrorHandlingMiddleware(RequestDelegate p_next, IHostEnvironment p_hostEnvironment)
    {
        m_next = p_next;
        m_hostEnvironment = p_hostEnvironment;
    }

    public async Task Invoke(HttpContext p_context)
    {
        try
        {
            await m_next(p_context);
        }
        catch (Exception p_ex)
        {
            int statusCode = p_ex switch
            {
                ArgumentException => (int)HttpStatusCode.BadRequest,
                KeyNotFoundException => (int)HttpStatusCode.NotFound,
                InvalidOperationException => (int)HttpStatusCode.Conflict,
                UnauthorizedAccessException => (int)HttpStatusCode.Forbidden,
                _ => (int)HttpStatusCode.InternalServerError
            };

            p_context.Response.StatusCode = statusCode;
            p_context.Response.ContentType = "application/json";

            Dictionary<string, string> responseBody;

            if (statusCode == (int)HttpStatusCode.InternalServerError)
            {
                responseBody = new Dictionary<string, string>
                {
                    ["message"] = ErrorMessages.InternalServerError,
                };

                if (m_hostEnvironment.IsDevelopment())
                {
                    responseBody["detail"] = p_ex.Message;
                }
            }
            else
            {
                responseBody = new Dictionary<string, string>
                {
                    ["message"] = p_ex.Message,
                };
            }

            await p_context.Response.WriteAsync(JsonSerializer.Serialize(responseBody));
        }
    }
}
