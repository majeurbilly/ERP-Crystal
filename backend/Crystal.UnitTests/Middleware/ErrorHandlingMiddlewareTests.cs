using System.Text.Json;
using Crystal.Core.Constants;
using Crystal.API.Middleware;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Moq;

namespace Crystal.UnitTests.Middleware;

public class ErrorHandlingMiddlewareTests
{
    [Fact]
    public async Task Invoke_Returns_500_Json_With_Detail_When_Development_And_Pipeline_Throws()
    {
        const string messageException = "Simulated database failure";

        RequestDelegate next = _ =>
            Task.FromException(new Exception(messageException));

        Mock<IHostEnvironment> hostEnvironmentMock = new();
        hostEnvironmentMock.Setup(p_host => p_host.EnvironmentName).Returns(Environments.Development);

        ErrorHandlingMiddleware middleware = new(next, hostEnvironmentMock.Object);

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

        Assert.Equal(ErrorMessages.InternalServerError, root.GetProperty("message").GetString());
        Assert.Equal(messageException, root.GetProperty("detail").GetString());
    }

    [Fact]
    public async Task Invoke_Returns_500_Json_Without_Detail_When_Not_Development_And_Pipeline_Throws()
    {
        RequestDelegate next = _ =>
            Task.FromException(new Exception("Secret internals"));

        Mock<IHostEnvironment> hostEnvironmentMock = new();
        hostEnvironmentMock.Setup(p_host => p_host.EnvironmentName).Returns(Environments.Production);

        ErrorHandlingMiddleware middleware = new(next, hostEnvironmentMock.Object);

        DefaultHttpContext httpContext = new();
        httpContext.Response.Body = new MemoryStream();

        await middleware.Invoke(httpContext);

        httpContext.Response.Body.Seek(0, SeekOrigin.Begin);
        using StreamReader reader = new(httpContext.Response.Body, leaveOpen: true);
        string json = await reader.ReadToEndAsync();

        using JsonDocument doc = JsonDocument.Parse(json);
        JsonElement root = doc.RootElement;

        Assert.Equal(ErrorMessages.InternalServerError, root.GetProperty("message").GetString());
        Assert.False(root.TryGetProperty("detail", out _));
    }

    [Fact]
    public async Task Invoke_Returns_400_Json_With_Exception_Message_When_ArgumentException_Thrown()
    {
        const string messageException = "Quantity cannot be negative.";

        RequestDelegate next = _ =>
            Task.FromException(new ArgumentException(messageException));

        Mock<IHostEnvironment> hostEnvironmentMock = new();
        hostEnvironmentMock.Setup(p_host => p_host.EnvironmentName).Returns(Environments.Production);

        ErrorHandlingMiddleware middleware = new(next, hostEnvironmentMock.Object);

        DefaultHttpContext httpContext = new();
        httpContext.Response.Body = new MemoryStream();

        await middleware.Invoke(httpContext);

        Assert.Equal(StatusCodes.Status400BadRequest, httpContext.Response.StatusCode);

        httpContext.Response.Body.Seek(0, SeekOrigin.Begin);
        using StreamReader reader = new(httpContext.Response.Body, leaveOpen: true);
        string json = await reader.ReadToEndAsync();

        using JsonDocument doc = JsonDocument.Parse(json);
        JsonElement root = doc.RootElement;

        Assert.Equal(messageException, root.GetProperty("message").GetString());
    }

    [Fact]
    public async Task Invoke_Returns_409_Json_With_Exception_Message_When_InvalidOperationException_Thrown()
    {
        const string messageException = "A location with this title already exists.";

        RequestDelegate next = _ =>
            Task.FromException(new InvalidOperationException(messageException));

        Mock<IHostEnvironment> hostEnvironmentMock = new();
        hostEnvironmentMock.Setup(p_host => p_host.EnvironmentName).Returns(Environments.Production);

        ErrorHandlingMiddleware middleware = new(next, hostEnvironmentMock.Object);

        DefaultHttpContext httpContext = new();
        httpContext.Response.Body = new MemoryStream();

        await middleware.Invoke(httpContext);

        Assert.Equal(StatusCodes.Status409Conflict, httpContext.Response.StatusCode);

        httpContext.Response.Body.Seek(0, SeekOrigin.Begin);
        using StreamReader reader = new(httpContext.Response.Body, leaveOpen: true);
        string json = await reader.ReadToEndAsync();

        using JsonDocument doc = JsonDocument.Parse(json);
        JsonElement root = doc.RootElement;

        Assert.Equal(messageException, root.GetProperty("message").GetString());
    }
}
