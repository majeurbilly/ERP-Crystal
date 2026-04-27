using System.Text.Json;
using Crystal.API.Middleware;
using Microsoft.AspNetCore.Http;

namespace Crystal.UnitTests.Middleware;

public class ErrorHandlingMiddlewareTests
{
    [Fact]
    public async Task Invoke_Returns_500_Json_When_Pipeline_Throws()
    {
        const string messageException = "Simulated database failure";

        RequestDelegate next = _ =>
            Task.FromException(new InvalidOperationException(messageException));

        ErrorHandlingMiddleware middleware = new(next);

        DefaultHttpContext httpContext = new();
        httpContext.Response.Body = new MemoryStream();

        await middleware.Invoke(httpContext);

        Assert.Equal(StatusCodes.Status500InternalServerError, httpContext.Response.StatusCode);
        Assert.Equal("application/json", httpContext.Response.ContentType);

        httpContext.Response.Body.Seek(0, SeekOrigin.Begin);
        using StreamReader reader = new(httpContext.Response.Body, leaveOpen: true);
        string json = await reader.ReadToEndAsync();

        using JsonDocument doc = JsonDocument.Parse(json);
        JsonElement root = doc.RootElement;

        Assert.Equal("Internal error", root.GetProperty("message").GetString());
        Assert.Equal(messageException, root.GetProperty("detail").GetString());
    }
}
